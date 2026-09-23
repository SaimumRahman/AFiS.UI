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
