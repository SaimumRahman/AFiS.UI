SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
GO

-- ============================================================
-- Phase 1 audit: Tier-1 transactional tables + CounterClosings
-- ChangedBy resolved from the row's CreatedBy/UpdatedBy/DeletedBy
-- (no SESSION_CONTEXT plumbing). Trigger-based row snapshots in JSON.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='AuditLog')
BEGIN
    CREATE TABLE dbo.AuditLog (
        AuditId       BIGINT IDENTITY(1,1) PRIMARY KEY,
        TableName     NVARCHAR(128) NOT NULL,
        Op            CHAR(1)       NOT NULL,
        RowId         BIGINT        NULL,
        BusinessKey   NVARCHAR(100) NULL,
        StoreId       INT           NULL,
        OldData       NVARCHAR(MAX) NULL,
        NewData       NVARCHAR(MAX) NULL,
        ChangedBy     INT           NULL,
        ChangedByName NVARCHAR(100) NULL,
        ChangedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        AppUser       NVARCHAR(128) NULL,
        HostName      NVARCHAR(128) NULL
    );
    CREATE INDEX IX_AuditLog_Table_Row   ON dbo.AuditLog (TableName, RowId, ChangedAt DESC);
    CREATE INDEX IX_AuditLog_BusinessKey ON dbo.AuditLog (BusinessKey) WHERE BusinessKey IS NOT NULL;
    CREATE INDEX IX_AuditLog_Store_Changed ON dbo.AuditLog (StoreId, ChangedAt DESC);
END
GO

-- Helper: drop trigger if exists
-- (inline per table below)

-- ---------- SalesMaster ----------
IF OBJECT_ID('dbo.trg_Audit_SalesMaster','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_SalesMaster;
GO
CREATE TRIGGER dbo.trg_Audit_SalesMaster ON dbo.SalesMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, ChangedByName, AppUser, HostName)
    SELECT N'SalesMaster',
           CASE WHEN d.SaleMasterId IS NULL THEN 'I' WHEN i.SaleMasterId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.SaleMasterId, d.SaleMasterId),
           COALESCE(i.InvoiceNo, d.InvoiceNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.SaleMasterId = d.SaleMasterId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.SaleMasterId = i.SaleMasterId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           NULL, SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.SaleMasterId = i.SaleMasterId;
END
GO

-- ---------- SalesDetails ----------
IF OBJECT_ID('dbo.trg_Audit_SalesDetails','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_SalesDetails;
GO
CREATE TRIGGER dbo.trg_Audit_SalesDetails ON dbo.SalesDetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'SalesDetails',
           CASE WHEN d.SalesDetailsId IS NULL THEN 'I' WHEN i.SalesDetailsId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.SalesDetailsId, d.SalesDetailsId),
           COALESCE(i.ReturnRefNo, d.ReturnRefNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.SalesDetailsId = d.SalesDetailsId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.SalesDetailsId = i.SalesDetailsId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.SalesDetailsId = i.SalesDetailsId;
END
GO

-- ---------- PaymentTransaction ----------
IF OBJECT_ID('dbo.trg_Audit_PaymentTransaction','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_PaymentTransaction;
GO
CREATE TRIGGER dbo.trg_Audit_PaymentTransaction ON dbo.PaymentTransaction
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'PaymentTransaction',
           CASE WHEN d.PayTranId IS NULL THEN 'I' WHEN i.PayTranId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.PayTranId, d.PayTranId),
           COALESCE(CAST(i.SalesMasterId AS NVARCHAR(50)), CAST(d.SalesMasterId AS NVARCHAR(50))),
           (SELECT d2.* FROM deleted  d2 WHERE d2.PayTranId = d.PayTranId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.PayTranId = i.PayTranId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.CreatedBy, d.CreatedBy, d.DeletedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.PayTranId = i.PayTranId;
END
GO

-- ---------- BookingMaster ----------
IF OBJECT_ID('dbo.trg_Audit_BookingMaster','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_BookingMaster;
GO
CREATE TRIGGER dbo.trg_Audit_BookingMaster ON dbo.BookingMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'BookingMaster',
           CASE WHEN d.BookingMasterId IS NULL THEN 'I' WHEN i.BookingMasterId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.BookingMasterId, d.BookingMasterId),
           COALESCE(i.InvoiceNo, d.InvoiceNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.BookingMasterId = d.BookingMasterId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.BookingMasterId = i.BookingMasterId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.CancelledBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.BookingMasterId = i.BookingMasterId;
END
GO

-- ---------- BookingDetail ----------
IF OBJECT_ID('dbo.trg_Audit_BookingDetail','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_BookingDetail;
GO
CREATE TRIGGER dbo.trg_Audit_BookingDetail ON dbo.BookingDetail
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'BookingDetail',
           CASE WHEN d.BookingDetailId IS NULL THEN 'I' WHEN i.BookingDetailId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.BookingDetailId, d.BookingDetailId),
           COALESCE(i.Barcode, d.Barcode),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.BookingDetailId = d.BookingDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.BookingDetailId = i.BookingDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.CreatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.BookingDetailId = i.BookingDetailId;
END
GO

-- ---------- StockMaster ----------
IF OBJECT_ID('dbo.trg_Audit_StockMaster','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_StockMaster;
GO
CREATE TRIGGER dbo.trg_Audit_StockMaster ON dbo.StockMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'StockMaster',
           CASE WHEN d.StockID IS NULL THEN 'I' WHEN i.StockID IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.StockID, d.StockID),
           COALESCE(CAST(i.ItemID AS NVARCHAR(50)), CAST(d.ItemID AS NVARCHAR(50))),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.StockID = d.StockID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.StockID = i.StockID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.StockID = i.StockID;
END
GO

-- ---------- StockDetail (stock ledger) ----------
IF OBJECT_ID('dbo.trg_Audit_StockDetail','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_StockDetail;
GO
CREATE TRIGGER dbo.trg_Audit_StockDetail ON dbo.StockDetail
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'StockDetail',
           CASE WHEN d.StockDetailId IS NULL THEN 'I' WHEN i.StockDetailId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.StockDetailId, d.StockDetailId),
           COALESCE(i.TransactionNo, d.TransactionNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.StockDetailId = d.StockDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.StockDetailId = i.StockDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.StockDetailId = i.StockDetailId;
END
GO

-- ---------- Purchases ----------
IF OBJECT_ID('dbo.trg_Audit_Purchases','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Purchases;
GO
CREATE TRIGGER dbo.trg_Audit_Purchases ON dbo.Purchases
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'Purchases',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.SystemInvoiceNo, i.InvoiceNo, d.SystemInvoiceNo, d.InvoiceNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.EditedBy, d.EditedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- PurchaseItems ----------
IF OBJECT_ID('dbo.trg_Audit_PurchaseItems','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_PurchaseItems;
GO
CREATE TRIGGER dbo.trg_Audit_PurchaseItems ON dbo.PurchaseItems
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'PurchaseItems',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.ReturnRefNo, d.ReturnRefNo),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           NULL, SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- TransferMaster ----------
IF OBJECT_ID('dbo.trg_Audit_TransferMaster','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_TransferMaster;
GO
CREATE TRIGGER dbo.trg_Audit_TransferMaster ON dbo.TransferMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'TransferMaster',
           CASE WHEN d.TransferId IS NULL THEN 'I' WHEN i.TransferId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.TransferId, d.TransferId),
           COALESCE(i.TransferNo, d.TransferNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.TransferId = d.TransferId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.TransferId = i.TransferId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.CancelledBy, d.DeletedBy, d.ReceivedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.TransferId = i.TransferId;
END
GO

-- ---------- TransferDetail ----------
IF OBJECT_ID('dbo.trg_Audit_TransferDetail','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_TransferDetail;
GO
CREATE TRIGGER dbo.trg_Audit_TransferDetail ON dbo.TransferDetail
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'TransferDetail',
           CASE WHEN d.TransferDetailId IS NULL THEN 'I' WHEN i.TransferDetailId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.TransferDetailId, d.TransferDetailId),
           COALESCE(CAST(i.TransferId AS NVARCHAR(50)), CAST(d.TransferId AS NVARCHAR(50))),
           (SELECT d2.* FROM deleted  d2 WHERE d2.TransferDetailId = d.TransferDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.TransferDetailId = i.TransferDetailId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.TransferDetailId = i.TransferDetailId;
END
GO

-- ---------- InvRequisitionMaster ----------
IF OBJECT_ID('dbo.trg_Audit_InvRequisitionMaster','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_InvRequisitionMaster;
GO
CREATE TRIGGER dbo.trg_Audit_InvRequisitionMaster ON dbo.InvRequisitionMaster
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'InvRequisitionMaster',
           CASE WHEN d.RequisitionID IS NULL THEN 'I' WHEN i.RequisitionID IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.RequisitionID, d.RequisitionID),
           COALESCE(i.RequisitionNo, d.RequisitionNo),
           COALESCE(i.ToStore, d.ToStore),
           (SELECT d2.* FROM deleted  d2 WHERE d2.RequisitionID = d.RequisitionID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.RequisitionID = i.RequisitionID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdateBy, i.CreateBy, i.StatusBy, i.RequisitionBy, d.UpdateBy, d.CreateBy, d.StatusBy, d.RequisitionBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.RequisitionID = i.RequisitionID;
END
GO

-- ---------- InvRequisitionDetail ----------
IF OBJECT_ID('dbo.trg_Audit_InvRequisitionDetail','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_InvRequisitionDetail;
GO
CREATE TRIGGER dbo.trg_Audit_InvRequisitionDetail ON dbo.InvRequisitionDetail
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'InvRequisitionDetail',
           CASE WHEN d.RequisitionDetailID IS NULL THEN 'I' WHEN i.RequisitionDetailID IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.RequisitionDetailID, d.RequisitionDetailID),
           COALESCE(CAST(i.RequisitionID AS NVARCHAR(50)), CAST(d.RequisitionID AS NVARCHAR(50))),
           (SELECT d2.* FROM deleted  d2 WHERE d2.RequisitionDetailID = d.RequisitionDetailID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.RequisitionDetailID = i.RequisitionDetailID FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.CreateBy, d.CreateBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.RequisitionDetailID = i.RequisitionDetailID;
END
GO

-- ---------- CounterClosings ----------
IF OBJECT_ID('dbo.trg_Audit_CounterClosings','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_CounterClosings;
GO
CREATE TRIGGER dbo.trg_Audit_CounterClosings ON dbo.CounterClosings
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'CounterClosings',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(CAST(i.BusinessDate AS NVARCHAR(20)), CAST(d.BusinessDate AS NVARCHAR(20))),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.ClosedBy, i.ReopenedBy, d.ReopenedBy, d.ClosedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

PRINT 'Phase 1 audit triggers installed.';
GO
