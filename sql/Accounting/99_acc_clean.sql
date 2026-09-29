/* =====================================================================
   Accounting DB clean script — Microsoft SQL Server (2017+)
   ---------------------------------------------------------------------
   MODES
     TXN  : delete transactions only (journal, voucher sequences, audit*)
            keep company / branches / chart / parties / calendar
     ALL  : delete ALL data in acc_* tables, reseed identities, keep schema
     DROP : drop every acc_* view, procedure, trigger and table
   SAFETY
     - @dry_run = 1 (default) only prints the plan; nothing changes
     - to execute: @dry_run = 0 AND @confirm_db = exact current DB name
     - runs in ONE transaction; any error rolls everything back
       (including disabled triggers)
   *  TXN keeps the audit log when @keep_audit = 1
   DESTRUCTIVE — never run against production without a verified backup.
   ===================================================================== */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'tempdb..#acc_row_counts') IS NOT NULL DROP PROCEDURE #acc_row_counts;   -- allow re-run in same session
GO

/* helper: exact row counts for every dbo.acc_* table */
CREATE PROCEDURE #acc_row_counts @title nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @sql nvarchar(max);
    SELECT @sql = STRING_AGG(CAST(CONCAT(N'SELECT N''', t.name, N''' AS table_name, COUNT_BIG(*) AS row_count FROM dbo.',
                                         QUOTENAME(t.name)) AS nvarchar(max)), N' UNION ALL ')
                  WITHIN GROUP (ORDER BY t.name)
    FROM sys.tables t
    WHERE t.schema_id = SCHEMA_ID(N'dbo') AND t.name LIKE N'acc[_]%';

    PRINT CONCAT(N'--- ', @title, N' ---');
    IF @sql IS NULL PRINT N'(no acc_* tables)';
    ELSE EXEC sys.sp_executesql @sql;
END;
GO

/* =========================== PARAMETERS =========================== */
DECLARE @mode           varchar(10) = 'TXN';   -- TXN | ALL | DROP
DECLARE @dry_run        bit         = 1;       -- 1 = print plan only
DECLARE @confirm_db     sysname     = N'';     -- must equal DB_NAME() to execute
DECLARE @keep_audit     bit         = 0;       -- TXN only: 1 = keep acc_audit_log
DECLARE @reopen_periods bit         = 0;       -- TXN only: 1 = set every period back to OPEN
/* ================================================================== */

DECLARE @m nvarchar(400);

IF @mode NOT IN ('TXN','ALL','DROP')
BEGIN SET @m = CONCAT(N'Invalid @mode ''', @mode, N'''. Use TXN, ALL or DROP.'); THROW 53001, @m, 1; END;

IF @dry_run = 0 AND (@confirm_db IS NULL OR @confirm_db <> DB_NAME())
BEGIN SET @m = CONCAT(N'Refusing to run: set @confirm_db = N''', DB_NAME(), N''' to confirm.'); THROW 53002, @m, 1; END;

IF @mode IN ('TXN','ALL') AND (OBJECT_ID(N'dbo.acc_journal_entry', N'U') IS NULL OR OBJECT_ID(N'dbo.acc_account', N'U') IS NULL)
    THROW 53003, N'acc_* schema not found in this database; nothing to clean.', 1;

DECLARE @plan TABLE (seq int IDENTITY(1,1) PRIMARY KEY, stmt nvarchar(max) NOT NULL);
DECLARE @reseed TABLE (table_name sysname PRIMARY KEY);

/* ---------------- TXN (also first half of ALL) ---------------- */
IF @mode IN ('TXN','ALL')
BEGIN
    -- guard + audit triggers would block / flood these deletes
    INSERT @plan (stmt) VALUES
      (N'ALTER TABLE dbo.acc_journal_line  DISABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_journal_entry DISABLE TRIGGER ALL;'),
      (N'UPDATE dbo.acc_journal_entry SET reversal_of = NULL WHERE reversal_of IS NOT NULL;'),  -- break self-FK
      (N'DELETE FROM dbo.acc_journal_line;'),
      (N'DELETE FROM dbo.acc_journal_entry;'),
      (N'DELETE FROM dbo.acc_voucher_sequence;');

    INSERT @reseed (table_name) VALUES (N'acc_journal_line'), (N'acc_journal_entry');

    IF @mode = 'ALL' OR @keep_audit = 0
    BEGIN
        INSERT @plan (stmt) VALUES (N'DELETE FROM dbo.acc_audit_log;');
        INSERT @reseed (table_name) VALUES (N'acc_audit_log');
    END;

    IF @mode = 'TXN' AND @reopen_periods = 1
        INSERT @plan (stmt) VALUES
          (N'ALTER TABLE dbo.acc_fiscal_period DISABLE TRIGGER ALL;'),        -- allows LOCKED -> OPEN
          (N'UPDATE dbo.acc_fiscal_period SET status = ''OPEN'' WHERE status <> ''OPEN'';'),
          (N'ALTER TABLE dbo.acc_fiscal_period ENABLE TRIGGER ALL;');
END;

/* ---------------- ALL: master data ---------------- */
IF @mode = 'ALL'
BEGIN
    INSERT @plan (stmt) VALUES
      (N'ALTER TABLE dbo.acc_account       DISABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_party         DISABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_fiscal_period DISABLE TRIGGER ALL;'),
      (N'DELETE FROM dbo.acc_legacy_ledger_map;'),
      (N'DELETE FROM dbo.acc_account_party_type;'),
      (N'UPDATE dbo.acc_account SET parent_id = NULL WHERE parent_id IS NOT NULL;'),              -- break self-FK
      (N'DELETE FROM dbo.acc_account;'),
      (N'DELETE FROM dbo.acc_party;'),
      (N'DELETE FROM dbo.acc_fiscal_period;'),
      (N'DELETE FROM dbo.acc_fiscal_year;'),
      (N'DELETE FROM dbo.acc_voucher_type;'),
      (N'DELETE FROM dbo.acc_branch;'),
      (N'DELETE FROM dbo.acc_company;'),
      (N'ALTER TABLE dbo.acc_account       ENABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_party         ENABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_fiscal_period ENABLE TRIGGER ALL;');

    INSERT @reseed (table_name) VALUES
      (N'acc_account'), (N'acc_party'), (N'acc_fiscal_period'), (N'acc_fiscal_year'),
      (N'acc_voucher_type'), (N'acc_branch'), (N'acc_company');
END;

IF @mode IN ('TXN','ALL')
BEGIN
    INSERT @plan (stmt) VALUES
      (N'ALTER TABLE dbo.acc_journal_line  ENABLE TRIGGER ALL;'),
      (N'ALTER TABLE dbo.acc_journal_entry ENABLE TRIGGER ALL;');

    -- Reseed only tables that ever had rows (last_value NOT NULL); a never-used
    -- table reseeded to 0 would hand out id 0 instead of 1.
    INSERT @plan (stmt)
    SELECT CONCAT(N'DBCC CHECKIDENT (''dbo.', t.name, N''', RESEED, 0) WITH NO_INFOMSGS;')
    FROM @reseed r
    JOIN sys.tables t            ON t.name = r.table_name AND t.schema_id = SCHEMA_ID(N'dbo')
    JOIN sys.identity_columns ic ON ic.object_id = t.object_id
    WHERE ic.last_value IS NOT NULL;
END;

/* ---------------- DROP: all objects ---------------- */
IF @mode = 'DROP'
BEGIN
    INSERT @plan (stmt) VALUES
      -- views (dependents first)
      (N'DROP VIEW IF EXISTS dbo.acc_vw_balance_rollup;'),
      (N'DROP VIEW IF EXISTS dbo.acc_vw_party_balance;'),
      (N'DROP VIEW IF EXISTS dbo.acc_vw_trial_balance_period;'),
      (N'DROP VIEW IF EXISTS dbo.acc_vw_gl;'),
      (N'DROP VIEW IF EXISTS dbo.acc_vw_account_ancestor;'),
      (N'DROP VIEW IF EXISTS dbo.acc_vw_account_tree;'),
      -- procedures
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_trial_balance;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_period_set_status;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_entry_reverse;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_entry_post;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_line_add;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_entry_create;'),
      (N'DROP PROCEDURE IF EXISTS dbo.acc_usp_voucher_next;'),
      -- tables, FK children first (triggers and indexes drop with their table)
      (N'DROP TABLE IF EXISTS dbo.acc_legacy_ledger_map;'),
      (N'DROP TABLE IF EXISTS dbo.acc_audit_log;'),
      (N'DROP TABLE IF EXISTS dbo.acc_journal_line;'),
      (N'DROP TABLE IF EXISTS dbo.acc_journal_entry;'),
      (N'DROP TABLE IF EXISTS dbo.acc_voucher_sequence;'),
      (N'DROP TABLE IF EXISTS dbo.acc_voucher_type;'),
      (N'DROP TABLE IF EXISTS dbo.acc_fiscal_period;'),
      (N'DROP TABLE IF EXISTS dbo.acc_fiscal_year;'),
      (N'DROP TABLE IF EXISTS dbo.acc_account_party_type;'),
      (N'DROP TABLE IF EXISTS dbo.acc_account;'),
      (N'DROP TABLE IF EXISTS dbo.acc_party;'),
      (N'DROP TABLE IF EXISTS dbo.acc_branch;'),
      (N'DROP TABLE IF EXISTS dbo.acc_company;');
END;

/* ---------------- Report / execute ---------------- */
PRINT CONCAT(N'Database: ', DB_NAME(), N' | mode: ', @mode, N' | dry_run: ', @dry_run,
             CASE WHEN @mode = 'TXN' THEN CONCAT(N' | keep_audit: ', @keep_audit, N' | reopen_periods: ', @reopen_periods) ELSE N'' END);
EXEC #acc_row_counts N'Row counts BEFORE';

IF @dry_run = 1
BEGIN
    PRINT N'--- DRY RUN: planned statements (nothing executed) ---';
    SELECT seq, stmt FROM @plan ORDER BY seq;
    PRINT CONCAT(N'To execute: SET @dry_run = 0 and @confirm_db = N''', DB_NAME(), N'''.');
    RETURN;
END;

DECLARE @i int = 1, @n int = (SELECT MAX(seq) FROM @plan), @s nvarchar(max);

BEGIN TRY
    BEGIN TRAN;
    WHILE @i <= @n
    BEGIN
        SELECT @s = stmt FROM @plan WHERE seq = @i;
        PRINT CONCAT(N'[', @i, N'/', @n, N'] ', @s);
        EXEC sys.sp_executesql @s;
        SET @i += 1;
    END;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT CONCAT(N'FAILED at step ', @i, N' — everything rolled back. ', ERROR_MESSAGE());
    THROW;
END CATCH;

IF @mode = 'DROP'
BEGIN
    IF EXISTS (SELECT 1 FROM sys.objects
               WHERE name LIKE N'acc[_]%' AND is_ms_shipped = 0 AND parent_object_id = 0
                 AND schema_id = SCHEMA_ID(N'dbo'))
    BEGIN
        PRINT N'WARNING: acc_* objects not in the drop list still exist:';
        SELECT name, type_desc FROM sys.objects
        WHERE name LIKE N'acc[_]%' AND is_ms_shipped = 0 AND parent_object_id = 0
          AND schema_id = SCHEMA_ID(N'dbo');
    END
    ELSE PRINT N'All acc_* objects dropped.';
END
ELSE
    EXEC #acc_row_counts N'Row counts AFTER';

PRINT N'Clean complete.';
GO
