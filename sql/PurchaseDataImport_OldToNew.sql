/* ============================================================================
   AFiS ERP - Purchase data import : OLD database  ->  NEW database
   ----------------------------------------------------------------------------
   Source (old) : fiaznewlive      (on ANOTHER SQL Server, read via raw connection)
   Target (new) : the database this script is run in (see USE below)

   The source tables are first copied into a local staging schema (imp) through
   an OPENDATASOURCE connection string. The import itself then runs locally, so
   no linked server is needed and no distributed transaction/MSDTC is involved.
   The staging schema is dropped after a successful run.

   WHAT THIS SCRIPT DOES
   ---------------------
   1. Copies the old purchase history into the (empty) target database:
        Suppliers, Items + all their lookup masters, Purchases, PurchaseItems,
        SupplierPayments, BatchNoSeq.
   2. Optionally copies PurchaseDrafts, Stock (snapshot + purchase ledger rows)
      and the posted Purchase accounting journals (defaults OFF).
   3. REMAPS every lookup id (Colors, Sizes, Groups, SubGroups, Designs,
      MeasurementUnits, Brands, Origins, Catalogue) because the two databases
      use different ids and different master data. Missing lookups are created
      in the target with new ids; unresolvable references are set to NULL (or
      to an "UNMAPPED" fallback where the column is NOT NULL).
   4. Preserves the transactional ids (Item, Supplier, Purchase, PurchaseItem,
      SupplierPayment, Stock ...) so all parent/child links stay intact.
   5. Re-seeds every identity column and the ProductSerialSequence.
   6. Runs reconciliation / orphan checks at the end.

   HOW TO USE
   ----------
   A. Create the target database. Recommended:
        1) take a FULL backup of AFIS_MAH_LIVE
        2) restore it as the new database (e.g. AFIS_MAH_NEWUSER)
        3) run  sql\FreshStart_DBCleanup.sql  in that new database
      This keeps the users / groups / routes / stores / accounting seed that
      the application needs, and clears all transactions.
   B. Change the USE line below to your new database name.
   C. Change the @SrcServer / @SrcDb / @SrcUser / @SrcPassword / @SrcProvider
      values below to match the old server.
   D. Review the three @Include... switches.
   E. Take a backup of the target, then run this script (it is one transaction,
      so any error rolls everything back).
   F. Check the validation output at the bottom.

   IMPORTANT
   ---------
   * The target must be EMPTY for Items, Suppliers, Purchases, PurchaseItems,
     SupplierPayments (and drafts/stock/journals if those blocks are enabled).
     The script aborts otherwise - it will never merge into existing data.
   * A truly schema-only database (no rows at all) cannot run this script
     because it has no application seed (users, groups, routes, accounting).
     Use the backup + FreshStart route above.
   * All inserts use explicit column lists, so the 3 extra nullable columns in
     the new SupplierPayments table are handled explicitly.
   * Old data contains many 0 / dangling lookup references. Those become NULL
     (nullable columns) or the fallback lookup rows created by this script.
   * Old = different business master data (cosmetics etc.). Old lookups are
     ADDED to the target; the target's own master data is never deleted.
   ============================================================================ */

USE [AFIS_MAH_LIVE_NEW];   -- <<<<<<<<<< CHANGE THIS to the new database name
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------------------------------------------------------------------------
   CONFIGURATION
   --------------------------------------------------------------------------- */
DECLARE @OldDb            sysname = N'fiaznewlive';  -- old/source database
DECLARE @IncludeDrafts    bit     = 0;  -- 1 = also import PurchaseDrafts / items (72 / 933)
DECLARE @IncludeStock     bit     = 0;  -- 1 = also import StockMaster snapshot + purchase StockDetail
DECLARE @IncludeAccounting bit    = 0;  -- 1 = also import the posted PUR / REVERSAL journals
DECLARE @FallbackGroupName    nvarchar(200) = N'IMPORTED (OLD DB)';
DECLARE @FallbackSubGroupName nvarchar(200) = N'UNMAPPED';
DECLARE @msg nvarchar(2048);

/* ---------------------------------------------------------------------------
   SOURCE CONNECTION + LOCAL STAGING
   ---------------------------------------------------------------------------
   fiaznewlive lives on ANOTHER SQL Server. We therefore pull the source tables
   through a raw OLE DB connection string (OPENDATASOURCE) into a local staging
   schema (imp) first. The import then runs entirely inside this database, so
   it is one local transaction with no linked server and no MSDTC dependency.

   Change the five @Src... values below if the source server/database/login
   changes. MSOLEDBSQL19 is the OLE DB driver shipped with SQL Server 2025;
   use N'MSOLEDBSQL' if your server only has the older driver.
   --------------------------------------------------------------------------- */
DECLARE @SrcServer   nvarchar(200) = N'202.4.117.56,1435';
DECLARE @SrcDb       sysname       = N'fiaznewlive';
DECLARE @SrcUser     nvarchar(100) = N'sa';
DECLARE @SrcPassword nvarchar(200) = N'Sobuj@4321';
DECLARE @SrcProvider nvarchar(50)  = N'MSOLEDBSQL19';

DECLARE @SrcConn nvarchar(1000) =
    N'Data Source=' + @SrcServer + N';Initial Catalog=' + @SrcDb +
    N';User ID=' + @SrcUser + N';Password=' + @SrcPassword +
    N';Use Encryption for Data=False;TrustServerCertificate=True;';

DECLARE @Src nvarchar(4000) =
    N'OPENDATASOURCE(''' + @SrcProvider + N''',''' +
    REPLACE(@SrcConn, N'''', N'''''') + N''')';

DECLARE @adhocWasOn bit =
    (SELECT CAST(value_in_use AS bit) FROM sys.configurations
     WHERE name = N'Ad Hoc Distributed Queries');
DECLARE @advWasOn bit =
    (SELECT CAST(value_in_use AS bit) FROM sys.configurations
     WHERE name = N'show advanced options');
DECLARE @sql nvarchar(max);

BEGIN TRY
    /* --- enable ad hoc distributed queries just for the staging pull ------ */
    IF @adhocWasOn = 0
    BEGIN
        IF @advWasOn = 0
        BEGIN
            EXEC sp_configure N'show advanced options', 1;
            RECONFIGURE;
        END;
        EXEC sp_configure N'Ad Hoc Distributed Queries', 1;
        RECONFIGURE;
    END;

    /* --- fresh local staging schema -------------------------------------- */
    IF SCHEMA_ID(N'imp') IS NOT NULL
    BEGIN
        SET @sql = N'';
        SELECT @sql = @sql + N'DROP TABLE imp.' + QUOTENAME(name) + N';'
        FROM sys.tables WHERE schema_id = SCHEMA_ID(N'imp');
        IF @sql <> N'' EXEC (@sql);
        EXEC (N'DROP SCHEMA imp;');
    END;
    EXEC (N'CREATE SCHEMA imp;');

    DECLARE @stageTbl TABLE (tbl sysname NOT NULL PRIMARY KEY);
    INSERT @stageTbl (tbl) VALUES
        (N'Stores'), (N'Colors'), (N'Sizes'), (N'MesurementUnits'),
        (N'ItemBrand'), (N'ItemOrigin'), (N'ItemCatalogue'), (N'ItemFeatures'),
        (N'Groups'), (N'SubGroups'), (N'Designs'), (N'Banks'),
        (N'Items'), (N'Suppliers'), (N'Purchases'), (N'PurchaseItems'),
        (N'SupplierPayments'), (N'BatchNoSeq'),
        (N'PurchaseDrafts'), (N'PurchaseDraftItems'),
        (N'StockMaster'), (N'StockDetail'),
        (N'acc_account'), (N'acc_party'),
        (N'acc_journal_entry'), (N'acc_journal_line');

    DECLARE @stageTable sysname;
    DECLARE stage_cur CURSOR LOCAL FAST_FORWARD FOR SELECT tbl FROM @stageTbl;
    OPEN stage_cur;
    FETCH NEXT FROM stage_cur INTO @stageTable;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'SELECT * INTO imp.' + QUOTENAME(@stageTable) +
                   N' FROM ' + @Src + N'.' + QUOTENAME(@SrcDb) +
                   N'.dbo.' + QUOTENAME(@stageTable) + N';';
        EXEC (@sql);
        FETCH NEXT FROM stage_cur INTO @stageTable;
    END;
    CLOSE stage_cur;
    DEALLOCATE stage_cur;

    /* ProductSerialSequence lives in source sys.sequences */
    SET @sql =
        N'SELECT name, CONVERT(bigint, current_value) AS current_value ' +
        N'INTO imp.SourceSequences FROM ' + @Src + N'.' +
        QUOTENAME(@SrcDb) + N'.sys.sequences;';
    EXEC (@sql);

    /* --- restore the server option we may have changed -------------------- */
    IF @adhocWasOn = 0
    BEGIN
        EXEC sp_configure N'Ad Hoc Distributed Queries', 0;
        RECONFIGURE;
        IF @advWasOn = 0
        BEGIN
            EXEC sp_configure N'show advanced options', 0;
            RECONFIGURE;
        END;
    END;
END TRY
BEGIN CATCH
    BEGIN TRY
        IF @adhocWasOn = 0
        BEGIN
            EXEC sp_configure N'Ad Hoc Distributed Queries', 0;
            RECONFIGURE;
            IF @advWasOn = 0
            BEGIN
                EXEC sp_configure N'show advanced options', 0;
                RECONFIGURE;
            END;
        END;
    END TRY
    BEGIN CATCH
        /* ignore restore errors */
    END CATCH;
    BEGIN TRY
        SET @sql = N'';
        SELECT @sql = @sql + N'DROP TABLE imp.' + QUOTENAME(name) + N';'
        FROM sys.tables WHERE schema_id = SCHEMA_ID(N'imp');
        IF @sql <> N'' EXEC (@sql);
        EXEC (N'DROP SCHEMA imp;');
    END TRY
    BEGIN CATCH
        /* ignore staging cleanup errors */
    END CATCH;
    THROW;
END CATCH;

IF OBJECT_ID(N'imp.Purchases') IS NULL
BEGIN
    SET @msg = N'Staging failed: imp.Purchases was not created. Check the source server/connection string/provider.';
    THROW 50001, @msg, 1;
END;

DECLARE @stagedPurchases int = (SELECT COUNT(*) FROM imp.Purchases);
PRINT N'Source staged locally: ' + CAST(@stagedPurchases AS nvarchar(20)) + N' purchases.';

/* ---------------------------------------------------------------------------
   PRE-FLIGHT CHECKS
   (wrapped so a failed check also removes the local staging schema)
   --------------------------------------------------------------------------- */
BEGIN TRY
IF DB_NAME() = @OldDb
BEGIN
    SET @msg = N'This script is running inside the OLD database. Run it in the NEW database.';
    THROW 50000, @msg, 1;
END;

IF OBJECT_ID(N'imp.Purchases') IS NULL
BEGIN
    SET @msg = N'Staging table imp.Purchases is missing. The source server/connection string could not be read.';
    THROW 50001, @msg, 1;
END;

IF DB_NAME() NOT IN (N'AFIS_MAH_LIVE_NEW')
    PRINT N'WARNING: current database is ' + QUOTENAME(DB_NAME())
        + N'. Make sure this is the NEW database, not the live one.';

/* --- target must be empty for the tables we populate --------------------- */
IF EXISTS (SELECT 1 FROM dbo.Purchases)
   OR EXISTS (SELECT 1 FROM dbo.PurchaseItems)
   OR EXISTS (SELECT 1 FROM dbo.SupplierPayments)
BEGIN
    SET @msg = N'Target already has purchase data. Restore/clear it before running this script. Nothing was changed.';
    THROW 50002, @msg, 1;
END;

IF EXISTS (SELECT 1 FROM dbo.Items) OR EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    SET @msg = N'Target already has Items and/or Suppliers. This script imports them from the old DB and needs them empty. Nothing was changed.';
    THROW 50003, @msg, 1;
END;

IF @IncludeDrafts = 1 AND (EXISTS (SELECT 1 FROM dbo.PurchaseDrafts) OR EXISTS (SELECT 1 FROM dbo.PurchaseDraftItems))
BEGIN
    SET @msg = N'@IncludeDrafts = 1 but the target already has purchase drafts. Nothing was changed.';
    THROW 50004, @msg, 1;
END;

IF @IncludeStock = 1 AND (EXISTS (SELECT 1 FROM dbo.StockMaster) OR EXISTS (SELECT 1 FROM dbo.StockDetail))
BEGIN
    SET @msg = N'@IncludeStock = 1 but the target already has stock rows. Nothing was changed.';
    THROW 50005, @msg, 1;
END;

IF @IncludeAccounting = 1 AND EXISTS (SELECT 1 FROM dbo.acc_journal_entry)
BEGIN
    SET @msg = N'@IncludeAccounting = 1 but the target already has journal entries. Nothing was changed.';
    THROW 50006, @msg, 1;
END;

/* --- application seed must exist (schema-only DBs will fail here) -------- */
IF NOT EXISTS (SELECT 1 FROM dbo.core_users)
   OR NOT EXISTS (SELECT 1 FROM dbo.Company)
   OR NOT EXISTS (SELECT 1 FROM dbo.Groups)
   OR NOT EXISTS (SELECT 1 FROM dbo.Routes)
   OR NOT EXISTS (SELECT 1 FROM dbo.Stores)
   OR NOT EXISTS (SELECT 1 FROM dbo.acc_company)
   OR NOT EXISTS (SELECT 1 FROM dbo.acc_account)
   OR NOT EXISTS (SELECT 1 FROM dbo.acc_voucher_type)
   OR NOT EXISTS (SELECT 1 FROM dbo.acc_fiscal_year)
   OR NOT EXISTS (SELECT 1 FROM dbo.acc_fiscal_period)
BEGIN
    SET @msg = N'Target has no application seed data (users/groups/routes/stores/accounting). '
             + N'Create it from a FULL backup of AFIS_MAH_LIVE + sql\FreshStart_DBCleanup.sql, '
             + N'then run this script. A schema-only database cannot run the application.';
    THROW 50007, @msg, 1;
END;

/* --- old data integrity checks ------------------------------------------- */
IF EXISTS (SELECT 1 FROM imp.PurchaseItems pi
           WHERE NOT EXISTS (SELECT 1 FROM imp.Items i WHERE i.Id = pi.ItemId))
BEGIN
    SET @msg = N'Old PurchaseItems contain item ids that do not exist in old Items. Fix the source before importing.';
    THROW 50008, @msg, 1;
END;

IF EXISTS (SELECT 1 FROM imp.Purchases p
           WHERE NOT EXISTS (SELECT 1 FROM imp.Suppliers s WHERE s.Id = p.SupplierId))
BEGIN
    SET @msg = N'Old Purchases reference supplier ids that do not exist in old Suppliers.';
    THROW 50009, @msg, 1;
END;

IF EXISTS (SELECT 1 FROM imp.Purchases p
           WHERE NOT EXISTS (SELECT 1 FROM imp.Stores s WHERE s.Id = p.StoreId))
BEGIN
    SET @msg = N'Old Purchases reference store ids that do not exist in old Stores.';
    THROW 50010, @msg, 1;
END;

IF EXISTS (SELECT 1 FROM (SELECT SystemInvoiceNo FROM imp.Purchases
                          WHERE SystemInvoiceNo IS NOT NULL
                          GROUP BY SystemInvoiceNo HAVING COUNT(*) > 1) d)
BEGIN
    SET @msg = N'Old Purchases contain duplicate SystemInvoiceNo values; the target has a unique index on it.';
    THROW 50011, @msg, 1;
END;

PRINT N'Pre-flight checks passed. Starting import...';
END TRY
BEGIN CATCH
    BEGIN TRY
        SET @sql = N'';
        SELECT @sql = @sql + N'DROP TABLE imp.' + QUOTENAME(name) + N';'
        FROM sys.tables WHERE schema_id = SCHEMA_ID(N'imp');
        IF @sql <> N'' EXEC (@sql);
        EXEC (N'DROP SCHEMA imp;');
    END TRY
    BEGIN CATCH
        /* ignore */
    END CATCH;
    THROW;
END CATCH;

/* ---------------------------------------------------------------------------
   MAIN TRANSACTION
   --------------------------------------------------------------------------- */
BEGIN TRY
    BEGIN TRANSACTION;

    /* =======================================================================
       1. STORES
       FreshStart-based targets already have them; a blank target gets them.
       ======================================================================= */
    IF NOT EXISTS (SELECT 1 FROM dbo.Stores)
    BEGIN
        SET IDENTITY_INSERT dbo.Stores ON;

        INSERT dbo.Stores
            (Id, Name, Code, Address, Contact, Email, VAT, TIN, LetterHeadFile,
             UseLetterHead, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy,
             FinancialAccountId, CostCenterId, ExchangeDay, TermsOfService,
             DailyExpenseLimit)
        SELECT
             Id, Name, Code, Address, Contact, Email, VAT, TIN, LetterHeadFile,
             UseLetterHead, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy,
             FinancialAccountId, CostCenterId, ExchangeDay, TermsOfService,
             DailyExpenseLimit
        FROM imp.Stores;

        SET IDENTITY_INSERT dbo.Stores OFF;
        PRINT N'Stores copied.';
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM imp.Purchases p
                   WHERE NOT EXISTS (SELECT 1 FROM dbo.Stores s WHERE s.Id = p.StoreId))
        BEGIN
            SET @msg = N'Target Stores is missing a store id used by the old purchases. Add that store first.';
            THROW 50012, @msg, 1;
        END;
        PRINT N'Stores already present - kept as-is.';
    END;

    /* =======================================================================
       2. LOOKUP MASTERS  (old ids -> new ids)
       Matching is by normalised name. Missing rows are inserted into the
       target with new ids. The target's existing lookups are never deleted.
       ======================================================================= */
    CREATE TABLE #mapColor      (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapSize       (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapUnit       (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapBrand      (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapOrigin     (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapBank       (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapCatalogue  (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapFeature    (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapGroup      (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapSubGroup   (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
    CREATE TABLE #mapDesign     (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);

    /* ---- Colors ---------------------------------------------------------- */
    INSERT dbo.Colors (Name, ColorCode, HexCode)
    SELECT s.Name, s.ColorCode, s.HexCode
    FROM (
        SELECT o.Name, o.ColorCode, o.HexCode,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))) ORDER BY o.Id) AS rn
        FROM imp.Colors o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Colors t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapColor (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.Colors o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.Colors GROUP BY LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* ---- Sizes ----------------------------------------------------------- */
    INSERT dbo.Sizes (Name)
    SELECT s.Name
    FROM (
        SELECT o.Name,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))) ORDER BY o.Id) AS rn
        FROM imp.Sizes o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Sizes t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapSize (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.Sizes o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.Sizes GROUP BY LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* ---- Measurement units ---------------------------------------------- */
    INSERT dbo.MesurementUnits (Name)
    SELECT s.Name
    FROM (
        SELECT o.Name,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))) ORDER BY o.Id) AS rn
        FROM imp.MesurementUnits o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.MesurementUnits t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapUnit (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.MesurementUnits o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.MesurementUnits GROUP BY LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* ---- Brands ---------------------------------------------------------- */
    INSERT dbo.ItemBrand (BrandName, Description)
    SELECT s.BrandName, s.Description
    FROM (
        SELECT o.BrandName, o.Description,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.BrandName, 400)))) ORDER BY o.BrandId) AS rn
        FROM imp.ItemBrand o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.ItemBrand t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.BrandName, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.BrandName, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapBrand (old_id, new_id)
    SELECT o.BrandId, t.Id
    FROM imp.ItemBrand o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(BrandName, 400)))) AS k, MIN(BrandId) AS Id
        FROM dbo.ItemBrand GROUP BY LOWER(LTRIM(RTRIM(LEFT(BrandName, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.BrandName, 400))));

    /* ---- Origins ---------------------------------------------------------
       The old table contains duplicate OriginId values (explicit identity
       inserts), so first collapse it to one name per id - preferring a name
       that already exists in the target. */
    CREATE TABLE #srcOrigin (old_id int NOT NULL, origin_name nvarchar(400) NOT NULL,
                             description nvarchar(400) NULL, rn int NOT NULL);

    INSERT #srcOrigin (old_id, origin_name, description, rn)
    SELECT o.OriginId, o.OriginName, o.Description,
           ROW_NUMBER() OVER (
               PARTITION BY o.OriginId
               ORDER BY CASE WHEN EXISTS (
                            SELECT 1 FROM dbo.ItemOrigin t
                            WHERE LOWER(LTRIM(RTRIM(LEFT(t.OriginName, 400)))) =
                                  LOWER(LTRIM(RTRIM(LEFT(o.OriginName, 400))))) THEN 0 ELSE 1 END,
                        o.OriginName)
    FROM imp.ItemOrigin o;

    INSERT dbo.ItemOrigin (OriginName, Description)
    SELECT s.origin_name, s.description
    FROM #srcOrigin s
    WHERE s.rn = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.ItemOrigin t
          WHERE LOWER(LTRIM(RTRIM(LEFT(t.OriginName, 400)))) =
                LOWER(LTRIM(RTRIM(LEFT(s.origin_name, 400)))));

    INSERT #mapOrigin (old_id, new_id)
    SELECT s.old_id, t.Id
    FROM #srcOrigin s
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(OriginName, 400)))) AS k, MIN(OriginId) AS Id
        FROM dbo.ItemOrigin GROUP BY LOWER(LTRIM(RTRIM(LEFT(OriginName, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(s.origin_name, 400))))
    WHERE s.rn = 1;

    /* ---- Banks (old has duplicate names, no duplicate id needed) --------- */
    INSERT #mapBank (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.Banks o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.Banks GROUP BY LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));
    /* ---- Item catalogue -------------------------------------------------- */
    INSERT dbo.ItemCatalogue (CatalogueName, IsActive, CreatedDate, CreatedBy, LastModifiedDate, LastModifiedBy)
    SELECT s.CatalogueName, s.IsActive, s.CreatedDate, s.CreatedBy, s.LastModifiedDate, s.LastModifiedBy
    FROM (
        SELECT o.CatalogueName, o.IsActive, o.CreatedDate, o.CreatedBy, o.LastModifiedDate, o.LastModifiedBy,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.CatalogueName, 400)))) ORDER BY o.CatalogueId) AS rn
        FROM imp.ItemCatalogue o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.ItemCatalogue t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.CatalogueName, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.CatalogueName, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapCatalogue (old_id, new_id)
    SELECT o.CatalogueId, t.Id
    FROM imp.ItemCatalogue o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(CatalogueName, 400)))) AS k, MIN(CatalogueId) AS Id
        FROM dbo.ItemCatalogue GROUP BY LOWER(LTRIM(RTRIM(LEFT(CatalogueName, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.CatalogueName, 400))));

    /* ---- Item features (old has none today, kept for completeness) ------- */
    INSERT dbo.ItemFeatures (FeatureName, Description)
    SELECT s.FeatureName, s.Description
    FROM (
        SELECT o.FeatureName, o.Description,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.FeatureName, 400)))) ORDER BY o.FeatureId) AS rn
        FROM imp.ItemFeatures o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.ItemFeatures t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.FeatureName, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.FeatureName, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapFeature (old_id, new_id)
    SELECT o.FeatureId, t.Id
    FROM imp.ItemFeatures o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(FeatureName, 400)))) AS k, MIN(FeatureId) AS Id
        FROM dbo.ItemFeatures GROUP BY LOWER(LTRIM(RTRIM(LEFT(FeatureName, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.FeatureName, 400))));

    /* ---- Groups ---------------------------------------------------------- */
    INSERT dbo.Groups (Name, Code, VAT)
    SELECT s.Name, 'IMP-' + CAST(s.Id AS nvarchar(20)), 0
    FROM (
        SELECT o.Id, o.Name,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))) ORDER BY o.Id) AS rn
        FROM imp.Groups o
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Groups t
            WHERE LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapGroup (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.Groups o
    JOIN (
        SELECT LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.Groups GROUP BY LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* fallback group for orphan references */
    IF NOT EXISTS (SELECT 1 FROM dbo.Groups WHERE Name = @FallbackGroupName)
        INSERT dbo.Groups (Name, Code, VAT) VALUES (@FallbackGroupName, 'IMP-000', 0);

    DECLARE @fallbackGroup int = (SELECT MIN(Id) FROM dbo.Groups WHERE Name = @FallbackGroupName);

    /* ---- Sub-groups ------------------------------------------------------ */
    INSERT dbo.SubGroups (Name, GroupId, Code)
    SELECT s.Name, s.new_group, 'IMP-' + CAST(s.Id AS nvarchar(20))
    FROM (
        SELECT o.Id, o.Name, mg.new_id AS new_group,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))), mg.new_id ORDER BY o.Id) AS rn
        FROM imp.SubGroups o
        JOIN #mapGroup mg ON mg.old_id = o.GroupId
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.SubGroups t
            WHERE t.GroupId = mg.new_id
              AND LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapSubGroup (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.SubGroups o
    JOIN #mapGroup mg ON mg.old_id = o.GroupId
    JOIN (
        SELECT GroupId, LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.SubGroups GROUP BY GroupId, LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.GroupId = mg.new_id
       AND t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* fallback sub-group for orphan references */
    IF NOT EXISTS (SELECT 1 FROM dbo.SubGroups WHERE GroupId = @fallbackGroup AND Name = @FallbackSubGroupName)
        INSERT dbo.SubGroups (Name, GroupId, Code) VALUES (@FallbackSubGroupName, @fallbackGroup, 'IMP-UNM');

    DECLARE @fallbackSubGroup int =
        (SELECT MIN(Id) FROM dbo.SubGroups WHERE GroupId = @fallbackGroup AND Name = @FallbackSubGroupName);

    /* ---- Designs --------------------------------------------------------- */
    INSERT dbo.Designs (Name, Code, SubGroupId)
    SELECT s.Name, 'IMP-' + CAST(s.Id AS nvarchar(20)), s.new_sg
    FROM (
        SELECT o.Id, o.Name,
               COALESCE(ms.new_id, @fallbackSubGroup) AS new_sg,
               ROW_NUMBER() OVER (PARTITION BY LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))),
                                               COALESCE(ms.new_id, @fallbackSubGroup)
                                  ORDER BY o.Id) AS rn
        FROM imp.Designs o
        LEFT JOIN #mapSubGroup ms ON ms.old_id = o.SubGroupId
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Designs t
            WHERE t.SubGroupId = COALESCE(ms.new_id, @fallbackSubGroup)
              AND LOWER(LTRIM(RTRIM(LEFT(t.Name, 400)))) = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400)))))
    ) s
    WHERE s.rn = 1;

    INSERT #mapDesign (old_id, new_id)
    SELECT o.Id, t.Id
    FROM imp.Designs o
    LEFT JOIN #mapSubGroup ms ON ms.old_id = o.SubGroupId
    JOIN (
        SELECT SubGroupId, LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) AS k, MIN(Id) AS Id
        FROM dbo.Designs GROUP BY SubGroupId, LOWER(LTRIM(RTRIM(LEFT(Name, 400))))
    ) t ON t.SubGroupId = COALESCE(ms.new_id, @fallbackSubGroup)
       AND t.k = LOWER(LTRIM(RTRIM(LEFT(o.Name, 400))));

    /* fallback measurement unit (PCS) */
    DECLARE @fallbackUnit int = (SELECT MIN(Id) FROM dbo.MesurementUnits
                                 WHERE LOWER(LTRIM(RTRIM(LEFT(Name, 400)))) = N'pcs');
    IF @fallbackUnit IS NULL
        SET @fallbackUnit = (SELECT MIN(Id) FROM dbo.MesurementUnits);

    PRINT N'Lookup masters mapped / created.';

    /* =======================================================================
       3. ITEMS  (ids preserved, FK lookups remapped; 0/orphan refs -> NULL)
       ======================================================================= */
    SET IDENTITY_INSERT dbo.Items ON;

    INSERT dbo.Items
        (Id, GroupId, SubGroupId, DesignId, Name, AlarmLevel, Barcode, SupplierId,
         ImageBase64, SalePrice, WholeSalePrice, MesurementUnitId,
         CountStockByColor, CountStockBySize, RawMaterial, FinishedGood,
         LastCostPrice, ShadeNo, MaterialType, Origin, Features, BrandColor,
         ProductPricePercentage, ProductType, Catalogue, BrandId, OriginId,
         ItemWiseFeatureId, ColorId, SizeId, CatalogueId, IsConsume, IsSaleable,
         ItemCode, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate,
         IsDeletedBy, DeletedBy, DeletedDate)
    SELECT
         o.Id,
         COALESCE(mg.new_id, @fallbackGroup),
         COALESCE(ms.new_id, @fallbackSubGroup),
         md.new_id,                                  -- nullable
         o.Name,
         o.AlarmLevel,
         o.Barcode,
         CASE WHEN s.Id IS NULL THEN NULL ELSE o.SupplierId END,
         o.ImageBase64,
         o.SalePrice,
         o.WholeSalePrice,
         COALESCE(mu.new_id, @fallbackUnit),
         o.CountStockByColor,
         o.CountStockBySize,
         o.RawMaterial,
         o.FinishedGood,
         o.LastCostPrice,
         o.ShadeNo,
         o.MaterialType,
         o.Origin,
         o.Features,
         o.BrandColor,
         o.ProductPricePercentage,
         o.ProductType,
         o.Catalogue,
         mb.new_id,                                  -- nullable
         mo.new_id,                                  -- nullable
         NULL,                                       -- ItemWiseFeatureId (old has no feature links)
         mc.new_id,                                  -- nullable
         mz.new_id,                                  -- nullable
         mcat.new_id,                                -- nullable
         o.IsConsume,
         o.IsSaleable,
         o.ItemCode,
         o.CreatedBy,
         o.CreatedDate,
         o.UpdatedBy,
         o.UpdatedDate,
         o.IsDeletedBy,
         o.DeletedBy,
         o.DeletedDate
    FROM imp.Items o
    LEFT JOIN #mapGroup     mg   ON mg.old_id   = o.GroupId
    LEFT JOIN #mapSubGroup  ms   ON ms.old_id   = o.SubGroupId
    LEFT JOIN #mapDesign    md   ON md.old_id   = o.DesignId
    LEFT JOIN #mapColor     mc   ON mc.old_id   = o.ColorId
    LEFT JOIN #mapSize      mz   ON mz.old_id   = o.SizeId
    LEFT JOIN #mapUnit      mu   ON mu.old_id   = o.MesurementUnitId
    LEFT JOIN #mapBrand     mb   ON mb.old_id   = o.BrandId
    LEFT JOIN #mapOrigin    mo   ON mo.old_id   = o.OriginId
    LEFT JOIN #mapCatalogue mcat ON mcat.old_id = o.CatalogueId
    LEFT JOIN dbo.Suppliers s    ON s.Id        = o.SupplierId;

    SET IDENTITY_INSERT dbo.Items OFF;
    PRINT N'Items copied.';

    /* =======================================================================
       4. SUPPLIERS
       Trigger trg_Suppliers_acc_party creates the matching acc_party rows and
       writes AccPartyId back automatically (no id collision with target).
       ======================================================================= */
    SET IDENTITY_INSERT dbo.Suppliers ON;

    INSERT dbo.Suppliers (Id, Name, ShortCode, Address, ContactNo, SupplierType, BlockDue)
    SELECT Id, Name, ShortCode, Address, ContactNo, SupplierType, BlockDue
    FROM imp.Suppliers;

    SET IDENTITY_INSERT dbo.Suppliers OFF;

    IF EXISTS (SELECT 1 FROM dbo.Suppliers WHERE AccPartyId IS NULL)
        PRINT N'WARNING: some suppliers have no AccPartyId (check trg_Suppliers_acc_party).';

    PRINT N'Suppliers copied.';

    /* =======================================================================
       5. PURCHASES + PURCHASE ITEMS
       ======================================================================= */
    SET IDENTITY_INSERT dbo.Purchases ON;

    INSERT dbo.Purchases
        (Id, StorePurchaseId, PurchaseDate, SupplierId, InvoiceNo, PurchaseOrderId,
         StoreId, UserName, EditedBy, EditedDate, Discount, DiscountPercentage,
         VAT, VATPercentage, LabourCharge, TransportCost, Total, NetTotal,
         IsVatIncluded, OtherCostTotal, CarryingCostTotal, OperationalCostTotal,
         BillInvoiceNumber, BillInvoiceName, CreatedDate, SystemInvoiceNo,
         IsOpenningStock, BillDate, Remarks)
    SELECT
         Id, StorePurchaseId, PurchaseDate, SupplierId, InvoiceNo, PurchaseOrderId,
         StoreId, UserName, EditedBy, EditedDate, Discount, DiscountPercentage,
         VAT, VATPercentage, LabourCharge, TransportCost, Total, NetTotal,
         IsVatIncluded, OtherCostTotal, CarryingCostTotal, OperationalCostTotal,
         BillInvoiceNumber, BillInvoiceName, CreatedDate, SystemInvoiceNo,
         IsOpenningStock, BillDate, Remarks
    FROM imp.Purchases;

    SET IDENTITY_INSERT dbo.Purchases OFF;
    PRINT N'Purchases copied.';

    SET IDENTITY_INSERT dbo.PurchaseItems ON;

    INSERT dbo.PurchaseItems
        (Id, PurchaseId, ItemId, Quantity, TradePrice, ColorId, SizeId, Barcode,
         MRP, ShadeNo, IsNewItem, PurchasePrice, OtherCost, CarryingCost,
         OperationalCost, VatPercentage, VatAmount, TotalAmount, IsSaleable,
         SalePrice, ProductType, IsActive, ProductPricePercentage, DesignId,
         ImageBase64, ReturnRefNo)
    SELECT
         pi.Id, pi.PurchaseId, pi.ItemId, pi.Quantity, pi.TradePrice,
         mc.new_id,                                  -- nullable
         mz.new_id,                                  -- nullable
         pi.Barcode, pi.MRP, pi.ShadeNo, pi.IsNewItem, pi.PurchasePrice,
         pi.OtherCost, pi.CarryingCost, pi.OperationalCost, pi.VatPercentage,
         pi.VatAmount, pi.TotalAmount, pi.IsSaleable, pi.SalePrice,
         pi.ProductType, pi.IsActive, pi.ProductPricePercentage,
         md.new_id,                                  -- nullable
         pi.ImageBase64, pi.ReturnRefNo
    FROM imp.PurchaseItems pi
    LEFT JOIN #mapColor  mc ON mc.old_id = pi.ColorId
    LEFT JOIN #mapSize   mz ON mz.old_id = pi.SizeId
    LEFT JOIN #mapDesign md ON md.old_id = pi.DesignId;

    SET IDENTITY_INSERT dbo.PurchaseItems OFF;
    PRINT N'Purchase items copied.';

    /* =======================================================================
       6. SUPPLIER PAYMENTS
       New columns FinancialAccountId / PaymentLedgerCode / TransactionNo are
       left NULL (old payments are all cash and were not GL-posted).
       ======================================================================= */
    SET IDENTITY_INSERT dbo.SupplierPayments ON;

    INSERT dbo.SupplierPayments
        (Id, PaymentDate, SupplierId, StoreId, PaymentAmount, PaymentMethod,
         ReferenceNo, BankId, ChequeNo, ChequeDate, Remarks, UserName,
         CreatedDate, EditedBy, EditedDate,
         FinancialAccountId, PaymentLedgerCode, TransactionNo)
    SELECT
         Id, PaymentDate, SupplierId, StoreId, PaymentAmount, PaymentMethod,
         ReferenceNo, mbank.new_id, ChequeNo, ChequeDate, Remarks, UserName,
         CreatedDate, EditedBy, EditedDate,
         NULL, NULL, NULL
    FROM imp.SupplierPayments sp
    LEFT JOIN #mapBank mbank ON mbank.old_id = sp.BankId;

    SET IDENTITY_INSERT dbo.SupplierPayments OFF;
    PRINT N'Supplier payments copied.';

    /* =======================================================================
       7. BATCH NUMBER SEQUENCE (keeps future ReturnRefNo values unique)
       ======================================================================= */
    SET IDENTITY_INSERT dbo.BatchNoSeq ON;

    INSERT dbo.BatchNoSeq (Id, GroupId, GroupCode, SubgroupId, SubgroupCode,
                           SizeId, SizeName, ItemId, SequenceNo, CreatedDate)
    SELECT b.Id,
           COALESCE(mg.new_id, @fallbackGroup),
           b.GroupCode,
           COALESCE(ms.new_id, @fallbackSubGroup),
           b.SubgroupCode,
           mz.new_id,
           b.SizeName,
           b.ItemId,
           b.SequenceNo,
           b.CreatedDate
    FROM imp.BatchNoSeq b
    LEFT JOIN #mapGroup    mg ON mg.old_id = b.GroupId
    LEFT JOIN #mapSubGroup ms ON ms.old_id = b.SubgroupId
    LEFT JOIN #mapSize     mz ON mz.old_id = b.SizeId;

    SET IDENTITY_INSERT dbo.BatchNoSeq OFF;
    PRINT N'Batch numbers copied.';

    /* ---- keep the item serial sequence ahead of the imported history ----- */
    IF EXISTS (SELECT 1 FROM sys.sequences WHERE name = N'ProductSerialSequence')
    BEGIN
        DECLARE @oldSeq  bigint = (SELECT CONVERT(bigint, current_value) FROM imp.SourceSequences WHERE name = N'ProductSerialSequence');
        DECLARE @curSeq  bigint = (SELECT CONVERT(bigint, current_value) FROM sys.sequences WHERE name = N'ProductSerialSequence');

        IF @oldSeq IS NOT NULL AND @oldSeq > @curSeq
        BEGIN
            DECLARE @alterSeq nvarchar(200) =
                N'ALTER SEQUENCE dbo.ProductSerialSequence RESTART WITH ' + CAST(@oldSeq AS nvarchar(20));
            EXEC (@alterSeq);
        END;
    END;

    /* =======================================================================
       8. OPTIONAL: PURCHASE DRAFTS
       ======================================================================= */
    IF @IncludeDrafts = 1
    BEGIN
        SET IDENTITY_INSERT dbo.PurchaseDrafts ON;

        INSERT dbo.PurchaseDrafts
            (Id, DraftName, SupplierId, StoreId, PurchaseDate, BillInvoiceNumber,
             BillInvoiceName, IsVatIncluded, TotalAmount, DiscountAmount, VatAmount,
             NetAmount, PaidAmount, DueAmount, Remarks, DraftData, CreatedBy,
             CreatedDate, LastModifiedDate, LastModifiedBy, IsActive, BillDate)
        SELECT Id, DraftName, SupplierId, StoreId, PurchaseDate, BillInvoiceNumber,
               BillInvoiceName, IsVatIncluded, TotalAmount, DiscountAmount, VatAmount,
               NetAmount, PaidAmount, DueAmount, Remarks, DraftData, CreatedBy,
               CreatedDate, LastModifiedDate, LastModifiedBy, IsActive, BillDate
        FROM imp.PurchaseDrafts;

        SET IDENTITY_INSERT dbo.PurchaseDrafts OFF;

        SET IDENTITY_INSERT dbo.PurchaseDraftItems ON;

        INSERT dbo.PurchaseDraftItems
            (Id, DraftId, ItemId, ItemName, GroupId, GroupName, SubGroupId,
             SubGroupName, ShadeNo, ColorId, ColorName, SizeId, SizeName, Barcode,
             Quantity, PurchasePrice, ProductPricePercentage, OtherCost,
             CarryingCost, OperationalCost, VatPercentage, VatAmount, TotalAmount,
             IsSaleable, SalePrice, ProductType, MaterialType, Origin, Features,
             BrandColor, MesurementUnitId, CountStockByColor, CountStockBySize,
             IsNewItem, IsActive, TransportCost, IsConsume, BrandId, BrandName,
             OriginId, OriginName, FeatureIds, FeaturesDisplay, DesignId,
             DesignName, Catalogue)
        SELECT
             di.Id, di.DraftId,
             CASE WHEN it.Id IS NULL THEN NULL ELSE di.ItemId END,
             di.ItemName,
             mg.new_id,
             di.GroupName,
             COALESCE(ms.new_id, @fallbackSubGroup),
             di.SubGroupName,
             di.ShadeNo,
             mc.new_id,
             di.ColorName,
             mz.new_id,
             di.SizeName,
             di.Barcode,
             di.Quantity, di.PurchasePrice, di.ProductPricePercentage,
             di.OtherCost, di.CarryingCost, di.OperationalCost,
             di.VatPercentage, di.VatAmount, di.TotalAmount,
             di.IsSaleable, di.SalePrice, di.ProductType, di.MaterialType,
             di.Origin, di.Features, di.BrandColor,
             COALESCE(mu.new_id, @fallbackUnit),
             di.CountStockByColor, di.CountStockBySize, di.IsNewItem, di.IsActive,
             di.TransportCost, di.IsConsume,
             mb.new_id, di.BrandName, mo.new_id, di.OriginName,
             di.FeatureIds, di.FeaturesDisplay, md.new_id, di.DesignName,
             di.Catalogue
        FROM imp.PurchaseDraftItems di
        LEFT JOIN dbo.Items    it ON it.Id     = di.ItemId
        LEFT JOIN #mapGroup    mg ON mg.old_id = di.GroupId
        LEFT JOIN #mapSubGroup ms ON ms.old_id = di.SubGroupId
        LEFT JOIN #mapColor    mc ON mc.old_id = di.ColorId
        LEFT JOIN #mapSize     mz ON mz.old_id = di.SizeId
        LEFT JOIN #mapUnit     mu ON mu.old_id = di.MesurementUnitId
        LEFT JOIN #mapBrand    mb ON mb.old_id = di.BrandId
        LEFT JOIN #mapOrigin   mo ON mo.old_id = di.OriginId
        LEFT JOIN #mapDesign   md ON md.old_id = di.DesignId;

        SET IDENTITY_INSERT dbo.PurchaseDraftItems OFF;
        PRINT N'Purchase drafts copied.';
    END;

    /* =======================================================================
       9. OPTIONAL: STOCK (current-stock snapshot + purchase movements)
       StockMaster is copied as-is (it already reflects old sales), and only
       purchase / opening-stock StockDetail rows are copied.
       ======================================================================= */
    IF @IncludeStock = 1
    BEGIN
        SET IDENTITY_INSERT dbo.StockMaster ON;

        INSERT dbo.StockMaster
            (StockID, StockDate, StoreId, ItemID, ItemTypeID, UOMID, ItemCode,
             SerialNo, ReceiveQty, ReceiveValue, LastReceiveDate, IssueQty,
             IssueValue, LastIssueDate, CurrentRate, SalesRate, SalesValue,
             LastSalesDate, CompanyID, CreatedBy, CreatedAt, CreatedRemarks,
             UpdatedBy, UpdatedAt, UpdateRemark, IsDeleted, DeletedBy, DeletedAt,
             DeleteRemarks)
        SELECT
             s.StockID, s.StockDate, s.StoreId, s.ItemID, s.ItemTypeID,
             COALESCE(mu.new_id, @fallbackUnit), s.ItemCode, s.SerialNo,
             s.ReceiveQty, s.ReceiveValue, s.LastReceiveDate, s.IssueQty,
             s.IssueValue, s.LastIssueDate, s.CurrentRate, s.SalesRate,
             s.SalesValue, s.LastSalesDate, s.CompanyID, s.CreatedBy,
             s.CreatedAt, s.CreatedRemarks, s.UpdatedBy, s.UpdatedAt,
             s.UpdateRemark, s.IsDeleted, s.DeletedBy, s.DeletedAt, s.DeleteRemarks
        FROM imp.StockMaster s
        LEFT JOIN #mapUnit mu ON mu.old_id = s.UOMID;

        SET IDENTITY_INSERT dbo.StockMaster OFF;

        SET IDENTITY_INSERT dbo.StockDetail ON;

        INSERT dbo.StockDetail
            (StockDetailID, StockID, StoreId, TransactionNo, Narration,
             TransactionID, TransactionTypeID, StockDate, ItemID, ItemTypeID,
             ItemCode, SerialNo, ReceiveQty, ReceiveValue, ReceiveRate, IssueQty,
             UOMID, IssueRate, IssueValue, CompanyID, CreatedBy, CreatedAt,
             CreatedRemarks, UpdatedBy, UpdatedAt, UpdateRemark, IsDeleted,
             DeletedBy, DeletedAt, DeleteRemarks, TranDtlId, ReturnRefNo)
        SELECT
             d.StockDetailID, d.StockID, d.StoreId, d.TransactionNo, d.Narration,
             d.TransactionID, d.TransactionTypeID, d.StockDate, d.ItemID,
             d.ItemTypeID, d.ItemCode, d.SerialNo, d.ReceiveQty, d.ReceiveValue,
             d.ReceiveRate, d.IssueQty,
             COALESCE(mu.new_id, @fallbackUnit),
             d.IssueRate, d.IssueValue, d.CompanyID, d.CreatedBy, d.CreatedAt,
             d.CreatedRemarks, d.UpdatedBy, d.UpdatedAt, d.UpdateRemark,
             d.IsDeleted, d.DeletedBy, d.DeletedAt, d.DeleteRemarks, d.TranDtlId,
             d.ReturnRefNo
        FROM imp.StockDetail d
        LEFT JOIN #mapUnit mu ON mu.old_id = d.UOMID
        WHERE d.TransactionTypeID IN (1, 3);   -- 1 = purchase, 3 = opening stock

        SET IDENTITY_INSERT dbo.StockDetail OFF;
        PRINT N'Stock snapshot + purchase movements copied.';
    END;

    /* =======================================================================
       10. OPTIONAL: ACCOUNTING (posted purchase journals + reversals)
       Journals are copied with their original entry/line ids, account ids
       mapped by account code, party ids mapped through the imported suppliers.
       Guard triggers are suspended during the copy and validated afterwards.
       ======================================================================= */
    IF @IncludeAccounting = 1
    BEGIN
        CREATE TABLE #mapAccount (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);
        CREATE TABLE #mapParty   (old_id int NOT NULL PRIMARY KEY, new_id int NOT NULL);

        /* accounts by company + code */
        INSERT #mapAccount (old_id, new_id)
        SELECT o.account_id, n.account_id
        FROM imp.acc_account o
        JOIN dbo.acc_account n ON n.company_id = o.company_id AND n.code = o.code;

        /* parties through suppliers (covers every PUR line), then by type+code */
        INSERT #mapParty (old_id, new_id)
        SELECT op.party_id, ns.AccPartyId
        FROM imp.acc_party op
        JOIN imp.Suppliers os ON os.AccPartyId = op.party_id
        JOIN dbo.Suppliers ns              ON ns.Id = os.Id
        WHERE ns.AccPartyId IS NOT NULL;

        INSERT #mapParty (old_id, new_id)
        SELECT op.party_id, np.party_id
        FROM imp.acc_party op
        JOIN dbo.acc_party np
          ON np.company_id = op.company_id
         AND np.party_type = op.party_type
         AND np.code = op.code
        WHERE NOT EXISTS (SELECT 1 FROM #mapParty m WHERE m.old_id = op.party_id);

        /* all used periods must be OPEN in the target */
        IF EXISTS (
            SELECT 1
            FROM imp.acc_journal_entry e
            LEFT JOIN dbo.acc_fiscal_period p
                   ON p.period_id = e.period_id
                  AND p.company_id = e.company_id
                  AND p.fiscal_year_id = e.fiscal_year_id
            WHERE p.period_id IS NULL OR p.status <> 'OPEN')
        BEGIN
            SET @msg = N'@IncludeAccounting = 1 but one or more journal periods are missing/closed in the target.';
            THROW 50020, @msg, 1;
        END;

        /* suspend the journal guard triggers (audit trigger stays on) */
        ALTER TABLE dbo.acc_journal_entry DISABLE TRIGGER acc_trg_journal_entry_guard;
        ALTER TABLE dbo.acc_journal_line  DISABLE TRIGGER acc_trg_journal_line_guard;

        SET IDENTITY_INSERT dbo.acc_journal_entry ON;

        INSERT dbo.acc_journal_entry
            (entry_id, company_id, voucher_type_id, branch_id, fiscal_year_id,
             period_id, voucher_no, entry_date, narration, reference_no,
             source_system, source_ref, status, reversal_of, created_by,
             created_at, posted_by, posted_at)
        SELECT entry_id, company_id, voucher_type_id, branch_id, fiscal_year_id,
               period_id, voucher_no, entry_date, narration, reference_no,
               source_system, source_ref, status, reversal_of, created_by,
               created_at, posted_by, posted_at
        FROM imp.acc_journal_entry;

        SET IDENTITY_INSERT dbo.acc_journal_entry OFF;

        SET IDENTITY_INSERT dbo.acc_journal_line ON;

        INSERT dbo.acc_journal_line
            (line_id, entry_id, line_no, account_id, branch_id, party_id,
             debit, credit, memo)
        SELECT l.line_id, l.entry_id, l.line_no,
               ma.new_id,
               l.branch_id,
               mp.new_id,
               l.debit, l.credit, l.memo
        FROM imp.acc_journal_line l
        JOIN #mapAccount ma ON ma.old_id = l.account_id
        LEFT JOIN #mapParty mp ON mp.old_id = l.party_id;

        SET IDENTITY_INSERT dbo.acc_journal_line OFF;

        ALTER TABLE dbo.acc_journal_entry ENABLE TRIGGER acc_trg_journal_entry_guard;
        ALTER TABLE dbo.acc_journal_line  ENABLE TRIGGER acc_trg_journal_line_guard;

        /* voucher numbering must continue after the imported numbers */
        MERGE dbo.acc_voucher_sequence AS t
        USING (
            SELECT company_id, voucher_type_id, fiscal_year_id, branch_id,
                   MAX(voucher_no) AS last_no
            FROM dbo.acc_journal_entry
            GROUP BY company_id, voucher_type_id, fiscal_year_id, branch_id
        ) s
           ON  t.company_id      = s.company_id
          AND  t.voucher_type_id = s.voucher_type_id
          AND  t.fiscal_year_id  = s.fiscal_year_id
          AND  t.branch_id       = s.branch_id
        WHEN MATCHED THEN UPDATE SET last_no = s.last_no
        WHEN NOT MATCHED THEN
            INSERT (company_id, voucher_type_id, fiscal_year_id, branch_id, last_no)
            VALUES (s.company_id, s.voucher_type_id, s.fiscal_year_id, s.branch_id, s.last_no);

        /* our own balance validation (guards were bypassed) */
        IF EXISTS (
            SELECT 1 FROM dbo.acc_journal_entry e
            WHERE (SELECT ISNULL(SUM(debit), 0) FROM dbo.acc_journal_line l WHERE l.entry_id = e.entry_id)
                <> (SELECT ISNULL(SUM(credit), 0) FROM dbo.acc_journal_line l WHERE l.entry_id = e.entry_id))
        BEGIN
            SET @msg = N'Imported journals are not balanced. Rolling back.';
            THROW 50021, @msg, 1;
        END;

        PRINT N'Accounting journals copied.';
    END;

    /* =======================================================================
       11. RESEED IDENTITY COLUMNS on every table we touched
       ======================================================================= */
    DECLARE @reseed TABLE (tbl sysname NOT NULL, idcol sysname NOT NULL);
    INSERT @reseed (tbl, idcol) VALUES
        ('Stores','Id'), ('Colors','Id'), ('Sizes','Id'), ('MesurementUnits','Id'),
        ('ItemBrand','BrandId'), ('ItemOrigin','OriginId'), ('ItemCatalogue','CatalogueId'),
        ('ItemFeatures','FeatureId'), ('Groups','Id'), ('SubGroups','Id'), ('Designs','Id'),
        ('Items','Id'), ('Suppliers','Id'), ('Purchases','Id'), ('PurchaseItems','Id'),
        ('SupplierPayments','Id'), ('BatchNoSeq','Id');

    IF @IncludeDrafts = 1
    BEGIN
        INSERT @reseed (tbl, idcol) VALUES ('PurchaseDrafts','Id'), ('PurchaseDraftItems','Id');
    END;

    IF @IncludeStock = 1
    BEGIN
        INSERT @reseed (tbl, idcol) VALUES ('StockMaster','StockID'), ('StockDetail','StockDetailID');
    END;

    IF @IncludeAccounting = 1
    BEGIN
        INSERT @reseed (tbl, idcol) VALUES ('acc_journal_entry','entry_id'), ('acc_journal_line','line_id');
    END;

    DECLARE @reseedTbl sysname, @idcol sysname, @maxId bigint;
    DECLARE reseed_cur CURSOR LOCAL FAST_FORWARD FOR SELECT tbl, idcol FROM @reseed;
    OPEN reseed_cur;
    FETCH NEXT FROM reseed_cur INTO @reseedTbl, @idcol;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'SELECT @m = ISNULL(MAX(' + QUOTENAME(@idcol) + N'), 0) FROM dbo.' + QUOTENAME(@reseedTbl) + N';';
        EXEC sp_executesql @sql, N'@m bigint OUTPUT', @m = @maxId OUTPUT;

        SET @sql = N'DBCC CHECKIDENT (''dbo.' + REPLACE(@reseedTbl, '''', '''''') + N''', RESEED, '
                 + CAST(@maxId AS nvarchar(20)) + N') WITH NO_INFOMSGS;';
        EXEC (@sql);

        FETCH NEXT FROM reseed_cur INTO @reseedTbl, @idcol;
    END;
    CLOSE reseed_cur;
    DEALLOCATE reseed_cur;

    /* =======================================================================
       12. VALIDATION
       ======================================================================= */
    IF (SELECT COUNT(*) FROM dbo.Purchases)     <> (SELECT COUNT(*) FROM imp.Purchases)
       OR (SELECT COUNT(*) FROM dbo.PurchaseItems) <> (SELECT COUNT(*) FROM imp.PurchaseItems)
       OR (SELECT COUNT(*) FROM dbo.Suppliers)     <> (SELECT COUNT(*) FROM imp.Suppliers)
       OR (SELECT COUNT(*) FROM dbo.Items)         <> (SELECT COUNT(*) FROM imp.Items)
    BEGIN
        SET @msg = N'Row-count reconciliation failed. Rolling back.';
        THROW 50030, @msg, 1;
    END;

    IF (SELECT ISNULL(SUM(NetTotal), 0) FROM dbo.Purchases)
       <> (SELECT ISNULL(SUM(NetTotal), 0) FROM imp.Purchases)
    BEGIN
        SET @msg = N'Purchase total reconciliation failed. Rolling back.';
        THROW 50031, @msg, 1;
    END;

    /* orphan checks - should be 0 */
    IF EXISTS (SELECT 1 FROM dbo.PurchaseItems pi
               WHERE NOT EXISTS (SELECT 1 FROM dbo.Purchases p WHERE p.Id = pi.PurchaseId))
    BEGIN
        SET @msg = N'Imported purchase lines have no parent purchase. Rolling back.';
        THROW 50032, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM dbo.PurchaseItems pi
               WHERE NOT EXISTS (SELECT 1 FROM dbo.Items i WHERE i.Id = pi.ItemId))
    BEGIN
        SET @msg = N'Imported purchase lines reference missing items. Rolling back.';
        THROW 50033, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Purchases p
               WHERE NOT EXISTS (SELECT 1 FROM dbo.Suppliers s WHERE s.Id = p.SupplierId))
    BEGIN
        SET @msg = N'Imported purchases reference missing suppliers. Rolling back.';
        THROW 50034, @msg, 1;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Purchases p
               WHERE NOT EXISTS (SELECT 1 FROM dbo.Stores s WHERE s.Id = p.StoreId))
    BEGIN
        SET @msg = N'Imported purchases reference missing stores. Rolling back.';
        THROW 50035, @msg, 1;
    END;

    COMMIT TRANSACTION;

    PRINT N'';
    PRINT N'============================================================';
    PRINT N' IMPORT COMPLETED SUCCESSFULLY';
    PRINT N'============================================================';

    /* reconciliation report */
    SELECT N'Purchases'      AS Entity, (SELECT COUNT(*) FROM imp.Purchases)     AS OldRows, (SELECT COUNT(*) FROM dbo.Purchases)      AS NewRows
    UNION ALL SELECT N'PurchaseItems',   (SELECT COUNT(*) FROM imp.PurchaseItems), (SELECT COUNT(*) FROM dbo.PurchaseItems)
    UNION ALL SELECT N'Suppliers',       (SELECT COUNT(*) FROM imp.Suppliers),     (SELECT COUNT(*) FROM dbo.Suppliers)
    UNION ALL SELECT N'Items',           (SELECT COUNT(*) FROM imp.Items),         (SELECT COUNT(*) FROM dbo.Items)
    UNION ALL SELECT N'SupplierPayments',(SELECT COUNT(*) FROM imp.SupplierPayments),(SELECT COUNT(*) FROM dbo.SupplierPayments)
    UNION ALL SELECT N'BatchNoSeq',      (SELECT COUNT(*) FROM imp.BatchNoSeq),    (SELECT COUNT(*) FROM dbo.BatchNoSeq);

    SELECT
        (SELECT ISNULL(SUM(NetTotal), 0) FROM imp.Purchases) AS OldPurchaseTotal,
        (SELECT ISNULL(SUM(NetTotal), 0) FROM dbo.Purchases)             AS NewPurchaseTotal,
        (SELECT ISNULL(SUM(PaymentAmount), 0) FROM imp.SupplierPayments) AS OldPaymentTotal,
        (SELECT ISNULL(SUM(PaymentAmount), 0) FROM dbo.SupplierPayments)             AS NewPaymentTotal;

    /* next purchase number the application will allocate */
    SELECT TOP 1
        SystemInvoiceNo AS LastImportedInvoiceNo,
        RIGHT(N'0000000' + CAST(CAST(SystemInvoiceNo AS int) + 1 AS nvarchar(20)), 7) AS NextSystemInvoiceNo
    FROM dbo.Purchases
    WHERE SystemInvoiceNo NOT LIKE 'OS-%' AND TRY_CAST(SystemInvoiceNo AS int) IS NOT NULL
    ORDER BY TRY_CAST(SystemInvoiceNo AS int) DESC;

    /* items that landed in the fallback group (informational) */
    SELECT COUNT(*) AS ItemsInFallbackGroup
    FROM dbo.Items i
    JOIN dbo.Groups g ON g.Id = i.GroupId
    WHERE g.Name = @FallbackGroupName;

    /* the local staging copy is no longer needed */
    SET @sql = N'';
    SELECT @sql = @sql + N'DROP TABLE imp.' + QUOTENAME(name) + N';'
    FROM sys.tables WHERE schema_id = SCHEMA_ID(N'imp');
    IF @sql <> N'' EXEC (@sql);
    EXEC (N'DROP SCHEMA imp;');
    PRINT N'Staging tables removed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    /* make sure the accounting guards are never left disabled */
    BEGIN TRY
        ALTER TABLE dbo.acc_journal_entry ENABLE TRIGGER acc_trg_journal_entry_guard;
        ALTER TABLE dbo.acc_journal_line  ENABLE TRIGGER acc_trg_journal_line_guard;
    END TRY
    BEGIN CATCH
        /* ignore - table/trigger may not exist in this target */
    END CATCH;

    /* best-effort removal of the local staging schema on failure */
    BEGIN TRY
        SET @sql = N'';
        SELECT @sql = @sql + N'DROP TABLE imp.' + QUOTENAME(name) + N';'
        FROM sys.tables WHERE schema_id = SCHEMA_ID(N'imp');
        IF @sql <> N'' EXEC (@sql);
        EXEC (N'DROP SCHEMA imp;');
    END TRY
    BEGIN CATCH
        /* ignore */
    END CATCH;

    PRINT N'';
    PRINT N'================ IMPORT FAILED - ROLLED BACK ================';
    PRINT N'Error ' + CAST(ERROR_NUMBER() AS nvarchar(20)) + N': ' + ERROR_MESSAGE();
    PRINT N'Line ' + CAST(ERROR_LINE() AS nvarchar(20));
    PRINT N'============================================================';
    THROW;
END CATCH;





