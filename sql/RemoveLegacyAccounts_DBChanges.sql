/* =============================================================
   Remove legacy Accounts / Vouchers system (API+UI cleanup)
   Drops: AccountsGroups, Accounts, Vouchers, VoucherDetails,
          StoreAccountsMapping  +  orphaned account/voucher columns
   -----------------------------------------------------------------
   APPLY MANUALLY against AFIS_MAH_LIVE AFTER the API + UI code
   changes are deployed. Take a DB backup first.
   NOTE: The FK list below is informational. The ONLY real FK on
   these tables is Accounts.AccountsGroupId -> AccountsGroups.
   acc_account / acc_account_party_type are separate legacy tables
   and are intentionally left untouched.
   ============================================================= */

BEGIN TRANSACTION;

-- 1. Drop orphaned view/columns (no FK constraints reference them)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Suppliers') AND name = 'AccountId')
    ALTER TABLE dbo.Suppliers DROP COLUMN AccountId;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Purchases') AND name = 'VoucherId')
    ALTER TABLE dbo.Purchases DROP COLUMN VoucherId;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PurchaseReturns') AND name = 'VoucherId')
    ALTER TABLE dbo.PurchaseReturns DROP COLUMN VoucherId;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.SupplierPayments') AND name = 'VoucherId')
    ALTER TABLE dbo.SupplierPayments DROP COLUMN VoucherId;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Employees') AND name = 'AccountId')
    ALTER TABLE dbo.Employees DROP COLUMN AccountId;

-- 2. Drop legacy account/voucher tables (child tables first)
IF OBJECT_ID('dbo.VoucherDetails', 'U') IS NOT NULL DROP TABLE dbo.VoucherDetails;
IF OBJECT_ID('dbo.Vouchers', 'U') IS NOT NULL DROP TABLE dbo.Vouchers;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;
IF OBJECT_ID('dbo.AccountsGroups', 'U') IS NOT NULL DROP TABLE dbo.AccountsGroups;
IF OBJECT_ID('dbo.StoreAccountsMapping', 'U') IS NOT NULL DROP TABLE dbo.StoreAccountsMapping;

COMMIT TRANSACTION;

PRINT 'Legacy Accounts/Vouchers removal applied.';