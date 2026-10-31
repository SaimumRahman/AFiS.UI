/* ============================================================================
   Counter Day-End Closing (Cash Reconciliation) — AFIS_MAH_LIVE

   Adds:
     dbo.fn_BusinessDate        business day = (moment - 6h).date
     dbo.SalesCounters          physical POS counters per store
     dbo.CounterClosings        one close per counter per business day
     dbo.CounterCashMovements   cash in/out declared at closing time
     SalesMaster.CounterId      which counter rang the sale
     SalesMaster.BusinessDate   the business day the sale was assigned to
     Routes row + grants for /CounterClosing

   Design notes
     * Business day runs 06:00:00 -> 05:59:59 next calendar day.
     * Closing is per (counter, business day). A reopened closing releases the
       counter, so the unique index is filtered to Status = 'CLOSED'.
     * SalesMaster.BusinessDate is stored (not derived at read time) because a
       sale rung before 06:00 on an already-closed day is pushed to the next
       open business day; that override must survive later reads.
     * Idempotent: safe to run more than once. No trigger is disabled or dropped.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

/* -------------------------------------------------------------------------- */
/* 1. Business-day helper                                                      */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.fn_BusinessDate', 'FN') IS NULL
BEGIN
    EXEC('
        CREATE FUNCTION dbo.fn_BusinessDate(@moment DATETIME)
        RETURNS DATE
        AS
        BEGIN
            -- 06:00 boundary: 02:00 on Oct 26 -> Oct 25
            RETURN CAST(DATEADD(HOUR, -6, @moment) AS DATE);
        END');
    PRINT 'Created dbo.fn_BusinessDate';
END
ELSE
    PRINT 'dbo.fn_BusinessDate already exists';
GO

/* -------------------------------------------------------------------------- */
/* 2. Counters                                                                 */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.SalesCounters', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesCounters
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesCounters PRIMARY KEY,
        StoreId     INT               NOT NULL,
        Name        NVARCHAR(100)     NOT NULL,
        Code        NVARCHAR(20)      NULL,
        IsDefault   BIT               NOT NULL CONSTRAINT DF_SalesCounters_IsDefault DEFAULT (0),
        IsActive    BIT               NOT NULL CONSTRAINT DF_SalesCounters_IsActive  DEFAULT (1),
        CreatedOn   DATETIME          NOT NULL CONSTRAINT DF_SalesCounters_CreatedOn DEFAULT (GETDATE()),
        CONSTRAINT FK_SalesCounters_Stores FOREIGN KEY (StoreId) REFERENCES dbo.Stores (Id)
    );
    PRINT 'Created dbo.SalesCounters';
END
ELSE
    PRINT 'dbo.SalesCounters already exists';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_SalesCounters_DefaultPerStore'
                                             AND object_id = OBJECT_ID('dbo.SalesCounters'))
BEGIN
    CREATE UNIQUE INDEX UX_SalesCounters_DefaultPerStore
        ON dbo.SalesCounters (StoreId)
        WHERE IsDefault = 1;
    PRINT 'Created UX_SalesCounters_DefaultPerStore';
END
GO

/* One default counter per store so the POS can attribute sales before the
   admin names the real counters. */
INSERT INTO dbo.SalesCounters (StoreId, Name, Code, IsDefault, IsActive)
SELECT s.Id, N'Counter 1', N'C1', 1, 1
FROM dbo.Stores s
WHERE NOT EXISTS (SELECT 1 FROM dbo.SalesCounters c WHERE c.StoreId = s.Id AND c.IsDefault = 1);
PRINT CONCAT('Seeded default counters: ', @@ROWCOUNT);
GO

/* -------------------------------------------------------------------------- */
/* 3. SalesMaster: counter + assigned business day                             */
/* -------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.SalesMaster', 'CounterId') IS NULL
BEGIN
    ALTER TABLE dbo.SalesMaster ADD CounterId INT NULL;
    PRINT 'Added SalesMaster.CounterId';
END
ELSE
    PRINT 'SalesMaster.CounterId already exists';
GO

IF COL_LENGTH('dbo.SalesMaster', 'BusinessDate') IS NULL
BEGIN
    ALTER TABLE dbo.SalesMaster ADD BusinessDate DATE NULL;
    PRINT 'Added SalesMaster.BusinessDate';
END
ELSE
    PRINT 'SalesMaster.BusinessDate already exists';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SalesMaster_SalesCounters')
BEGIN
    ALTER TABLE dbo.SalesMaster WITH CHECK
        ADD CONSTRAINT FK_SalesMaster_SalesCounters
        FOREIGN KEY (CounterId) REFERENCES dbo.SalesCounters (Id);
    PRINT 'Added FK_SalesMaster_SalesCounters';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesMaster_Counter_BusinessDate'
                                             AND object_id = OBJECT_ID('dbo.SalesMaster'))
BEGIN
    CREATE INDEX IX_SalesMaster_Counter_BusinessDate
        ON dbo.SalesMaster (CounterId, BusinessDate)
        INCLUDE (StoreId, InvoiceNo, TotalBill, TotalPaid, IsDeleted, IsDraft);
    PRINT 'Created IX_SalesMaster_Counter_BusinessDate';
END
GO

/* Backfill: derive the business day, then attach every historical sale to the
   store's default counter. Guarded so re-running never rewrites a real value. */
UPDATE sm
SET sm.BusinessDate = dbo.fn_BusinessDate(sm.CreatedDate)
FROM dbo.SalesMaster sm
WHERE sm.BusinessDate IS NULL;
PRINT CONCAT('Backfilled SalesMaster.BusinessDate: ', @@ROWCOUNT);
GO

UPDATE sm
SET sm.CounterId = c.Id
FROM dbo.SalesMaster sm
CROSS APPLY (
    SELECT TOP 1 sc.Id
    FROM dbo.SalesCounters sc
    WHERE sc.StoreId = sm.StoreId AND sc.IsDefault = 1
    ORDER BY sc.Id
) c
WHERE sm.CounterId IS NULL;
PRINT CONCAT('Backfilled SalesMaster.CounterId: ', @@ROWCOUNT);
GO

/* -------------------------------------------------------------------------- */
/* 4. Closings                                                                 */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.CounterClosings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CounterClosings
    (
        Id                  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CounterClosings PRIMARY KEY,
        StoreId             INT             NOT NULL,
        CounterId           INT             NOT NULL,
        BusinessDate        DATE            NOT NULL,
        PeriodStart         DATETIME        NOT NULL,
        PeriodEnd           DATETIME        NOT NULL,

        OpeningCash         DECIMAL(18,2)   NOT NULL,
        CashSales           DECIMAL(18,2)   NOT NULL,
        CashIn              DECIMAL(18,2)   NOT NULL CONSTRAINT DF_CounterClosings_CashIn  DEFAULT (0),
        CashOut             DECIMAL(18,2)   NOT NULL CONSTRAINT DF_CounterClosings_CashOut DEFAULT (0),
        ExpectedCash        DECIMAL(18,2)   NOT NULL,
        ActualCash          DECIMAL(18,2)   NOT NULL,
        Difference          DECIMAL(18,2)   NOT NULL,

        Note                NVARCHAR(500)   NULL,
        PaymentSummaryJson  NVARCHAR(MAX)   NULL,

        ClosedBy            INT             NOT NULL,
        ClosedAt            DATETIME        NOT NULL CONSTRAINT DF_CounterClosings_ClosedAt DEFAULT (GETDATE()),

        Status              NVARCHAR(20)    NOT NULL CONSTRAINT DF_CounterClosings_Status DEFAULT ('CLOSED'),
        ReopenedBy          INT             NULL,
        ReopenedAt          DATETIME        NULL,
        ReopenReason        NVARCHAR(500)   NULL,

        CONSTRAINT FK_CounterClosings_Stores  FOREIGN KEY (StoreId)   REFERENCES dbo.Stores (Id),
        CONSTRAINT FK_CounterClosings_Counters FOREIGN KEY (CounterId) REFERENCES dbo.SalesCounters (Id),
        CONSTRAINT CK_CounterClosings_Status  CHECK (Status IN ('CLOSED', 'REOPENED'))
    );
    PRINT 'Created dbo.CounterClosings';
END
ELSE
    PRINT 'dbo.CounterClosings already exists';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CounterClosings_Open'
                                             AND object_id = OBJECT_ID('dbo.CounterClosings'))
BEGIN
    -- At most one open (CLOSED) record per counter per business day.
    CREATE UNIQUE INDEX UX_CounterClosings_Open
        ON dbo.CounterClosings (CounterId, BusinessDate)
        WHERE Status = 'CLOSED';
    PRINT 'Created UX_CounterClosings_Open';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CounterClosings_Store_Date'
                                             AND object_id = OBJECT_ID('dbo.CounterClosings'))
BEGIN
    CREATE INDEX IX_CounterClosings_Store_Date
        ON dbo.CounterClosings (StoreId, BusinessDate DESC);
    PRINT 'Created IX_CounterClosings_Store_Date';
END
GO

/* -------------------------------------------------------------------------- */
/* 5. Declared cash in / out                                                   */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.CounterCashMovements', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CounterCashMovements
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CounterCashMovements PRIMARY KEY,
        ClosingId   BIGINT        NOT NULL,
        Direction   NVARCHAR(3)   NOT NULL,
        Amount      DECIMAL(18,2) NOT NULL,
        Reason      NVARCHAR(200) NULL,
        CreatedBy   INT           NOT NULL,
        CreatedAt   DATETIME      NOT NULL CONSTRAINT DF_CounterCashMovements_CreatedAt DEFAULT (GETDATE()),
        CONSTRAINT FK_CounterCashMovements_Closings FOREIGN KEY (ClosingId) REFERENCES dbo.CounterClosings (Id) ON DELETE CASCADE,
        CONSTRAINT CK_CounterCashMovements_Direction CHECK (Direction IN ('IN', 'OUT')),
        CONSTRAINT CK_CounterCashMovements_Amount    CHECK (Amount > 0)
    );
    PRINT 'Created dbo.CounterCashMovements';
END
ELSE
    PRINT 'dbo.CounterCashMovements already exists';
GO

/* -------------------------------------------------------------------------- */
/* 6. Route + grants                                                           */
/* -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = '/CounterClosing')
BEGIN
    INSERT INTO dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Counter Day-End Closing', N'/CounterClosing', 1);
    PRINT 'Inserted Routes row /CounterClosing';
END
GO

INSERT INTO dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT g.GroupId, r.RouteId
FROM dbo.GroupsRole g
CROSS JOIN dbo.Routes r
WHERE r.RoutePath = '/CounterClosing'
  AND NOT EXISTS (
        SELECT 1 FROM dbo.GroupRoutePermissions p
        WHERE p.GroupId = g.GroupId AND p.RouteId = r.RouteId);
PRINT CONCAT('Granted /CounterClosing to groups: ', @@ROWCOUNT);
GO

/* -------------------------------------------------------------------------- */
/* 7. Verify                                                                   */
/* -------------------------------------------------------------------------- */
SELECT 'Counters' AS What, COUNT(*) AS Rows FROM dbo.SalesCounters
UNION ALL SELECT 'Closings', COUNT(*) FROM dbo.CounterClosings
UNION ALL SELECT 'SalesMaster w/o Counter', COUNT(*) FROM dbo.SalesMaster WHERE CounterId IS NULL
UNION ALL SELECT 'SalesMaster w/o BusinessDate', COUNT(*) FROM dbo.SalesMaster WHERE BusinessDate IS NULL;
GO
