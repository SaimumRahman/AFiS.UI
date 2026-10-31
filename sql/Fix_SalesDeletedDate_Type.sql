/* ============================================================================
   Fix: SalesPOS "Void invoice" fails with
        "Implicit conversion from data type datetime to int is not allowed"

   Cause
     dbo.SalesMaster.DeletedDate, dbo.SalesDetails.DeletedDate and
     dbo.PaymentTransaction.DeletedDate were created as INT while the
     void-invoice code writes GETDATE() into them (and the sibling audit
     columns CreatedDate / UpdatedDate are DATETIME).

   Fix
     Widen the audit column to DATETIME NULL. All existing values are NULL,
     so there is no data to convert, no index/computed column/trigger/view
     depends on these columns, and no rows are lost.

   Idempotent: safe to run more than once.
   ============================================================================ */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

/* ── Pre-check: every existing value must be NULL ───────────────────────── */
IF EXISTS (SELECT 1 FROM dbo.SalesMaster        WHERE DeletedDate IS NOT NULL)
   OR EXISTS (SELECT 1 FROM dbo.SalesDetails     WHERE DeletedDate IS NOT NULL)
   OR EXISTS (SELECT 1 FROM dbo.PaymentTransaction WHERE DeletedDate IS NOT NULL)
BEGIN
    RAISERROR('Aborting: one of the DeletedDate columns already holds non-NULL int values. Convert manually first.', 16, 1);
    RETURN;
END
GO

/* ── SalesMaster ────────────────────────────────────────────────────────── */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.SalesMaster')
             AND name = 'DeletedDate' AND TYPE_NAME(user_type_id) = 'int')
BEGIN
    ALTER TABLE dbo.SalesMaster ALTER COLUMN DeletedDate DATETIME NULL;
    PRINT 'SalesMaster.DeletedDate -> DATETIME NULL';
END
ELSE
    PRINT 'SalesMaster.DeletedDate already DATETIME (skipped)';
GO

/* ── SalesDetails ───────────────────────────────────────────────────────── */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.SalesDetails')
             AND name = 'DeletedDate' AND TYPE_NAME(user_type_id) = 'int')
BEGIN
    ALTER TABLE dbo.SalesDetails ALTER COLUMN DeletedDate DATETIME NULL;
    PRINT 'SalesDetails.DeletedDate -> DATETIME NULL';
END
ELSE
    PRINT 'SalesDetails.DeletedDate already DATETIME (skipped)';
GO

/* ── PaymentTransaction ─────────────────────────────────────────────────── */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.PaymentTransaction')
             AND name = 'DeletedDate' AND TYPE_NAME(user_type_id) = 'int')
BEGIN
    ALTER TABLE dbo.PaymentTransaction ALTER COLUMN DeletedDate DATETIME NULL;
    PRINT 'PaymentTransaction.DeletedDate -> DATETIME NULL';
END
ELSE
    PRINT 'PaymentTransaction.DeletedDate already DATETIME (skipped)';
GO

/* ── Verify ─────────────────────────────────────────────────────────────── */
SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS DataType, c.is_nullable
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('SalesMaster', 'SalesDetails', 'PaymentTransaction')
  AND c.name = 'DeletedDate'
ORDER BY t.name;
GO
