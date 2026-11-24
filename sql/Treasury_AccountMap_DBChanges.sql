/* =========================================================================
   Treasury_AccountMap_DBChanges.sql
   -------------------------------------------------------------------------
   Adds the petty-cash / payment-ledger columns on ERP tables and ensures the
   GL leaves the new data-entry pages post to. Idempotent.

   IMPORTANT — this chart does NOT use 1000/2000/5000 as parent codes. The
   real tree (acc_account) is:

       1 Assets
         11 Current Assets
           1110 Cash-in-Hand          -> 111001 Cash, 111002 Petty Cash
           1120 Mobile Financial Services
           1130 Card/POS Clearing
           1140 Bank Accounts
           1150 Staff & Worker Advances/Loans
           1160 Deposits
           1170 Loans Given           -> 117001 Loans Given (Receivable)
           1190 Trade Receivables
       2 Liabilities
         21 Current Liabilities
         22 Borrowings                -> 220001..220003
       5 Expenses
         55 Financial Charges         -> 550002 MFS / POS Charges

   So the treasury pages map onto:
       Cash                    -> 111001  (existing)
       Petty Cash              -> 111002  (existing)
       Cash in Transit         -> 111003  (NEW, under 1110)
       Due from Branches       -> 117002  (NEW, under 1170 Loans Given)
       Due to Branches         -> 220004  (NEW, under 22 Borrowings)
       POS / MFS Commission    -> 550002  (existing "MFS / POS Charges")
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

/* ---- 1. Stores: petty cash ledger + cap -------------------------------- */
IF COL_LENGTH(N'dbo.Stores', N'PettyCashLedgerAccountId') IS NULL
BEGIN
    ALTER TABLE dbo.Stores ADD PettyCashLedgerAccountId INT NULL;
    PRINT 'Stores.PettyCashLedgerAccountId added.';
END
ELSE
    PRINT 'Stores.PettyCashLedgerAccountId already exists - no change.';
GO

IF COL_LENGTH(N'dbo.Stores', N'PettyCashLimit') IS NULL
BEGIN
    ALTER TABLE dbo.Stores ADD PettyCashLimit DECIMAL(18,2) NULL;
    PRINT 'Stores.PettyCashLimit added.';
END
ELSE
    PRINT 'Stores.PettyCashLimit already exists - no change.';
GO

/* ---- 2. DailyExpenses: payment ledger + petty flag --------------------- */
IF COL_LENGTH(N'dbo.DailyExpenses', N'PaymentLedgerCode') IS NULL
BEGIN
    ALTER TABLE dbo.DailyExpenses ADD PaymentLedgerCode NVARCHAR(20) NULL;
    PRINT 'DailyExpenses.PaymentLedgerCode added.';
END
ELSE
    PRINT 'DailyExpenses.PaymentLedgerCode already exists - no change.';
GO

IF COL_LENGTH(N'dbo.DailyExpenses', N'IsPettyCash') IS NULL
BEGIN
    ALTER TABLE dbo.DailyExpenses ADD IsPettyCash BIT NOT NULL DEFAULT 0;
    PRINT 'DailyExpenses.IsPettyCash added.';
END
ELSE
    PRINT 'DailyExpenses.IsPettyCash already exists - no change.';
GO

/* ---- 3. GL leaves (only those absent are inserted) --------------------- */
DECLARE @companyId SMALLINT = (SELECT company_id FROM dbo.acc_company WHERE code = N'AFHO');
IF @companyId IS NULL
BEGIN
    PRINT 'Company AFHO not found - skipping leaf creation.';
    RETURN;
END

DECLARE @cashInHandParent INT = (SELECT account_id FROM dbo.acc_account WHERE company_id = @companyId AND code = N'1110');
DECLARE @loansGivenParent INT = (SELECT account_id FROM dbo.acc_account WHERE company_id = @companyId AND code = N'1170');
DECLARE @borrowingsParent INT = (SELECT account_id FROM dbo.acc_account WHERE company_id = @companyId AND code = N'22');

IF @cashInHandParent IS NULL OR @loansGivenParent IS NULL OR @borrowingsParent IS NULL
BEGIN
    PRINT 'Required parent accounts (1110 / 1170 / 22) not found - skipping leaf creation.';
    RETURN;
END

-- Cash in Transit (under 1110 Cash-in-Hand)
IF NOT EXISTS (SELECT 1 FROM dbo.acc_account WHERE company_id = @companyId AND code = N'111003')
BEGIN
    INSERT INTO dbo.acc_account (company_id, code, name, nature, parent_id, is_group, requires_party, requires_branch, is_cash_bank, is_active)
    VALUES (@companyId, N'111003', N'Cash in Transit', N'ASSET', @cashInHandParent, 0, 0, 1, 0, 1);
    PRINT 'Leaf 111003 Cash in Transit created.';
END
ELSE
    PRINT 'Leaf 111003 already exists - no change.';

-- Due from Branches (under 1170 Loans Given)
IF NOT EXISTS (SELECT 1 FROM dbo.acc_account WHERE company_id = @companyId AND code = N'117002')
BEGIN
    INSERT INTO dbo.acc_account (company_id, code, name, nature, parent_id, is_group, requires_party, requires_branch, is_cash_bank, is_active)
    VALUES (@companyId, N'117002', N'Due from Branches', N'ASSET', @loansGivenParent, 0, 0, 1, 0, 1);
    PRINT 'Leaf 117002 Due from Branches created.';
END
ELSE
    PRINT 'Leaf 117002 already exists - no change.';

-- Due to Branches (under 22 Borrowings)
IF NOT EXISTS (SELECT 1 FROM dbo.acc_account WHERE company_id = @companyId AND code = N'220004')
BEGIN
    INSERT INTO dbo.acc_account (company_id, code, name, nature, parent_id, is_group, requires_party, requires_branch, is_cash_bank, is_active)
    VALUES (@companyId, N'220004', N'Due to Branches', N'LIABILITY', @borrowingsParent, 0, 0, 1, 0, 1);
    PRINT 'Leaf 220004 Due to Branches created.';
END
ELSE
    PRINT 'Leaf 220004 already exists - no change.';

-- Point every store's petty-cash ledger at the existing 111002 Petty Cash leaf
-- (unless the store already has an explicit petty-cash ledger configured).
UPDATE dbo.Stores
SET PettyCashLedgerAccountId = (SELECT account_id FROM dbo.acc_account WHERE company_id = @companyId AND code = N'111002')
WHERE PettyCashLedgerAccountId IS NULL;

PRINT 'Treasury GL leaves ensured (only absent ones were inserted); stores petty-cash ledger set to 111002.';
GO

/* ---- 4. Verify ----------------------------------------------- */
SELECT a.code, a.name, a.nature, p.code AS parent
FROM dbo.acc_account a
LEFT JOIN dbo.acc_account p ON p.account_id = a.parent_id
WHERE a.code IN (N'111001',N'111002',N'111003',N'117002',N'220004',N'550002')
ORDER BY a.code;

SELECT s.Id AS StoreId, s.Name, s.PettyCashLedgerAccountId, a.code AS PettyCashCode
FROM dbo.Stores s
LEFT JOIN dbo.acc_account a ON a.account_id = s.PettyCashLedgerAccountId
ORDER BY s.Id;
GO
PRINT 'Treasury_AccountMap_DBChanges.sql complete.';
GO
