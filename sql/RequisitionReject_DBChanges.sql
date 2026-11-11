SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. RequisitionStatus lookup: today it has three duplicate "Approved" rows and
--    no Pending/Rejected/Completed. The UI rejects by StatusName == "Rejected",
--    so the names are the contract.
-- ─────────────────────────────────────────────────────────────────────────────
-- Drop the duplicate Approved rows that nothing references (master is empty).
DELETE FROM dbo.RequisitionStatus
WHERE Id IN (4, 5)
  AND NOT EXISTS (SELECT 1 FROM dbo.InvRequisitionMaster m WHERE m.StatusID = RequisitionStatus.Id);

IF NOT EXISTS (SELECT 1 FROM dbo.RequisitionStatus WHERE StatusName = N'Pending')
BEGIN
    SET IDENTITY_INSERT dbo.RequisitionStatus ON;
    INSERT INTO dbo.RequisitionStatus (Id, StatusName) VALUES (1, N'Pending');
    SET IDENTITY_INSERT dbo.RequisitionStatus OFF;
END
IF NOT EXISTS (SELECT 1 FROM dbo.RequisitionStatus WHERE StatusName = N'Rejected')
BEGIN
    SET IDENTITY_INSERT dbo.RequisitionStatus ON;
    INSERT INTO dbo.RequisitionStatus (Id, StatusName) VALUES (2, N'Rejected');
    SET IDENTITY_INSERT dbo.RequisitionStatus OFF;
END
IF NOT EXISTS (SELECT 1 FROM dbo.RequisitionStatus WHERE StatusName = N'Approved')
BEGIN
    SET IDENTITY_INSERT dbo.RequisitionStatus ON;
    INSERT INTO dbo.RequisitionStatus (Id, StatusName) VALUES (3, N'Approved');
    SET IDENTITY_INSERT dbo.RequisitionStatus OFF;
END
IF NOT EXISTS (SELECT 1 FROM dbo.RequisitionStatus WHERE StatusName = N'Completed')
BEGIN
    SET IDENTITY_INSERT dbo.RequisitionStatus ON;
    INSERT INTO dbo.RequisitionStatus (Id, StatusName) VALUES (4, N'Completed');
    SET IDENTITY_INSERT dbo.RequisitionStatus OFF;
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. Link a requisition back to the POS sale/booking that created it.
-- ─────────────────────────────────────────────────────────────────────────────
IF COL_LENGTH('dbo.InvRequisitionMaster', 'SourceType') IS NULL
BEGIN
    ALTER TABLE dbo.InvRequisitionMaster
        ADD SourceType TINYINT NULL;  -- 1 = sale, 2 = booking
END
IF COL_LENGTH('dbo.InvRequisitionMaster', 'SourceId') IS NULL
BEGIN
    ALTER TABLE dbo.InvRequisitionMaster
        ADD SourceId INT NULL;  -- SaleMasterId / BookingMasterId
END

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. Cancellation state on a booking (so a rejected booking stays auditable).
-- ─────────────────────────────────────────────────────────────────────────────
IF COL_LENGTH('dbo.BookingMaster', 'IsCancelled') IS NULL
BEGIN
    ALTER TABLE dbo.BookingMaster
        ADD IsCancelled BIT NOT NULL
            CONSTRAINT DF_BookingMaster_IsCancelled DEFAULT (0) WITH VALUES;
END
IF COL_LENGTH('dbo.BookingMaster', 'CancelReason') IS NULL
BEGIN
    ALTER TABLE dbo.BookingMaster
        ADD CancelReason NVARCHAR(500) NULL;
END
IF COL_LENGTH('dbo.BookingMaster', 'CancelledBy') IS NULL
BEGIN
    ALTER TABLE dbo.BookingMaster
        ADD CancelledBy INT NULL;
END
IF COL_LENGTH('dbo.BookingMaster', 'CancelledDate') IS NULL
BEGIN
    ALTER TABLE dbo.BookingMaster
        ADD CancelledDate DATETIME NULL;
END

SELECT Id, StatusName FROM dbo.RequisitionStatus ORDER BY Id;
SELECT '--- InvRequisitionMaster new columns ---';
SELECT c.name, t.name AS DataType FROM sys.columns c
 JOIN sys.types t ON t.user_type_id=c.user_type_id
 WHERE c.object_id=OBJECT_ID('dbo.InvRequisitionMaster') AND c.name IN ('SourceType','SourceId');
SELECT '--- BookingMaster new columns ---';
SELECT c.name, t.name AS DataType FROM sys.columns c
 JOIN sys.types t ON t.user_type_id=c.user_type_id
 WHERE c.object_id=OBJECT_ID('dbo.BookingMaster') AND c.name IN ('IsCancelled','CancelReason','CancelledBy','CancelledDate');