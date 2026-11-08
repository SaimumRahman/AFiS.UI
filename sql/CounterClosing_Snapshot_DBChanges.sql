/* ============================================================================
   Counter Day-End Closing - snapshot + booking-payment + cash-out schema
   Idempotent. Run against AFIS_MAH_LIVE.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

/* -------------------------------------------------------------------------- */
/* 1. CounterClosings: cash-out breakdown + auto-close flag                   */
/* -------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.CounterClosings','AllIncome') IS NULL
    ALTER TABLE dbo.CounterClosings ADD AllIncome DECIMAL(18,2) NOT NULL CONSTRAINT DF_CC_AllIncome DEFAULT (0);
IF COL_LENGTH('dbo.CounterClosings','BankPosAmount') IS NULL
    ALTER TABLE dbo.CounterClosings ADD BankPosAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CC_BankPos DEFAULT (0);
IF COL_LENGTH('dbo.CounterClosings','MfsAmount') IS NULL
    ALTER TABLE dbo.CounterClosings ADD MfsAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CC_Mfs DEFAULT (0);
IF COL_LENGTH('dbo.CounterClosings','CashExpense') IS NULL
    ALTER TABLE dbo.CounterClosings ADD CashExpense DECIMAL(18,2) NOT NULL CONSTRAINT DF_CC_CashExpense DEFAULT (0);
IF COL_LENGTH('dbo.CounterClosings','SupplierCashOut') IS NULL
    ALTER TABLE dbo.CounterClosings ADD SupplierCashOut DECIMAL(18,2) NOT NULL CONSTRAINT DF_CC_SupplierCashOut DEFAULT (0);
IF COL_LENGTH('dbo.CounterClosings','IsAutoClosed') IS NULL
    ALTER TABLE dbo.CounterClosings ADD IsAutoClosed BIT NOT NULL CONSTRAINT DF_CC_IsAutoClosed DEFAULT (0);
GO

/* -------------------------------------------------------------------------- */
/* 2. PaymentTransaction: booking + business-day + counter stamping            */
/* -------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.PaymentTransaction','BookingMasterId') IS NULL
    ALTER TABLE dbo.PaymentTransaction ADD BookingMasterId INT NULL;
IF COL_LENGTH('dbo.PaymentTransaction','BusinessDate') IS NULL
    ALTER TABLE dbo.PaymentTransaction ADD BusinessDate DATE NULL;
IF COL_LENGTH('dbo.PaymentTransaction','CounterId') IS NULL
    ALTER TABLE dbo.PaymentTransaction ADD CounterId INT NULL;
GO

/* Backfill A: payment row points at a booking only */
UPDATE pt
SET pt.BookingMasterId = pt.SalesMasterId,
    pt.SalesMasterId   = 0
FROM dbo.PaymentTransaction pt
JOIN dbo.BookingMaster bm ON bm.BookingMasterId = pt.SalesMasterId
WHERE ISNULL(pt.IsDeleted,0) = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.SalesMaster sm WHERE sm.SaleMasterId = pt.SalesMasterId);
GO

/* Backfill B: business day / counter for the remaining rows */
UPDATE pt
SET pt.BusinessDate = sm.BusinessDate,
    pt.CounterId     = sm.CounterId
FROM dbo.PaymentTransaction pt
JOIN dbo.SalesMaster sm ON sm.SaleMasterId = pt.SalesMasterId
WHERE pt.BusinessDate IS NULL
  AND pt.SalesMasterId > 0;
GO

UPDATE pt
SET pt.BusinessDate = dbo.fn_BusinessDate(bm.CreatedDate),
    pt.CounterId     = bm.CounterId
FROM dbo.PaymentTransaction pt
JOIN dbo.BookingMaster bm ON bm.BookingMasterId = pt.BookingMasterId
WHERE pt.BusinessDate IS NULL
  AND pt.BookingMasterId IS NOT NULL;
GO

/* Review: rows that could not be stamped */
SELECT 'UNSTAMPED' AS Kind, COUNT(*) AS Cnt
FROM dbo.PaymentTransaction
WHERE ISNULL(IsDeleted,0)=0 AND BusinessDate IS NULL;
GO

/* -------------------------------------------------------------------------- */
/* 3. usp_CounterDaySnapshot                                                  */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_CounterDaySnapshot','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CounterDaySnapshot;
GO
CREATE PROCEDURE dbo.usp_CounterDaySnapshot
    @StoreId     INT,
    @CounterId   INT,
    @BusinessDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OpeningCash DECIMAL(18,2) = 0;
    SELECT TOP 1 @OpeningCash = ActualCash
    FROM dbo.CounterClosings
    WHERE CounterId = @CounterId
      AND BusinessDate < @BusinessDate
      AND Status = 'CLOSED'
    ORDER BY BusinessDate DESC, Id DESC;

    DECLARE @AllIncome    DECIMAL(18,2) = 0;
    DECLARE @CashIncome   DECIMAL(18,2) = 0;
    DECLARE @BankPos      DECIMAL(18,2) = 0;
    DECLARE @Mfs          DECIMAL(18,2) = 0;
    DECLARE @CashTxnCount INT = 0;

    SELECT @AllIncome    = ISNULL(SUM(pt.PaidAmount),0),
           @CashIncome   = ISNULL(SUM(CASE WHEN ISNULL(fa.FinancialAccountTypeId,0) NOT IN (1,2) THEN pt.PaidAmount ELSE 0 END),0),
           @BankPos      = ISNULL(SUM(CASE WHEN fa.FinancialAccountTypeId = 1 THEN pt.PaidAmount ELSE 0 END),0),
           @Mfs          = ISNULL(SUM(CASE WHEN fa.FinancialAccountTypeId = 2 THEN pt.PaidAmount ELSE 0 END),0),
           @CashTxnCount = COUNT(CASE WHEN ISNULL(fa.FinancialAccountTypeId,0) NOT IN (1,2) THEN 1 END)
    FROM dbo.PaymentTransaction pt
    LEFT JOIN dbo.FinancialAccounts fa ON fa.Id = pt.TransactionTypeId AND pt.TransactionTypeId > 0
    WHERE ISNULL(pt.IsDeleted,0) = 0
      AND pt.CounterId   = @CounterId
      AND pt.BusinessDate = @BusinessDate;

    DECLARE @CashExpense DECIMAL(18,2) = 0;
    DECLARE @UnpostedExpenseCount INT = 0;

    SELECT @CashExpense = ISNULL(SUM(de.Amount),0)
    FROM dbo.DailyExpenses de
    LEFT JOIN dbo.FinancialAccounts fa ON fa.Id = de.FinancialAccountId
    WHERE de.StoreId = (SELECT StoreId FROM dbo.SalesCounters WHERE Id = @CounterId)
      AND CAST(de.ExpenseDate AS DATE) = @BusinessDate
      AND ISNULL(de.IsDeleted,0) = 0
      AND de.IsPosted = 1
      AND (de.FinancialAccountId IS NULL OR ISNULL(fa.FinancialAccountTypeId,0) NOT IN (1,2));

    SELECT @UnpostedExpenseCount = COUNT(*)
    FROM dbo.DailyExpenses
    WHERE StoreId = (SELECT StoreId FROM dbo.SalesCounters WHERE Id = @CounterId)
      AND CAST(ExpenseDate AS DATE) = @BusinessDate
      AND ISNULL(IsDeleted,0) = 0
      AND IsPosted = 0;

    DECLARE @SupplierCashOut DECIMAL(18,2) = 0;

    SELECT @SupplierCashOut = ISNULL(SUM(sp.PaymentAmount),0)
    FROM dbo.SupplierPayments sp
    LEFT JOIN dbo.FinancialAccounts fa ON fa.Id = sp.FinancialAccountId
    WHERE sp.StoreId = (SELECT StoreId FROM dbo.SalesCounters WHERE Id = @CounterId)
      AND dbo.fn_BusinessDate(sp.PaymentDate) = @BusinessDate
      AND (sp.FinancialAccountId IS NULL OR ISNULL(fa.FinancialAccountTypeId,0) NOT IN (1,2)
           OR sp.PaymentMethod = 'Cash');

    DECLARE @TransactionCount INT = 0;
    DECLARE @PendingDraftCount INT = 0;

    SELECT @TransactionCount = COUNT(*),
           @PendingDraftCount = ISNULL(SUM(CASE WHEN ISNULL(IsDraft,0)=1 THEN 1 ELSE 0 END),0)
    FROM (
        SELECT IsDraft FROM dbo.SalesMaster
        WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND ISNULL(IsDeleted,0)=0
        UNION ALL
        SELECT IsDraft FROM dbo.BookingMaster
        WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND ISNULL(IsDeleted,0)=0
    ) t;

    DECLARE @ClosingBalance DECIMAL(18,2) =
        @OpeningCash + @AllIncome - @CashExpense - @SupplierCashOut - @BankPos - @Mfs;

    SELECT
        @OpeningCash          AS OpeningCash,
        @AllIncome            AS AllIncome,
        @CashIncome           AS CashIncome,
        @BankPos              AS BankPosAmount,
        @Mfs                  AS MfsAmount,
        @CashExpense          AS CashExpense,
        @SupplierCashOut      AS SupplierCashOut,
        @ClosingBalance       AS ClosingBalance,
        @CashTxnCount         AS CashTxnCount,
        @TransactionCount     AS TransactionCount,
        @PendingDraftCount    AS PendingDraftCount,
        @UnpostedExpenseCount AS UnpostedExpenseCount;
END
GO
PRINT 'Created dbo.usp_CounterDaySnapshot';
GO

/* -------------------------------------------------------------------------- */
/* 4. usp_TryAutoCloseCounter                                                 */
/* -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.usp_TryAutoCloseCounter','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_TryAutoCloseCounter;
GO
CREATE PROCEDURE dbo.usp_TryAutoCloseCounter
    @CounterId    INT,
    @BusinessDate DATE,
    @SystemUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @rc INT = 0;   -- 0 = skipped, 1 = closed

    /* Only past business days */
    IF @BusinessDate >= dbo.fn_BusinessDate(GETDATE())
        RETURN 0;

    /* Already closed? */
    IF EXISTS (SELECT 1 FROM dbo.CounterClosings
               WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND Status = 'CLOSED')
        RETURN 0;

    /* Has any activity? */
    DECLARE @txnCount INT;
    SELECT @txnCount = COUNT(*) FROM (
        SELECT SaleMasterId FROM dbo.SalesMaster
        WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND ISNULL(IsDeleted,0)=0
        UNION ALL
        SELECT BookingMasterId FROM dbo.BookingMaster
        WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND ISNULL(IsDeleted,0)=0
    ) t;
    IF ISNULL(@txnCount,0) = 0
        RETURN 0;

    DECLARE @storeId INT = (SELECT StoreId FROM dbo.SalesCounters WHERE Id = @CounterId);
    DECLARE @lockResource NVARCHAR(100) = 'COUNTERCLOSE:' + CAST(@CounterId AS VARCHAR(10)) + ':' + CONVERT(VARCHAR(8), @BusinessDate, 112);

    /* Compute snapshot into temp table */
    DECLARE @snap TABLE (
        OpeningCash DECIMAL(18,2), AllIncome DECIMAL(18,2), CashIncome DECIMAL(18,2),
        BankPosAmount DECIMAL(18,2), MfsAmount DECIMAL(18,2), CashExpense DECIMAL(18,2),
        SupplierCashOut DECIMAL(18,2), ClosingBalance DECIMAL(18,2),
        CashTxnCount INT, TransactionCount INT, PendingDraftCount INT, UnpostedExpenseCount INT
    );
    INSERT INTO @snap
    EXEC dbo.usp_CounterDaySnapshot
         @StoreId = @storeId,
         @CounterId = @CounterId,
         @BusinessDate = @BusinessDate;

    DECLARE @OpeningCash DECIMAL(18,2), @AllIncome DECIMAL(18,2), @BankPos DECIMAL(18,2),
            @Mfs DECIMAL(18,2), @CashExpense DECIMAL(18,2), @SupplierCashOut DECIMAL(18,2),
            @ClosingBalance DECIMAL(18,2), @CashSales DECIMAL(18,2),
            @PendingDraftCount INT, @UnpostedExpenseCount INT;

    SELECT @OpeningCash = OpeningCash, @AllIncome = AllIncome, @CashSales = CashIncome,
           @BankPos = BankPosAmount, @Mfs = MfsAmount, @CashExpense = CashExpense,
           @SupplierCashOut = SupplierCashOut, @ClosingBalance = ClosingBalance,
           @PendingDraftCount = PendingDraftCount, @UnpostedExpenseCount = UnpostedExpenseCount
    FROM @snap;

    BEGIN TRY
        BEGIN TRAN;
        EXEC sys.sp_getapplock @Resource = @lockResource,
             @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;

        /* Re-check after lock */
        IF EXISTS (SELECT 1 FROM dbo.CounterClosings
                   WHERE CounterId = @CounterId AND BusinessDate = @BusinessDate AND Status = 'CLOSED')
        BEGIN
            ROLLBACK TRAN;
            RETURN 0;
        END

        INSERT INTO dbo.CounterClosings
            (StoreId, CounterId, BusinessDate, PeriodStart, PeriodEnd,
             OpeningCash, CashSales, CashIn, CashOut, ExpectedCash,
             ActualCash, Difference, Note, PaymentSummaryJson,
             ClosedBy, ClosedAt, Status,
             AllIncome, BankPosAmount, MfsAmount, CashExpense, SupplierCashOut, IsAutoClosed)
        VALUES
            ((SELECT StoreId FROM dbo.SalesCounters WHERE Id = @CounterId), @CounterId, @BusinessDate,
             DATEADD(HOUR,6,CAST(@BusinessDate AS DATETIME)), DATEADD(HOUR,30,CAST(@BusinessDate AS DATETIME)),
             @OpeningCash, @CashSales, 0, @CashExpense + @SupplierCashOut, @ClosingBalance,
             @ClosingBalance, 0,
             'Auto-closed by system at day end (' + CAST(@PendingDraftCount AS VARCHAR(10)) + ' draft(s), '
                + CAST(@UnpostedExpenseCount AS VARCHAR(10)) + ' unposted expense(s)).',
             NULL, @SystemUserId, GETDATE(), 'CLOSED',
             @AllIncome, @BankPos, @Mfs, @CashExpense, @SupplierCashOut, 1);

        COMMIT TRAN;
        SET @rc = 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SET @rc = 0;
    END CATCH

    RETURN @rc;
END
GO
PRINT 'Created dbo.usp_TryAutoCloseCounter';
GO