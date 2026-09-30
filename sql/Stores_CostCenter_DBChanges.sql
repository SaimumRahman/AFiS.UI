/* ============================================================
   Stores <-> ACC_Branch (Cost Center) relation
   Author : opencode
   Date   : 2026-09-30
   Run on : AFIS_MAH_LIVE (idempotent, safe to re-run)
   ------------------------------------------------------------
   Adds CostCenterId to Stores + FK to acc_branch(branch_id),
   then back-fills existing stores from the legacy Store.Code
   -> acc_branch.code guesswork currently hardcoded in
   AccountingRepository.GetBranchCodeAsync (BTB->BT, WH->HO,
   112->HLS).

   NOTE: acc_branch.branch_id is SMALLINT, so Stores.CostCenterId
   must also be SMALLINT or the FK cannot be created. This script
   is self-healing: it repairs a column that was previously added
   with a mismatched type.
   ============================================================ */

SET NOCOUNT ON;
GO

/* ---- 1. Column: create as SMALLINT, or repair a wrong-typed one ---- */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Stores')
      AND name = N'CostCenterId'
)
BEGIN
    ALTER TABLE dbo.Stores ADD CostCenterId SMALLINT NULL;
    PRINT 'Column Stores.CostCenterId added (SMALLINT).';
END
ELSE IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Stores')
      AND name = N'CostCenterId'
      AND system_type_id <> TYPE_ID(N'smallint')
)
BEGIN
    /* Guard: refuse to narrow if any value would not fit in SMALLINT */
    IF EXISTS (SELECT 1 FROM dbo.Stores WHERE CostCenterId < -32768 OR CostCenterId > 32767)
    BEGIN
        RAISERROR('Stores.CostCenterId holds values outside SMALLINT range; fix data before re-running.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_Stores_AccBranch' AND parent_object_id = OBJECT_ID(N'dbo.Stores'))
    BEGIN
        ALTER TABLE dbo.Stores DROP CONSTRAINT FK_Stores_AccBranch;
        PRINT 'FK FK_Stores_AccBranch dropped for type fix.';
    END

    IF EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Stores_CostCenterId' AND object_id = OBJECT_ID(N'dbo.Stores'))
    BEGIN
        DROP INDEX IX_Stores_CostCenterId ON dbo.Stores;
        PRINT 'Index IX_Stores_CostCenterId dropped for type fix.';
    END

    ALTER TABLE dbo.Stores ALTER COLUMN CostCenterId SMALLINT NULL;
    PRINT 'Column Stores.CostCenterId re-typed to SMALLINT.';
END
ELSE
BEGIN
    PRINT 'Column Stores.CostCenterId already exists as SMALLINT.';
END
GO

/* ---- 2. Index to support the join ---- */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Stores_CostCenterId' AND object_id = OBJECT_ID(N'dbo.Stores')
)
BEGIN
    CREATE INDEX IX_Stores_CostCenterId ON dbo.Stores(CostCenterId);
    PRINT 'Index IX_Stores_CostCenterId created.';
END
GO

/* ---- 3. Foreign key (types now match: SMALLINT -> SMALLINT) ---- */
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Stores_AccBranch' AND parent_object_id = OBJECT_ID(N'dbo.Stores')
)
BEGIN
    /* Drop any pre-existing FK on this column created under a different name */
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.Stores') AND referenced_object_id = OBJECT_ID(N'dbo.acc_branch')
    )
    BEGIN
        DECLARE @staleFk sysname = (
            SELECT fk.name FROM sys.foreign_keys fk
            WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Stores')
              AND fk.referenced_object_id = OBJECT_ID(N'dbo.acc_branch')
        );
        EXEC('ALTER TABLE dbo.Stores DROP CONSTRAINT ' + @staleFk);
        PRINT 'Stale FK dropped: ' + @staleFk;
    END

    ALTER TABLE dbo.Stores
        ADD CONSTRAINT FK_Stores_AccBranch
        FOREIGN KEY (CostCenterId) REFERENCES dbo.acc_branch(branch_id);
    PRINT 'FK FK_Stores_AccBranch added.';
END
ELSE
BEGIN
    PRINT 'FK FK_Stores_AccBranch already exists.';
END
GO

/* ---- 4. Back-fill existing stores (best-effort from legacy mapping) ---- */
UPDATE s
SET s.CostCenterId = b.branch_id
FROM dbo.Stores s
JOIN dbo.acc_branch b
  ON b.code = CASE
        WHEN UPPER(LTRIM(RTRIM(ISNULL(s.Code, N'')))) = 'BTB' THEN 'BT'
        WHEN UPPER(LTRIM(RTRIM(ISNULL(s.Code, N'')))) = 'WH'  THEN 'HO'
        WHEN UPPER(LTRIM(RTRIM(ISNULL(s.Code, N'')))) = '112' THEN 'HLS'
        ELSE LTRIM(RTRIM(ISNULL(s.Code, N'')))
      END
WHERE s.CostCenterId IS NULL;

/* ---- Verify ---- */
SELECT s.Id, s.Name, s.Code, s.CostCenterId, b.code AS BranchCode, b.name AS BranchName
FROM dbo.Stores s
LEFT JOIN dbo.acc_branch b ON b.branch_id = s.CostCenterId
ORDER BY s.Name;

SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable,
    CASE WHEN EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.parent_object_id = c.object_id AND fk.name = N'FK_Stores_AccBranch'
    ) THEN 'Yes' ELSE 'No' END AS HasFK
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.Stores') AND c.name = N'CostCenterId';