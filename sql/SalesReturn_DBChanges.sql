-- ============================================================================
-- Sales Return / Exchange feature - required database changes
-- Run this ONCE against the target database before using the new
-- Return / Exchange flow.
-- ============================================================================

-- 1. Flag returned detail lines so they can be distinguished from sold lines
--    in reports and documents (negative totals + IsReturn = 1).
IF COL_LENGTH('SalesDetails', 'IsReturn') IS NULL
BEGIN
    ALTER TABLE SalesDetails
        ADD IsReturn BIT NOT NULL CONSTRAINT DF_SalesDetails_IsReturn DEFAULT 0;
END
GO

-- 2. Register the "Sales Return" stock movement type (used by the return's
--    StockIn entry, TransactionTypeID = 4).
IF NOT EXISTS (SELECT 1 FROM TransactionType WHERE TransactionTypeID = 4)
BEGIN
    SET IDENTITY_INSERT TransactionType ON;
    INSERT INTO TransactionType (TransactionTypeID, TransactionTypeName, StockDirection)
    VALUES (4, 'Sales Return', 1);
    SET IDENTITY_INSERT TransactionType OFF;
END
GO
