/* ============================================================================
   04_erp_mapping.sql — ERP <-> accounting integration glue for AFIS_MAH_LIVE
   ----------------------------------------------------------------------------
   The repository (AccountingRepository.cs) resolves accounts directly in C#
   using the ERP master tables (no acc_erp_* map tables), so this script only:
     1) adds AccVoucherEntryId on the four ERP tables the API links back to
     2) prints reconciliation/preview queries for branch / supplier / pay maps
   Idempotent - safe to re-run. Does NOT touch master data.
   ============================================================================ */
SET NOCOUNT ON;

/* ---------------------------------------------------------------------------
   1. AccVoucherEntryId (links an ERP row to its last active accounting entry)
   --------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.SalesMaster', N'AccVoucherEntryId') IS NULL
    ALTER TABLE dbo.SalesMaster ADD AccVoucherEntryId BIGINT NULL;

IF COL_LENGTH(N'dbo.Purchases', N'AccVoucherEntryId') IS NULL
    ALTER TABLE dbo.Purchases ADD AccVoucherEntryId BIGINT NULL;

IF COL_LENGTH(N'dbo.DailyExpenses', N'AccVoucherEntryId') IS NULL
    ALTER TABLE dbo.DailyExpenses ADD AccVoucherEntryId BIGINT NULL;

IF COL_LENGTH(N'dbo.SupplierPayments', N'AccVoucherEntryId') IS NULL
    ALTER TABLE dbo.SupplierPayments ADD AccVoucherEntryId BIGINT NULL;

PRINT N'[04] AccVoucherEntryId columns ensured on SalesMaster/Purchases/DailyExpenses/SupplierPayments.';

/* ---------------------------------------------------------------------------
   2. Reconciliation / preview queries (column names match the live schema)
   --------------------------------------------------------------------------- */

PRINT N'--- Stores mapped to acc_branch (blank/unknown => API defaults to HO) ---';
SELECT s.Id AS StoreId, s.Name AS StoreName, ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(s.Code, N''))), N''), N'HO') AS UsedBranch, b.name AS BranchFound
FROM dbo.Stores s
LEFT JOIN dbo.acc_branch b ON b.code = ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(s.Code, N''))), N''), N'HO')
ORDER BY s.Id;

PRINT N'--- Suppliers: party code resolution preview ---';
SELECT TOP 20 Id, Name, ShortCode,
       CASE WHEN LEN(LTRIM(RTRIM(ISNULL(ShortCode, N'')))) > 0
            THEN N'S-' + LTRIM(RTRIM(ShortCode))
            ELSE N'S-' + CAST(Id AS NVARCHAR(20)) END AS PartyCode
FROM dbo.Suppliers
ORDER BY Id;

PRINT N'--- FinancialAccounts -> GL account resolution preview ---';
SELECT fa.Id, fa.AccountNo, mt.Name AS MFS, ba.Name AS Bank,
       CASE
           WHEN fa.BankId IS NOT NULL THEN
               CASE
                   WHEN ba.Name LIKE N'%City%'            THEN N'114002'
                   WHEN ba.Name LIKE N'%Eastern%'
                     OR ba.Name LIKE N'%EBL%'             THEN N'114003'
                   WHEN ba.Name LIKE N'%Al Arafah%'
                     OR ba.Name LIKE N'%Islami%'
                     OR ba.Name LIKE N'%AIBL%'            THEN N'114001'
                   ELSE N'114001'
               END
           WHEN fa.MFSTypeId IS NOT NULL THEN N'112001'
           ELSE N'111001'
       END AS GLAccount
FROM dbo.FinancialAccounts fa
LEFT JOIN dbo.MFSTypes mt ON mt.Id = fa.MFSTypeId
LEFT JOIN dbo.Banks ba ON ba.Id = fa.BankId
ORDER BY fa.Id;

PRINT N'--- BillTypes -> GL expense account resolution preview ---';
SELECT BillTypeId, Name, IsActive,
       CASE
           WHEN Name LIKE N'%Rent%'          THEN N'530001'
           WHEN Name LIKE N'%Electric%'      THEN N'530002'
           WHEN Name LIKE N'%Water%'
             OR Name LIKE N'%WASA%'          THEN N'530003'
           WHEN Name LIKE N'%Gas%'           THEN N'530004'
           WHEN Name LIKE N'%Fuel%'
             OR Name LIKE N'%Generator%'     THEN N'530005'
           WHEN Name LIKE N'%Internet%'
             OR Name LIKE N'%Telecom%'       THEN N'530006'
           ELSE N'530001'
       END AS GLAccount
FROM dbo.BillTypes
ORDER BY BillTypeId;

PRINT N'[04] Mapping glue complete.';
GO