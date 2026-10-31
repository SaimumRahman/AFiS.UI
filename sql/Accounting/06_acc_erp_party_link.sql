/* ============================================================================
   06_acc_erp_party_link.sql — Option 2: explicit ERP <-> acc_party links
   Target: AFIS_MAH_LIVE (dbo)

   Adds an AccPartyId column (FK -> acc_party.party_id) to Suppliers,
   CustomerDetails and Employees, plus AFTER INSERT/UPDATE triggers that keep
   the party in sync automatically:

       ERP row inserted/updated
         -> upsert acc_party(company_id, party_type, code)   [name/phone refreshed]
         -> write party_id back into ERP.AccPartyId           [FK enforced]

   The API (AccountingRepository) then posts acc_journal_line.party_id straight
   from the linked party instead of resolving by string code.

   Idempotent and additive only (no business data dropped). Re-runnable.

   Requires: the accounting tables (01_acc_schema.sql), in particular
             dbo.acc_party and dbo.acc_company.

   Company scope: acc_party is company-scoped. @CompanyId defaults to 1
                  (single-company install). Change it if you go multi-company.

   NOTE: 99_acc_clean.sql drops acc_party; the triggers below guard on the
         table's existence, so ERP inserts keep working even if the accounting
         schema is later reset.
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------------------------------------------------------------------------
   0. Pre-flight: acc_party must exist
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.acc_party', N'U') IS NULL
BEGIN
    RAISERROR(N'[06] dbo.acc_party not found - run 01_acc_schema.sql first.', 16, 1);
    RETURN;
END
GO

/* ---------------------------------------------------------------------------
   1. AccPartyId columns
   --------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.Suppliers', N'AccPartyId') IS NULL
    ALTER TABLE dbo.Suppliers ADD AccPartyId INT NULL;

IF COL_LENGTH(N'dbo.CustomerDetails', N'AccPartyId') IS NULL
    ALTER TABLE dbo.CustomerDetails ADD AccPartyId INT NULL;

IF COL_LENGTH(N'dbo.Employees', N'AccPartyId') IS NULL
    ALTER TABLE dbo.Employees ADD AccPartyId INT NULL;
GO

/* ---------------------------------------------------------------------------
   2. Foreign keys
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Suppliers_party')
    ALTER TABLE dbo.Suppliers ADD CONSTRAINT FK_Suppliers_party
        FOREIGN KEY (AccPartyId) REFERENCES dbo.acc_party(party_id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CustomerDetails_party')
    ALTER TABLE dbo.CustomerDetails ADD CONSTRAINT FK_CustomerDetails_party
        FOREIGN KEY (AccPartyId) REFERENCES dbo.acc_party(party_id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Employees_party')
    ALTER TABLE dbo.Employees ADD CONSTRAINT FK_Employees_party
        FOREIGN KEY (AccPartyId) REFERENCES dbo.acc_party(party_id);
GO

/* ---------------------------------------------------------------------------
   3. Backfill existing rows
   --------------------------------------------------------------------------- */
DECLARE @CompanyId SMALLINT = 1;

/* ---- 3a. SUPPLIERS -> SUPPLIER (code: S-{ShortCode} else S-{Id}) ---- */
INSERT dbo.acc_party (company_id, party_type, code, name, phone, is_active)
SELECT @CompanyId, N'SUPPLIER',
       CASE WHEN LTRIM(RTRIM(ISNULL(s.ShortCode, N''))) <> N''
            THEN N'S-' + LTRIM(RTRIM(s.ShortCode))
            ELSE N'S-' + CAST(s.Id AS NVARCHAR(20)) END,
       s.Name, s.ContactNo, 1
FROM dbo.Suppliers s
WHERE s.AccPartyId IS NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.acc_party p
        WHERE p.company_id = @CompanyId AND p.party_type = N'SUPPLIER'
          AND p.code = CASE WHEN LTRIM(RTRIM(ISNULL(s.ShortCode, N''))) <> N''
                            THEN N'S-' + LTRIM(RTRIM(s.ShortCode))
                            ELSE N'S-' + CAST(s.Id AS NVARCHAR(20)) END);

UPDATE s
SET    s.AccPartyId = p.party_id
FROM   dbo.Suppliers s
JOIN   dbo.acc_party p
  ON   p.company_id = @CompanyId AND p.party_type = N'SUPPLIER'
 AND   p.code = CASE WHEN LTRIM(RTRIM(ISNULL(s.ShortCode, N''))) <> N''
                     THEN N'S-' + LTRIM(RTRIM(s.ShortCode))
                     ELSE N'S-' + CAST(s.Id AS NVARCHAR(20)) END
WHERE  s.AccPartyId IS NULL;

/* ---- 3b. CUSTOMERDETAILS -> CUSTOMER (code: C-{Id}) ---- */
INSERT dbo.acc_party (company_id, party_type, code, name, phone, is_active)
SELECT @CompanyId, N'CUSTOMER', N'C-' + CAST(c.Id AS NVARCHAR(20)),
       c.Name, c.Phone, 1
FROM dbo.CustomerDetails c
WHERE c.AccPartyId IS NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.acc_party p
        WHERE p.company_id = @CompanyId AND p.party_type = N'CUSTOMER'
          AND p.code = N'C-' + CAST(c.Id AS NVARCHAR(20)));

UPDATE c
SET    c.AccPartyId = p.party_id
FROM   dbo.CustomerDetails c
JOIN   dbo.acc_party p
  ON   p.company_id = @CompanyId AND p.party_type = N'CUSTOMER'
 AND   p.code = N'C-' + CAST(c.Id AS NVARCHAR(20))
WHERE  c.AccPartyId IS NULL;

/* ---- 3c. EMPLOYEES -> EMPLOYEE (code: E-{EmployeeCode} else E-{Id}) ---- */
INSERT dbo.acc_party (company_id, party_type, code, name, phone, is_active)
SELECT @CompanyId, N'EMPLOYEE',
       CASE WHEN LTRIM(RTRIM(ISNULL(e.EmployeeCode, N''))) <> N''
            THEN N'E-' + LTRIM(RTRIM(e.EmployeeCode))
            ELSE N'E-' + CAST(e.Id AS NVARCHAR(20)) END,
       e.Name, e.Contact, 1
FROM dbo.Employees e
WHERE e.AccPartyId IS NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.acc_party p
        WHERE p.company_id = @CompanyId AND p.party_type = N'EMPLOYEE'
          AND p.code = CASE WHEN LTRIM(RTRIM(ISNULL(e.EmployeeCode, N''))) <> N''
                            THEN N'E-' + LTRIM(RTRIM(e.EmployeeCode))
                            ELSE N'E-' + CAST(e.Id AS NVARCHAR(20)) END);

UPDATE e
SET    e.AccPartyId = p.party_id
FROM   dbo.Employees e
JOIN   dbo.acc_party p
  ON   p.company_id = @CompanyId AND p.party_type = N'EMPLOYEE'
 AND   p.code = CASE WHEN LTRIM(RTRIM(ISNULL(e.EmployeeCode, N''))) <> N''
                     THEN N'E-' + LTRIM(RTRIM(e.EmployeeCode))
                     ELSE N'E-' + CAST(e.Id AS NVARCHAR(20)) END
WHERE  e.AccPartyId IS NULL;
GO

/* ---------------------------------------------------------------------------
   4. Triggers — keep acc_party in sync + write back AccPartyId
   --------------------------------------------------------------------------- */

/* ---- 4a. Suppliers ---- */
CREATE OR ALTER TRIGGER dbo.trg_Suppliers_acc_party
ON dbo.Suppliers
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.acc_party', N'U') IS NULL RETURN;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;              -- ignore the self-update below

    DECLARE @CompanyId SMALLINT = 1;

    ;WITH src AS (
        SELECT CASE WHEN LTRIM(RTRIM(ISNULL(i.ShortCode, N''))) <> N''
                    THEN N'S-' + LTRIM(RTRIM(i.ShortCode))
                    ELSE N'S-' + CAST(i.Id AS NVARCHAR(20)) END AS party_code,
               MAX(i.Name)      AS name,
               MAX(i.ContactNo) AS phone
        FROM inserted i
        GROUP BY CASE WHEN LTRIM(RTRIM(ISNULL(i.ShortCode, N''))) <> N''
                      THEN N'S-' + LTRIM(RTRIM(i.ShortCode))
                      ELSE N'S-' + CAST(i.Id AS NVARCHAR(20)) END
    )
    MERGE dbo.acc_party AS p
    USING src AS s
       ON p.company_id = @CompanyId AND p.party_type = N'SUPPLIER' AND p.code = s.party_code
    WHEN MATCHED THEN
        UPDATE SET p.name = s.name, p.phone = s.phone
    WHEN NOT MATCHED THEN
        INSERT (company_id, party_type, code, name, phone, is_active)
        VALUES (@CompanyId, N'SUPPLIER', s.party_code, s.name, s.phone, 1);

    UPDATE s
    SET    s.AccPartyId = p.party_id
    FROM   dbo.Suppliers s
    JOIN   inserted i ON i.Id = s.Id
    JOIN   dbo.acc_party p
      ON   p.company_id = @CompanyId AND p.party_type = N'SUPPLIER'
     AND   p.code = CASE WHEN LTRIM(RTRIM(ISNULL(s.ShortCode, N''))) <> N''
                         THEN N'S-' + LTRIM(RTRIM(s.ShortCode))
                         ELSE N'S-' + CAST(s.Id AS NVARCHAR(20)) END;
END
GO

/* ---- 4b. CustomerDetails ---- */
CREATE OR ALTER TRIGGER dbo.trg_CustomerDetails_acc_party
ON dbo.CustomerDetails
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.acc_party', N'U') IS NULL RETURN;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    DECLARE @CompanyId SMALLINT = 1;

    ;WITH src AS (
        SELECT N'C-' + CAST(i.Id AS NVARCHAR(20)) AS party_code,
               MAX(i.Name)  AS name,
               MAX(i.Phone) AS phone
        FROM inserted i
        GROUP BY N'C-' + CAST(i.Id AS NVARCHAR(20))
    )
    MERGE dbo.acc_party AS p
    USING src AS s
       ON p.company_id = @CompanyId AND p.party_type = N'CUSTOMER' AND p.code = s.party_code
    WHEN MATCHED THEN
        UPDATE SET p.name = s.name, p.phone = s.phone
    WHEN NOT MATCHED THEN
        INSERT (company_id, party_type, code, name, phone, is_active)
        VALUES (@CompanyId, N'CUSTOMER', s.party_code, s.name, s.phone, 1);

    UPDATE c
    SET    c.AccPartyId = p.party_id
    FROM   dbo.CustomerDetails c
    JOIN   inserted i ON i.Id = c.Id
    JOIN   dbo.acc_party p
      ON   p.company_id = @CompanyId AND p.party_type = N'CUSTOMER'
     AND   p.code = N'C-' + CAST(c.Id AS NVARCHAR(20));
END
GO

/* ---- 4c. Employees ---- */
CREATE OR ALTER TRIGGER dbo.trg_Employees_acc_party
ON dbo.Employees
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.acc_party', N'U') IS NULL RETURN;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    DECLARE @CompanyId SMALLINT = 1;

    ;WITH src AS (
        SELECT CASE WHEN LTRIM(RTRIM(ISNULL(i.EmployeeCode, N''))) <> N''
                    THEN N'E-' + LTRIM(RTRIM(i.EmployeeCode))
                    ELSE N'E-' + CAST(i.Id AS NVARCHAR(20)) END AS party_code,
               MAX(i.Name)    AS name,
               MAX(i.Contact) AS phone
        FROM inserted i
        GROUP BY CASE WHEN LTRIM(RTRIM(ISNULL(i.EmployeeCode, N''))) <> N''
                      THEN N'E-' + LTRIM(RTRIM(i.EmployeeCode))
                      ELSE N'E-' + CAST(i.Id AS NVARCHAR(20)) END
    )
    MERGE dbo.acc_party AS p
    USING src AS s
       ON p.company_id = @CompanyId AND p.party_type = N'EMPLOYEE' AND p.code = s.party_code
    WHEN MATCHED THEN
        UPDATE SET p.name = s.name, p.phone = s.phone
    WHEN NOT MATCHED THEN
        INSERT (company_id, party_type, code, name, phone, is_active)
        VALUES (@CompanyId, N'EMPLOYEE', s.party_code, s.name, s.phone, 1);

    UPDATE e
    SET    e.AccPartyId = p.party_id
    FROM   dbo.Employees e
    JOIN   inserted i ON i.Id = e.Id
    JOIN   dbo.acc_party p
      ON   p.company_id = @CompanyId AND p.party_type = N'EMPLOYEE'
     AND   p.code = CASE WHEN LTRIM(RTRIM(ISNULL(e.EmployeeCode, N''))) <> N''
                         THEN N'E-' + LTRIM(RTRIM(e.EmployeeCode))
                         ELSE N'E-' + CAST(e.Id AS NVARCHAR(20)) END;
END
GO

PRINT N'[06] ERP <-> acc_party links, triggers and backfill complete.';
GO
