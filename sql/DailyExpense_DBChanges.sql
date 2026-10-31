-- ============================================================================
-- Daily Expense module - database changes
-- Target: AFIS_MAH_LIVE (idempotent - safe to re-run)
-- ============================================================================

-- 1. Bill type lookup table (source of the Bill Type dropdown) ---------------
IF OBJECT_ID('dbo.BillTypes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BillTypes
    (
        BillTypeId  INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100)  NOT NULL,
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedDate DATETIME       NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Seed bill types ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.BillTypes)
BEGIN
    INSERT INTO dbo.BillTypes (Name)
    VALUES
        (N'Rent'),
        (N'Salary / Wages'),
        (N'Electricity'),
        (N'Gas'),
        (N'Water'),
        (N'Internet & Phone'),
        (N'Transport & Fuel'),
        (N'Repair & Maintenance'),
        (N'Office & Cleaning'),
        (N'Bank Charges'),
        (N'Refreshment'),
        (N'Others');
END
GO

-- 3. Daily expense entries ---------------------------------------------------
IF OBJECT_ID('dbo.DailyExpenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DailyExpenses
    (
        DailyExpenseId    INT IDENTITY(1,1) PRIMARY KEY,
        ExpenseDate       DATETIME        NOT NULL DEFAULT GETDATE(),
        BillTypeId        INT             NOT NULL,
        StoreId           INT             NOT NULL,
        FinancialAccountId INT            NULL,   -- NULL = Cash
        Amount            DECIMAL(18,2)   NOT NULL,
        Description       NVARCHAR(500)   NOT NULL,
        ReceiptBillNo     NVARCHAR(50)    NULL,
        ReferenceNo       NVARCHAR(50)    NULL,
        IsDeleted         BIT             NOT NULL DEFAULT 0,
        CreatedBy         INT             NOT NULL,
        CreatedDate       DATETIME        NOT NULL DEFAULT GETDATE(),
        UpdatedBy         INT             NULL,
        UpdatedDate       DATETIME        NULL,

        CONSTRAINT FK_DailyExpenses_BillTypes
            FOREIGN KEY (BillTypeId) REFERENCES dbo.BillTypes(BillTypeId),
        CONSTRAINT FK_DailyExpenses_FinancialAccounts
            FOREIGN KEY (FinancialAccountId) REFERENCES dbo.FinancialAccounts(Id)
    );

    CREATE INDEX IX_DailyExpenses_ExpenseDate ON dbo.DailyExpenses (ExpenseDate);
    CREATE INDEX IX_DailyExpenses_Store_Date   ON dbo.DailyExpenses (StoreId, ExpenseDate);
END
GO

-- 4. Selected expense ledger (Chart of Accounts -> acc_account.account_id) -----
--    Required on every new expense; posting always debits this account.
IF COL_LENGTH('dbo.DailyExpenses', 'LedgerAccountId') IS NULL
BEGIN
    ALTER TABLE dbo.DailyExpenses ADD LedgerAccountId INT NULL;
END
GO

-- 5. Deferred ledger posting (Post Ledger page) --------------------------------
--    Expenses are no longer posted to the GL on save. They stay unposted until
--    they are posted from the Post Ledger page, which writes the voucher and
--    sets these flags.
IF COL_LENGTH('dbo.DailyExpenses', 'AccVoucherEntryId') IS NULL
BEGIN
    -- Usually created by sql\Accounting\04_erp_mapping.sql; ensured here so this
    -- script can be applied on its own.
    ALTER TABLE dbo.DailyExpenses ADD AccVoucherEntryId BIGINT NULL;
END
GO

IF COL_LENGTH('dbo.DailyExpenses', 'IsPosted') IS NULL
BEGIN
    ALTER TABLE dbo.DailyExpenses ADD IsPosted  BIT      NOT NULL DEFAULT 0;
    ALTER TABLE dbo.DailyExpenses ADD PostedAt  DATETIME NULL;
    ALTER TABLE dbo.DailyExpenses ADD PostedBy  INT      NULL;
END
GO

-- 5.1 Backfill: any expense that already carries a voucher link was posted by
--     the old save-time flow, so mark it as posted to keep history intact.
UPDATE dbo.DailyExpenses
   SET IsPosted = 1
 WHERE AccVoucherEntryId IS NOT NULL
   AND IsPosted = 0;
GO

-- 5.2 Index to support the Post Ledger page (unposted expenses for a day).
IF OBJECT_ID('dbo.IX_DailyExpenses_Posted', 'IX') IS NULL
BEGIN
    CREATE INDEX IX_DailyExpenses_Posted ON dbo.DailyExpenses (IsPosted, ExpenseDate);
END
GO

-- 6. Bill Type removed from the Daily Expense feature -------------------------
--    The dropdown and its posting fallback are gone; the expense is now driven
--    by LedgerAccountId alone. The column (and FK) are kept so historical rows
--    retain their bill type, but it is dropped to NULL for new rows.
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.DailyExpenses')
             AND name = 'BillTypeId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.DailyExpenses ALTER COLUMN BillTypeId INT NULL;
END
GO
