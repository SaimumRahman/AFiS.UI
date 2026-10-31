-- ============================================================
--  AFiSDb  |  FRESH START — Delete Transactional Data Only
--  Updated: 2026-09-30  (v2 — aligned to current DB schema)
--
--  CHANGES IN v2
--    * Hangfire tables live in the [Hangfire] schema, not [dbo]
--    * Journal guard/audit triggers are suspended around the
--      accounting purge, then restored (see sections 1 and 1b)
--    * GO between every section so one failure cannot roll back
--      the whole script
--
--  PRESERVED (static / seed data — NOT touched):
--    - Company, Stores
--    - acc_company, acc_branch, acc_account, acc_account_party_type
--    - acc_voucher_type, acc_fiscal_year, acc_fiscal_period
--    - acc_party, acc_legacy_ledger_map
--    - FinancialAccounts, FinancialAccountTypes, MFSTypes, Banks
--    - Lookups: Colors, Sizes, ItemBrand, ItemOrigin, MesurementUnits,
--               MembershipType, DiscountType, BarcodeTemplate, BillTypes,
--               RequisitionStatus, RequisitionType, TransferType,
--               CouponType, TransactionType
--    - Employees, Designations, Shifts
--    - AspNetRoles / Routes / Actions / RolePermissions /
--      GroupRoutePermissions / GroupActionPermissions / core_users
--
--  DELETED: all transactional / operational tables
--
--  ⚠  Always take a full backup before running this in LIVE!
-- ============================================================

GO

-- Disable all FK constraints temporarily
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- ============================================================
--  1. ACCOUNTING TRANSACTIONS
--     (replaces old Vouchers / VoucherDetails)
--
--     acc_trg_journal_entry_guard only allows DELETE of DRAFT
--     entries, and acc_trg_journal_line_guard blocks any line
--     change once the parent entry is not DRAFT. For a bulk
--     purge these business guards are suspended here and
--     restored in section 1b.
--     acc_trg_journal_entry_audit is suspended too, so deleting
--     entries does not immediately re-populate acc_audit_log.
-- ============================================================
ALTER TABLE [dbo].[acc_journal_line]  DISABLE TRIGGER [acc_trg_journal_line_guard];
ALTER TABLE [dbo].[acc_journal_entry] DISABLE TRIGGER [acc_trg_journal_entry_guard];
ALTER TABLE [dbo].[acc_journal_entry] DISABLE TRIGGER [acc_trg_journal_entry_audit];
PRINT 'Journal guard/audit triggers suspended.';
GO

DELETE FROM [dbo].[acc_journal_line];      -- child of acc_journal_entry
DELETE FROM [dbo].[acc_journal_entry];
DELETE FROM [dbo].[acc_audit_log];
DELETE FROM [dbo].[acc_voucher_sequence];  -- resets voucher numbering
--DELETE FROM [dbo].[TransactionType]     -- lookup; re-seed if needed
PRINT 'Accounting transactions cleared.';
GO

-- ============================================================
--  1b. RESTORE ACCOUNTING TRIGGERS (keep these ON in production)
-- ============================================================
ALTER TABLE [dbo].[acc_journal_line]  ENABLE TRIGGER [acc_trg_journal_line_guard];
ALTER TABLE [dbo].[acc_journal_entry] ENABLE TRIGGER [acc_trg_journal_entry_guard];
ALTER TABLE [dbo].[acc_journal_entry] ENABLE TRIGGER [acc_trg_journal_entry_audit];
PRINT 'Journal guard/audit triggers restored.';
GO

-- ============================================================
--  2. SUPPLIER PAYMENTS
-- ============================================================
DELETE FROM [dbo].[SupplierPaymentDetails];
DELETE FROM [dbo].[SupplierPayments];
GO

-- ============================================================
--  3. PURCHASES & PURCHASE ORDERS
-- ============================================================
DELETE FROM [dbo].[PurchaseReturnItems];
DELETE FROM [dbo].[PurchaseReturns];
DELETE FROM [dbo].[PurchaseItems];
DELETE FROM [dbo].[Purchases];
DELETE FROM [dbo].[PurchaseDraftItems];
DELETE FROM [dbo].[PurchaseDrafts];
DELETE FROM [dbo].[PurchaseOrderItems];
DELETE FROM [dbo].[PurchaseOrders];
GO

-- ============================================================
--  4. STOCK, TRANSFERS, SALES
-- ============================================================
DELETE FROM [dbo].[StockDetail];
DELETE FROM [dbo].[StockMaster];
DELETE FROM [dbo].[TransferDetail];
DELETE FROM [dbo].[TransferMaster];
DELETE FROM [dbo].[SalesDetails];
DELETE FROM [dbo].[SalesMaster];
GO

-- ============================================================
--  5. REQUISITIONS
-- ============================================================
DELETE FROM [dbo].[InvRequisitionDetail];
DELETE FROM [dbo].[InvRequisitionMaster];
-- DELETE FROM [dbo].[RequisitionStatus]
-- DELETE FROM [dbo].[RequisitionType]
GO

-- ============================================================
--  6. ITEMS / CATALOGUE / BARCODES
-- ============================================================
DELETE FROM [dbo].[ItemWiseFeature];
DELETE FROM [dbo].[ItemImages];
DELETE FROM [dbo].[ItemFeatures];
DELETE FROM [dbo].[ItemCatalogue];
DELETE FROM [dbo].[Barcodes];
DELETE FROM [dbo].[BarcodePrintConfig];
--DELETE FROM [dbo].[BarcodeTemplate]     -- seed
DELETE FROM [dbo].[Items];
GO

-- ============================================================
--  7. ITEM LOOKUP / CLASSIFICATION TABLES
--     (delete only if you want a clean slate for these too;
--      comment out any you want to keep)
-- ============================================================
--DELETE FROM [dbo].[ItemBrand]
--DELETE FROM [dbo].[ItemOrigin]
--DELETE FROM [dbo].[Colors]
--DELETE FROM [dbo].[Sizes]
--DELETE FROM [dbo].[Designs]
DELETE FROM [dbo].[DiscountManagerDetails];
DELETE FROM [dbo].[DiscountManager];
--DELETE FROM [dbo].[DiscountType]
--DELETE FROM [dbo].[MesurementUnits]
--DELETE FROM [dbo].[MembershipType]
GO

-- ============================================================
--  8. CUSTOMERS, SUPPLIERS, EMPLOYEES
-- ============================================================
DELETE FROM [dbo].[CustomerDetails];
DELETE FROM [dbo].[Suppliers];
--DELETE FROM [dbo].[Employees]
--DELETE FROM [dbo].[Designations]
--DELETE FROM [dbo].[Shifts]
GO

-- ============================================================
--  9. USERS & AUTH  (clears logins — keep if you want admin)
-- ============================================================
--DELETE FROM [dbo].[core_refreshtoken]
--DELETE FROM [dbo].[AspNetUserClaims]
--DELETE FROM [dbo].[AspNetUserLogins]
--DELETE FROM [dbo].[AspNetUserRoles]
--DELETE FROM [dbo].[UserGroups]
--DELETE FROM [dbo].[GroupsRole]
--DELETE FROM [dbo].[core_users] where UserId <>1
GO

-- ============================================================
-- 10. COUPONS, EXPENSES, PAYMENTS, STORE ACCOUNT MAPPING
-- ============================================================
DELETE FROM [dbo].[CouponItems];
DELETE FROM [dbo].[Coupons];
DELETE FROM [dbo].[CouponCustomerBind];
DELETE FROM [dbo].[CouponUsageLog];
DELETE FROM [dbo].[DailyExpenses];
DELETE FROM [dbo].[PaymentTransaction];
DELETE FROM [dbo].[StoreAccounts];   -- store <-> financial account mapping
DELETE FROM [dbo].[BatchNoSeq];
GO

-- ============================================================
-- 11. BACKGROUND JOBS (Hangfire) — transient operational data.
--      NOTE: these live in the [Hangfire] schema, NOT [dbo].
--      Schema/Server/List/Set/Hash are kept (job infra/config).
-- ============================================================
DELETE FROM [Hangfire].[JobParameter];
DELETE FROM [Hangfire].[State];
DELETE FROM [Hangfire].[JobQueue];
DELETE FROM [Hangfire].[Job];
DELETE FROM [Hangfire].[Counter];
DELETE FROM [Hangfire].[AggregatedCounter];
GO

-- ============================================================
-- 12. SEQUENCES — reset counters to 1
--      (SQ_DesignCode / SQ_GroupCode / SQ_SubGroupCode back PRESERVED
--       master tables, so they are deliberately left alone)
-- ============================================================
ALTER SEQUENCE [dbo].[ProductSerialSequence]  RESTART WITH 1;
ALTER SEQUENCE [dbo].[RequisitionSequence]    RESTART WITH 1;
GO

-- ============================================================
-- 13. RESEED IDENTITY COLUMNS on cleared tables
--      (only tables that actually have an identity column)
-- ============================================================
DBCC CHECKIDENT ('[dbo].[acc_journal_line]',        RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[acc_journal_entry]',       RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[acc_audit_log]',           RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[SupplierPaymentDetails]',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[SupplierPayments]',        RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseReturnItems]',     RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseReturns]',         RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseItems]',           RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[Purchases]',               RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseDraftItems]',      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseDrafts]',          RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseOrderItems]',      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PurchaseOrders]',          RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[StockDetail]',             RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[StockMaster]',             RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[TransferDetail]',          RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[TransferMaster]',          RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[SalesDetails]',            RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[SalesMaster]',             RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[InvRequisitionDetail]',    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[InvRequisitionMaster]',    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[Barcodes]',                RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[BarcodePrintConfig]',      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[Items]',                   RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[ItemCatalogue]',           RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[DiscountManager]',         RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[DiscountManagerDetails]',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[CustomerDetails]',         RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[Suppliers]',               RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[Coupons]',                 RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[CouponItems]',             RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[DailyExpenses]',           RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[PaymentTransaction]',      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('[dbo].[StoreAccounts]',           RESEED, 0) WITH NO_INFOMSGS;
GO

-- NOT reseeded (no identity column): Colors, Sizes, ItemOrigin,
-- ItemFeatures, ItemImages, ItemWiseFeature, Employees,
-- Designations, Shifts, ItemBrand, acc_party, acc_account

-- ============================================================
-- 14. Re-enable all FK constraints
-- ============================================================
EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

-- ============================================================
-- PRESERVED (untouched) — verify row counts:
-- ============================================================
SELECT 'Company'        AS [Table], COUNT(*) AS [Rows] FROM [dbo].[Company]
UNION ALL
SELECT 'Stores',                    COUNT(*) FROM [dbo].[Stores]
UNION ALL
SELECT 'acc_company',               COUNT(*) FROM [dbo].[acc_company]
UNION ALL
SELECT 'acc_branch',                COUNT(*) FROM [dbo].[acc_branch]
UNION ALL
SELECT 'acc_account',               COUNT(*) FROM [dbo].[acc_account]
UNION ALL
SELECT 'acc_account_party_type',    COUNT(*) FROM [dbo].[acc_account_party_type]
UNION ALL
SELECT 'acc_voucher_type',          COUNT(*) FROM [dbo].[acc_voucher_type]
UNION ALL
SELECT 'acc_party',                 COUNT(*) FROM [dbo].[acc_party]
UNION ALL
SELECT 'acc_fiscal_year',           COUNT(*) FROM [dbo].[acc_fiscal_year]
UNION ALL
SELECT 'acc_fiscal_period',         COUNT(*) FROM [dbo].[acc_fiscal_period]
UNION ALL
SELECT 'FinancialAccounts',         COUNT(*) FROM [dbo].[FinancialAccounts]
UNION ALL
SELECT 'MFSTypes',                  COUNT(*) FROM [dbo].[MFSTypes]
UNION ALL
SELECT 'Banks',                     COUNT(*) FROM [dbo].[Banks]
UNION ALL
SELECT 'AspNetRoles',               COUNT(*) FROM [dbo].[AspNetRoles]
UNION ALL
SELECT 'Routes',                    COUNT(*) FROM [dbo].[Routes]
UNION ALL
SELECT 'Actions',                   COUNT(*) FROM [dbo].[Actions]
UNION ALL
SELECT 'Groups',                    COUNT(*) FROM [dbo].[Groups]
UNION ALL
SELECT 'SubGroups',                 COUNT(*) FROM [dbo].[SubGroups]
UNION ALL
SELECT 'GroupRoutePermissions',     COUNT(*) FROM [dbo].[GroupRoutePermissions]
UNION ALL
SELECT 'GroupActionPermissions',    COUNT(*) FROM [dbo].[GroupActionPermissions]
GO

-- ============================================================
-- TRANSACTIONAL — verify all are now 0 rows
-- ============================================================
SELECT 'acc_journal_line'      AS [Table], COUNT(*) AS [Rows] FROM [dbo].[acc_journal_line]
UNION ALL SELECT 'acc_journal_entry',      COUNT(*) FROM [dbo].[acc_journal_entry]
UNION ALL SELECT 'acc_audit_log',          COUNT(*) FROM [dbo].[acc_audit_log]
UNION ALL SELECT 'acc_voucher_sequence',   COUNT(*) FROM [dbo].[acc_voucher_sequence]
UNION ALL SELECT 'Purchases',              COUNT(*) FROM [dbo].[Purchases]
UNION ALL SELECT 'PurchaseOrders',         COUNT(*) FROM [dbo].[PurchaseOrders]
UNION ALL SELECT 'StockMaster',            COUNT(*) FROM [dbo].[StockMaster]
UNION ALL SELECT 'StockDetail',            COUNT(*) FROM [dbo].[StockDetail]
UNION ALL SELECT 'TransferMaster',         COUNT(*) FROM [dbo].[TransferMaster]
UNION ALL SELECT 'SalesMaster',            COUNT(*) FROM [dbo].[SalesMaster]
UNION ALL SELECT 'SalesDetails',           COUNT(*) FROM [dbo].[SalesDetails]
UNION ALL SELECT 'InvRequisitionMaster',   COUNT(*) FROM [dbo].[InvRequisitionMaster]
UNION ALL SELECT 'InvRequisitionDetail',   COUNT(*) FROM [dbo].[InvRequisitionDetail]
UNION ALL SELECT 'Items',                  COUNT(*) FROM [dbo].[Items]
UNION ALL SELECT 'Barcodes',               COUNT(*) FROM [dbo].[Barcodes]
UNION ALL SELECT 'ItemCatalogue',          COUNT(*) FROM [dbo].[ItemCatalogue]
UNION ALL SELECT 'CustomerDetails',        COUNT(*) FROM [dbo].[CustomerDetails]
UNION ALL SELECT 'Suppliers',              COUNT(*) FROM [dbo].[Suppliers]
UNION ALL SELECT 'DiscountManager',        COUNT(*) FROM [dbo].[DiscountManager]
UNION ALL SELECT 'DiscountManagerDetails', COUNT(*) FROM [dbo].[DiscountManagerDetails]
UNION ALL SELECT 'Coupons',                COUNT(*) FROM [dbo].[Coupons]
UNION ALL SELECT 'DailyExpenses',          COUNT(*) FROM [dbo].[DailyExpenses]
UNION ALL SELECT 'PaymentTransaction',     COUNT(*) FROM [dbo].[PaymentTransaction]
UNION ALL SELECT 'StoreAccounts',          COUNT(*) FROM [dbo].[StoreAccounts]
UNION ALL SELECT 'SupplierPayments',       COUNT(*) FROM [dbo].[SupplierPayments]
UNION ALL SELECT 'Hangfire.Job',           COUNT(*) FROM [Hangfire].[Job]
UNION ALL SELECT 'Hangfire.AggregatedCounter', COUNT(*) FROM [Hangfire].[AggregatedCounter]
GO

-- ============================================================
-- SANITY: all accounting triggers must be enabled again
-- ============================================================
SELECT tr.name AS TriggerName, tr.is_disabled AS IsDisabled
FROM sys.triggers tr
WHERE tr.parent_class = 1
  AND OBJECT_NAME(tr.parent_id) IN ('acc_journal_entry', 'acc_journal_line')
ORDER BY tr.name;
GO