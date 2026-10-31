/* =========================================================================
   07_acc_opening_balance.sql
   Opening Balance feature:

     1. Create the contra ledger 314001 "Opening Balance Equity" (child of the
        Equity group, code "3"). Opening-balance vouchers post the balancing
        amount to this account.
     2. Register the /OpeningBalance route so the page can be granted to groups.

   Idempotent / additive. Safe to re-run.
   Target: AFIS_MAH_LIVE   (acc_account unique key = company_id + code)
   ========================================================================= */
SET NOCOUNT ON;

/* ---- 1. Opening Balance Equity ledger ---------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_account WHERE company_id = 1 AND code = '314001')
BEGIN
    INSERT dbo.acc_account
        (company_id, parent_id, code, name, nature, is_group, requires_party, requires_branch, is_cash_bank, is_active, created_at)
    SELECT 1,
           (SELECT TOP 1 account_id FROM dbo.acc_account WHERE company_id = 1 AND code = '3' AND is_group = 1),
           '314001', N'Opening Balance Equity', 'EQUITY', 0, 0, 1, 0, 1, SYSUTCDATETIME();
END
GO

/* ---- 2. Route registration --------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = '/OpeningBalance')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Opening Balance', '/OpeningBalance', 1);
END
GO

/* ---- Verify ------------------------------------------------------------- */
SELECT 'acc 314001'            AS item, COUNT(*) AS n FROM dbo.acc_account WHERE company_id = 1 AND code = '314001'
UNION ALL
SELECT 'route /OpeningBalance', COUNT(*)      FROM dbo.Routes       WHERE RoutePath = '/OpeningBalance';
GO
