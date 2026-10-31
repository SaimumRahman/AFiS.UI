/* ============================================================================
   Supplier Payments - payment source (Cash / Financial Account / Cheque)

   Adds the columns needed to record which payment source a supplier payment
   used and which GL ledger it actually hit:

     FinancialAccountId  -> FinancialAccounts row (MFS / bank).
     PaymentLedgerCode   -> exact acc_account.code posted (cash ledger too).
     TransactionNo       -> MFS / bank transaction reference.

   The balance guard itself is computed from dbo.acc_vw_gl at save time; no
   table or trigger is disabled or dropped.

   Idempotent: safe to run more than once.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

IF COL_LENGTH('dbo.SupplierPayments', 'FinancialAccountId') IS NULL
BEGIN
    ALTER TABLE dbo.SupplierPayments ADD FinancialAccountId INT NULL;
    PRINT 'Added SupplierPayments.FinancialAccountId';
END
ELSE
    PRINT 'SupplierPayments.FinancialAccountId already exists';

IF COL_LENGTH('dbo.SupplierPayments', 'PaymentLedgerCode') IS NULL
BEGIN
    ALTER TABLE dbo.SupplierPayments ADD PaymentLedgerCode NVARCHAR(20) NULL;
    PRINT 'Added SupplierPayments.PaymentLedgerCode';
END
ELSE
    PRINT 'SupplierPayments.PaymentLedgerCode already exists';

IF COL_LENGTH('dbo.SupplierPayments', 'TransactionNo') IS NULL
BEGIN
    ALTER TABLE dbo.SupplierPayments ADD TransactionNo NVARCHAR(100) NULL;
    PRINT 'Added SupplierPayments.TransactionNo';
END
ELSE
    PRINT 'SupplierPayments.TransactionNo already exists';

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SupplierPayments_FinancialAccount')
BEGIN
    ALTER TABLE dbo.SupplierPayments WITH CHECK
        ADD CONSTRAINT FK_SupplierPayments_FinancialAccount
        FOREIGN KEY (FinancialAccountId) REFERENCES dbo.FinancialAccounts (Id);
    PRINT 'Added FK_SupplierPayments_FinancialAccount';
END
ELSE
    PRINT 'FK_SupplierPayments_FinancialAccount already exists';

GO

/* ---------------------------------------------------------------------------
   Backfill the ledger from the actual SPAY credit line of each legacy payment.
   This is audit-accurate: it records what was posted, even where the old page
   labelled a bank payment as "Cash" (the stale BankId bug).
   --------------------------------------------------------------------------- */
UPDATE sp
SET sp.PaymentLedgerCode = x.LedgerCode
FROM dbo.SupplierPayments sp
OUTER APPLY (
    SELECT TOP 1 a.code AS LedgerCode
    FROM dbo.acc_journal_entry e
    JOIN dbo.acc_journal_line l ON l.entry_id = e.entry_id AND l.credit > 0
    JOIN dbo.acc_account a ON a.account_id = l.account_id
    WHERE e.source_system = 'SPAY'
      AND (e.source_ref = 'SPAY-' + CAST(sp.Id AS NVARCHAR(20))
           OR e.source_ref LIKE 'SPAY-' + CAST(sp.Id AS NVARCHAR(20)) + '#%')
      AND e.status <> 'REVERSED'
    ORDER BY e.entry_id DESC
) x
WHERE sp.PaymentLedgerCode IS NULL
  AND x.LedgerCode IS NOT NULL;

PRINT CONCAT('Backfilled PaymentLedgerCode on ', @@ROWCOUNT, ' supplier payment row(s).');

/* Match the legacy BankId + store to a FinancialAccounts row when unambiguous. */
UPDATE sp
SET sp.FinancialAccountId = x.Id
FROM dbo.SupplierPayments sp
OUTER APPLY (
    SELECT TOP 1 fa.Id
    FROM dbo.FinancialAccounts fa
    WHERE RTRIM(fa.LedgerCode) = RTRIM(sp.PaymentLedgerCode)
      AND (fa.StoreId = sp.StoreId OR fa.StoreId IS NULL)
      AND (sp.BankId IS NULL OR fa.BankId = sp.BankId)
    ORDER BY CASE WHEN fa.StoreId = sp.StoreId THEN 0 ELSE 1 END, fa.Id
) x
WHERE sp.FinancialAccountId IS NULL
  AND x.Id IS NOT NULL;

PRINT CONCAT('Matched FinancialAccountId on ', @@ROWCOUNT, ' supplier payment row(s).');
