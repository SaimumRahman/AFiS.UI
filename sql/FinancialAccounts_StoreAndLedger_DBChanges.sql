/* =========================================================================
   FinancialAccounts_StoreAndLedger_DBChanges.sql
   -------------------------------------------------------------------------
   Date   : 2026-10-19
   Target : AFIS_MAH_LIVE

   Purpose
   -------
   Bind each Financial Account (used by the Sales POS payment dialog) to:
     * ONE store          -> FinancialAccounts.StoreId      (FK -> Stores.Id)
     * ONE general-ledger -> FinancialAccounts.LedgerCode    (NVARCHAR(20))

   The POS payment dialog now lists only the accounts of the sale's store, and
   MFS / Bank payments post to the ledger attached here (LedgerCode). When
   LedgerCode is NULL the API falls back to the legacy name-matching resolver.

   Both columns are NULLable on purpose:
     - Mandatory is enforced by the form + API, not the schema.
     - No unique index / constraint (an account is not forced to be unique).
     - Existing rows are left untouched (no backfill).

   Idempotent: safe to run multiple times.
   ========================================================================= */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/* ---- 1. FinancialAccounts.StoreId ------------------------------------- */
IF COL_LENGTH('dbo.FinancialAccounts', 'StoreId') IS NULL
BEGIN
    ALTER TABLE dbo.FinancialAccounts ADD StoreId INT NULL;
    PRINT 'FinancialAccounts.StoreId added.';
END
ELSE
    PRINT 'FinancialAccounts.StoreId already exists - no change.';
GO

/* ---- 2. FinancialAccounts.LedgerCode ---------------------------------- */
IF COL_LENGTH('dbo.FinancialAccounts', 'LedgerCode') IS NULL
BEGIN
    ALTER TABLE dbo.FinancialAccounts ADD LedgerCode NVARCHAR(20) NULL;
    PRINT 'FinancialAccounts.LedgerCode added.';
END
ELSE
    PRINT 'FinancialAccounts.LedgerCode already exists - no change.';
GO

/* ---- 3. Store foreign key --------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_FinancialAccounts_Store')
BEGIN
    ALTER TABLE dbo.FinancialAccounts
        ADD CONSTRAINT FK_FinancialAccounts_Store
        FOREIGN KEY (StoreId) REFERENCES dbo.Stores (Id);
    PRINT 'FK_FinancialAccounts_Store added.';
END
ELSE
    PRINT 'FK_FinancialAccounts_Store already exists - no change.';
GO

/* =========================================================================
   4. Data (mappings applied through the Financial Accounts page)

   These are OPTIONAL data updates - run only the lines you need.
   Ledger meanings (12 cash/bank leaves under parent 6):
     111001 Cash-in-Hand , 111002 Cash  ...
     112001..112004  MFS  (bKash / Nagad / Rocket / ...)
     113001..113003  Card / POS clearing
     114001..114003  Bank accounts
   ========================================================================= */

/* 4a. APPLIED on 2026-10-19: bKash-916 -> Store 3, ledger 112001 (MFS) */
UPDATE dbo.FinancialAccounts
SET    StoreId    = 3,
       LedgerCode = '112001'
WHERE  Id = 3;
GO

/* 4b. STILL TO MAP (currently NULL) - uncomment once the ledger is decided:
   -- 0181020012183 (Bank: City Bank PLC) -> e.g. ledger 114002
   UPDATE dbo.FinancialAccounts SET StoreId = 4, LedgerCode = '114002' WHERE Id = 1;

   -- 01968377913 (MFS) -> e.g. ledger 112004
   UPDATE dbo.FinancialAccounts SET StoreId = 4, LedgerCode = '112004' WHERE Id = 2;
*/

/* =========================================================================
   5. Verify
   ========================================================================= */
SELECT  fa.Id,
        fa.FinancialAccountTypeId,
        fa.MFSTypeId,
        fa.BankId,
        fa.AccountNo,
        fa.StoreId,
        s.Name  AS StoreName,
        fa.LedgerCode
FROM    dbo.FinancialAccounts fa
LEFT JOIN dbo.Stores s ON s.Id = fa.StoreId
ORDER BY fa.Id;
GO
