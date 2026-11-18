-- CustomerDetailsRegistration_DBChanges.sql
-- Customer registration form (mobile lookup, company master, 3 addresses, etc.)
-- Does NOT touch the existing Company table (software's own company).

SET NOCOUNT ON;

-- 1. Customer employer company master (separate from the software's own Company table)
IF OBJECT_ID('dbo.CustomerCompany', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomerCompany (
        Id             INT IDENTITY(1,1) PRIMARY KEY,
        Code           NVARCHAR(6)   NOT NULL,
        Name           NVARCHAR(256) NOT NULL,
        Address        NVARCHAR(MAX) NULL,
        SpecialNote    NVARCHAR(MAX) NULL,
        DefaultCategoryId INT NULL,
        IsActive       BIT           NOT NULL CONSTRAINT DF_CustomerCompany_IsActive DEFAULT (1),
        CreatedBy      INT           NULL,
        CreatedDate    DATETIME      NULL,
        ModifiedBy     INT           NULL,
        ModifiedDate   DATETIME      NULL,
        CONSTRAINT UQ_CustomerCompany_Code UNIQUE (Code),
        CONSTRAINT UQ_CustomerCompany_Name UNIQUE (Name),
        CONSTRAINT FK_CustomerCompany_Category FOREIGN KEY (DefaultCategoryId) REFERENCES dbo.MembershipType(Id)
    );
END
GO

-- Case-insensitive uniqueness is already handled by the default collation (SQL_Latin1_General_CP1_CI_AS).
-- If the DB collation is case-sensitive, the UNIQUE constraints above may allow 'SEPL' and 'sepl'.

-- 2. Extend CustomerDetails
IF COL_LENGTH('dbo.CustomerDetails','WhatsApp') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD WhatsApp NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.CustomerDetails','DateOfBirth') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD DateOfBirth DATE NULL;
IF COL_LENGTH('dbo.CustomerDetails','Anniversary') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD Anniversary DATE NULL;
IF COL_LENGTH('dbo.CustomerDetails','CompanyId') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD CompanyId INT NULL;
IF COL_LENGTH('dbo.CustomerDetails','Designation') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD Designation NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.CustomerDetails','EmployeeId') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD EmployeeId NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.CustomerDetails','AddressHome') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD AddressHome NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.CustomerDetails','AddressOffice') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD AddressOffice NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.CustomerDetails','AddressOther') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD AddressOther NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.CustomerDetails','DefaultAddressType') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD DefaultAddressType NVARCHAR(10) NULL;
IF COL_LENGTH('dbo.CustomerDetails','Status') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD Status NVARCHAR(20) NOT NULL CONSTRAINT DF_CustomerDetails_Status DEFAULT (N'Active');
IF COL_LENGTH('dbo.CustomerDetails','Remarks') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD Remarks NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.CustomerDetails','UpdatedBy') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD UpdatedBy INT NULL;
IF COL_LENGTH('dbo.CustomerDetails','UpdatedDate') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD UpdatedDate DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CustomerDetails_Company')
    ALTER TABLE dbo.CustomerDetails ADD CONSTRAINT FK_CustomerDetails_Company
        FOREIGN KEY (CompanyId) REFERENCES dbo.CustomerCompany(Id);
GO

-- Index on mobile for the lookup (not unique - several customers can share a mobile)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CustomerDetails_Phone' AND object_id = OBJECT_ID('dbo.CustomerDetails'))
    CREATE INDEX IX_CustomerDetails_Phone ON dbo.CustomerDetails(Phone);
GO

-- Backfill Status for any pre-existing rows (already defaulted)
UPDATE dbo.CustomerDetails SET Status = N'Active' WHERE Status IS NULL;
GO
