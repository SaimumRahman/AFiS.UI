/* =========================================================================
   Treasury_BankAccounts_DBChanges.sql
   -------------------------------------------------------------------------
   Seeds bank current-account rows in FinancialAccounts so the Settlement
   "Payment Receiving Bank" dropdown and the Contra "Bank Account" dropdown
   are populated. Each maps to an existing bank GL leaf:

       LedgerCode 114001  ->  Al Arafah Islami Bank (Banks.Id 5)
       LedgerCode 114002  ->  City Bank              (Banks.Id 7)
       LedgerCode 114003  ->  Eastern Bank           (Banks.Id 3)

   FinancialAccountTypeId 2 = Bank. Idempotent.
   ========================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.FinancialAccounts WHERE LedgerCode = N'114001')
    INSERT dbo.FinancialAccounts (FinancialAccountTypeId, BankId, MFSTypeId, AccountNo, Remarks, StoreId, LedgerCode)
    VALUES (2, 5, NULL, N'AIBL-CD-001', N'Al Arafah Islami Bank current account', NULL, N'114001');

IF NOT EXISTS (SELECT 1 FROM dbo.FinancialAccounts WHERE LedgerCode = N'114002')
    INSERT dbo.FinancialAccounts (FinancialAccountTypeId, BankId, MFSTypeId, AccountNo, Remarks, StoreId, LedgerCode)
    VALUES (2, 7, NULL, N'1102.0091', N'City Bank current account', NULL, N'114002');

IF NOT EXISTS (SELECT 1 FROM dbo.FinancialAccounts WHERE LedgerCode = N'114003')
    INSERT dbo.FinancialAccounts (FinancialAccountTypeId, BankId, MFSTypeId, AccountNo, Remarks, StoreId, LedgerCode)
    VALUES (2, 3, NULL, N'EBL-CD-001', N'Eastern Bank current account', NULL, N'114003');

SELECT fa.Id, fa.FinancialAccountTypeId, fa.BankId, fa.AccountNo, fa.LedgerCode, b.Name AS BankName
FROM dbo.FinancialAccounts fa
LEFT JOIN dbo.Banks b ON b.Id = fa.BankId
WHERE fa.LedgerCode IN (N'114001', N'114002', N'114003')
ORDER BY fa.LedgerCode;
GO
PRINT 'Treasury_BankAccounts_DBChanges.sql complete.';
GO
