SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
-- Phase 2 audit: Tier-2 master data + SmsSettings (masked secrets)
-- ============================================================

-- ---------- Items (SalePrice drift is the key risk) ----------
IF OBJECT_ID('dbo.trg_Audit_Items','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Items;
GO
CREATE TRIGGER dbo.trg_Audit_Items ON dbo.Items
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'Items',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Barcode, d.Barcode),
           NULL,
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.UpdatedBy, i.CreatedBy, d.IsDeletedBy, d.DeletedBy, d.UpdatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- CustomerDetails ----------
IF OBJECT_ID('dbo.trg_Audit_CustomerDetails','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_CustomerDetails;
GO
CREATE TRIGGER dbo.trg_Audit_CustomerDetails ON dbo.CustomerDetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'CustomerDetails',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Phone, d.Phone),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.CreatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- Employees ----------
IF OBJECT_ID('dbo.trg_Audit_Employees','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Employees;
GO
CREATE TRIGGER dbo.trg_Audit_Employees ON dbo.Employees
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'Employees',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Name, d.Name),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
            COALESCE(i.CreatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- Suppliers ----------
IF OBJECT_ID('dbo.trg_Audit_Suppliers','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Suppliers;
GO
CREATE TRIGGER dbo.trg_Audit_Suppliers ON dbo.Suppliers
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, AppUser, HostName)
    SELECT N'Suppliers',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Name, d.Name),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO
-- ---------- Stores ----------
IF OBJECT_ID('dbo.trg_Audit_Stores','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Stores;
GO
CREATE TRIGGER dbo.trg_Audit_Stores ON dbo.Stores
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'Stores',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Name, d.Name),
           COALESCE(i.Id, d.Id),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           COALESCE(i.CreatedBy, d.CreatedBy),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- Groups (VAT!) ----------
IF OBJECT_ID('dbo.trg_Audit_Groups','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Groups;
GO
CREATE TRIGGER dbo.trg_Audit_Groups ON dbo.Groups
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'Groups',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Name, d.Name),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           NULL, SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- FinancialAccounts ----------
IF OBJECT_ID('dbo.trg_Audit_FinancialAccounts','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_FinancialAccounts;
GO
CREATE TRIGGER dbo.trg_Audit_FinancialAccounts ON dbo.FinancialAccounts
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'FinancialAccounts',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.AccountNo, d.AccountNo),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           NULL, SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- SalesCounters ----------
IF OBJECT_ID('dbo.trg_Audit_SalesCounters','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_SalesCounters;
GO
CREATE TRIGGER dbo.trg_Audit_SalesCounters ON dbo.SalesCounters
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, StoreId, OldData, NewData, AppUser, HostName)
    SELECT N'SalesCounters',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           COALESCE(i.Name, d.Name),
           COALESCE(i.StoreId, d.StoreId),
           (SELECT d2.* FROM deleted  d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

-- ---------- Routes ----------
IF OBJECT_ID('dbo.trg_Audit_Routes','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_Routes;
GO
CREATE TRIGGER dbo.trg_Audit_Routes ON dbo.Routes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, AppUser, HostName)
    SELECT N'Routes',
           CASE WHEN d.RouteId IS NULL THEN 'I' WHEN i.RouteId IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.RouteId, d.RouteId),
           COALESCE(i.RoutePath, d.RoutePath),
           (SELECT d2.* FROM deleted  d2 WHERE d2.RouteId = d.RouteId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.* FROM inserted i2 WHERE i2.RouteId = i.RouteId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.RouteId = i.RouteId;
END
GO

-- ---------- SmsSettings (masked secrets) ----------
IF OBJECT_ID('dbo.trg_Audit_SmsSettings','TR') IS NOT NULL DROP TRIGGER dbo.trg_Audit_SmsSettings;
GO
CREATE TRIGGER dbo.trg_Audit_SmsSettings ON dbo.SmsSettings
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLog (TableName, Op, RowId, BusinessKey, OldData, NewData, ChangedBy, AppUser, HostName)
    SELECT N'SmsSettings',
           CASE WHEN d.Id IS NULL THEN 'I' WHEN i.Id IS NULL THEN 'D' ELSE 'U' END,
           COALESCE(i.Id, d.Id),
           N'SmsSettings',
           (SELECT d2.Id, d2.UserName, d2.Sid, Password = REPLICATE('*', LEN(d2.Password)), ApiToken = REPLICATE('*', LEN(d2.ApiToken))
            FROM deleted d2 WHERE d2.Id = d.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           (SELECT i2.Id, i2.UserName, i2.Sid, Password = REPLICATE('*', LEN(i2.Password)), ApiToken = REPLICATE('*', LEN(i2.ApiToken))
            FROM inserted i2 WHERE i2.Id = i.Id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
           NULL, SUSER_SNAME(), HOST_NAME()
    FROM inserted i FULL OUTER JOIN deleted d ON d.Id = i.Id;
END
GO

PRINT 'Phase 2 audit triggers installed.';
GO
