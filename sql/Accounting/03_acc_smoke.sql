/* ============================================================================
   03_acc_smoke.sql — Smoke + negative tests for the accounting layer on the
   LIVE schema (AFIS_MAH_LIVE). Run standalone; NO dependent seed required.

   No stored procedures: every test mirrors the RAW SQL batches executed by
   AccountingRepository in the same order (create DRAFT -> add lines -> post ->
   reverse -> re-post with effective-ref suffix), so what is verified here is
   exactly what the API runs.

   SAFETY: every test runs inside its own transaction and is rolled back. This
   script NEVER commits - running it leaves zero footprint on the live DB.

   Requires QUOTED_IDENTIFIER ON (the acc_journal_entry table has a filtered
   index). sqlcmd: add -I. SSMS/ADO.NET enable it by default.

   Expected output: one [PASS]/[FAIL] per test, then "Smoke tests complete."
   ============================================================================ */
SET NOCOUNT ON;
SET XACT_ABORT OFF;

/* ---------------------------------------------------------------------------
   Helpers
   --------------------------------------------------------------------------- */

/* Runs @sql in a transaction; passes only if it throws @expected_error.    */
IF OBJECT_ID(N'tempdb..#acc_expect_error') IS NOT NULL DROP PROCEDURE #acc_expect_error;
GO
CREATE PROCEDURE #acc_expect_error
    @test_name      NVARCHAR(100),
    @expected_error INT,
    @sql            NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ok INT = 0, @actual INT = 0;
    BEGIN TRY
        BEGIN TRAN;
        EXEC (@sql);
        IF XACT_STATE() <> 0 ROLLBACK;
    END TRY
    BEGIN CATCH
        SET @actual = ERROR_NUMBER();
        IF XACT_STATE() <> 0 ROLLBACK;
        IF @actual = @expected_error SET @ok = 1;
    END CATCH

    IF @ok = 1
        SELECT N'[PASS] ' + @test_name AS result;
    ELSE
        SELECT N'[FAIL] ' + @test_name + N' (expected ' + CAST(@expected_error AS NVARCHAR(10)) + N', got ' + CAST(@actual AS NVARCHAR(10)) + N')' AS result;
END
GO

/* Runs @sql in a transaction; passes only if it completes without error.   */
IF OBJECT_ID(N'tempdb..#acc_expect_ok') IS NOT NULL DROP PROCEDURE #acc_expect_ok;
GO
CREATE PROCEDURE #acc_expect_ok
    @test_name NVARCHAR(100),
    @sql       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @err INT = 0, @msg NVARCHAR(MAX) = N'';
    BEGIN TRY
        BEGIN TRAN;
        EXEC (@sql);
        IF XACT_STATE() <> 0 ROLLBACK;
    END TRY
    BEGIN CATCH
        SET @err = ERROR_NUMBER();
        SET @msg = ERROR_MESSAGE();
        IF XACT_STATE() <> 0 ROLLBACK;
    END CATCH

    IF @err = 0
        SELECT N'[PASS] ' + @test_name AS result;
    ELSE
        SELECT N'[FAIL] ' + @test_name + N' threw ' + CAST(@err AS NVARCHAR(10)) + N': ' + @msg AS result;
END
GO

/* ---------------------------------------------------------------------------
   create_batch — mirrors AccountingRepository.PostVoucherAsync step 1:
   resolves company / voucher type / branch / OPEN period, applies the
   effective-ref suffix when the source ref is already taken (UX_acc_je_source
   is unique), inserts a DRAFT header. Entry id lands in @e.
   Tokens: [BR] [VT] [SS] [SR] [DATE] [NAR].
   --------------------------------------------------------------------------- */
DECLARE @create NVARCHAR(MAX) = N'
    DECLARE @e BIGINT;
    DECLARE @company_id SMALLINT, @vt_id SMALLINT, @br_id SMALLINT,
            @fy_id SMALLINT, @pid INT, @pst VARCHAR(10), @use_ref NVARCHAR(200), @n INT;

    SELECT @company_id = company_id FROM dbo.acc_company WHERE code = N''AFHO'';
    IF @company_id IS NULL THROW 51030, N''Company not found'', 1;

    SELECT @vt_id = voucher_type_id FROM dbo.acc_voucher_type WHERE code = N''[VT]'';
    IF @vt_id IS NULL THROW 51031, N''Unknown voucher type'', 1;

    SELECT @br_id = branch_id FROM dbo.acc_branch
    WHERE company_id = @company_id AND code = N''[BR]'' AND is_active = 1;
    IF @br_id IS NULL THROW 51032, N''Unknown or inactive branch'', 1;

    SELECT @pid = period_id, @fy_id = fiscal_year_id, @pst = status
    FROM dbo.acc_fiscal_period
    WHERE company_id = @company_id AND CAST(N''[DATE]'' AS DATE) BETWEEN start_date AND end_date;
    IF @pid IS NULL THROW 51033, N''No fiscal period for entry date'', 1;
    IF @pst <> N''OPEN'' THROW 51034, N''Fiscal period is not OPEN'', 1;

    SET @use_ref = N''[SR]'';
    SET @n = 1;
    WHILE EXISTS (SELECT 1 FROM dbo.acc_journal_entry
                  WHERE source_system = N''[SS]'' AND source_ref = @use_ref)
    BEGIN
        SET @n = @n + 1;
        SET @use_ref = N''[SR]'' + N''#'' + CAST(@n AS NVARCHAR(12));
    END;

    INSERT dbo.acc_journal_entry
        (company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, entry_date,
         narration, reference_no, source_system, source_ref, created_by)
    VALUES
        (@company_id, @vt_id, @br_id, @fy_id, @pid, CAST(N''[DATE]'' AS DATE),
         N''[NAR]'', NULL, N''[SS]'', @use_ref, CAST(1 AS NVARCHAR(100)));
    SET @e = SCOPE_IDENTITY();
';

/* ---------------------------------------------------------------------------
   post_batch — mirrors AccountingRepository.PostVoucherAsync step 3:
   checks >= 2 lines + balanced, then takes the next sequence no (gapless) and
   flips the entry to POSTED. Uses @e/@company_id/@vt_id/@fy_id/@br_id from the
   create batch (must be appended to the same @sql string).
   --------------------------------------------------------------------------- */
DECLARE @post NVARCHAR(MAX) = N'
    DECLARE @cnt INT, @dr DECIMAL(18,2), @cr DECIMAL(18,2), @no INT;
    SELECT @cnt = COUNT(*), @dr = ISNULL(SUM(debit),0), @cr = ISNULL(SUM(credit),0)
    FROM dbo.acc_journal_line WHERE entry_id = @e;
    IF @cnt < 2 THROW 51047, N''Entry needs at least 2 lines'', 1;
    IF @dr <> @cr THROW 51048, N''Entry is unbalanced'', 1;

    UPDATE dbo.acc_voucher_sequence WITH (UPDLOCK, HOLDLOCK)
       SET @no = last_no = last_no + 1
     WHERE company_id = @company_id AND voucher_type_id = @vt_id
       AND fiscal_year_id = @fy_id AND branch_id = @br_id;
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT dbo.acc_voucher_sequence (company_id, voucher_type_id, fiscal_year_id, branch_id, last_no)
        VALUES (@company_id, @vt_id, @fy_id, @br_id, 1);
        SET @no = 1;
    END;

    UPDATE dbo.acc_journal_entry
       SET status = ''POSTED'', voucher_no = @no,
           posted_by = CAST(1 AS NVARCHAR(100)), posted_at = SYSUTCDATETIME()
     WHERE entry_id = @e;
';

DECLARE @sql NVARCHAR(MAX), @t NVARCHAR(MAX);

/* ---------------------------------------------------------------------------
   NEGATIVE TESTS
   --------------------------------------------------------------------------- */

/* N1 — Unbalanced entry must not post (repository check 51048) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N1');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N1');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 99, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
EXEC #acc_expect_error @test_name = N'N1 unbalanced entry rejected', @expected_error = 51048, @sql = @t;

/* N2 — Single-line entry must not post (repository check 51047) */
SET @sql = REPLACE(@create, N'[BR]', N'BT');
SET @sql = REPLACE(@sql, N'[VT]', N'JRN');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N2');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N2');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 10, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';';
SET @t = @t + @post;
EXEC #acc_expect_error @test_name = N'N2 single-line entry rejected', @expected_error = 51047, @sql = @t;

/* N3 — Entry dated into a CLOSED period must be rejected (repository check 51034) */
DECLARE @closed_date DATE = (SELECT TOP 1 start_date FROM dbo.acc_fiscal_period
                             WHERE status = N'CLOSED' ORDER BY start_date DESC);
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'JRN');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N3');
SET @sql = REPLACE(@sql, N'[DATE]', CONVERT(NVARCHAR(10), @closed_date, 120));
SET @sql = REPLACE(@sql, N'[NAR]', N'N3');
EXEC #acc_expect_error @test_name = N'N3 closed period rejected', @expected_error = 51034, @sql = @sql;

/* N4 — Group account must not be used on a line (trigger 51023) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'JRN');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N4');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N4');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, (SELECT TOP 1 account_id FROM dbo.acc_account WHERE is_group = 1), @br_id, NULL, 100, 0, NULL;
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';';
EXEC #acc_expect_error @test_name = N'N4 group account rejected', @expected_error = 51023, @sql = @t;

/* N5 — Party-required account without a party must fail (trigger 51025) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'PUR');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N5');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N5');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''510001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''210001'';';
EXEC #acc_expect_error @test_name = N'N5 missing required party rejected', @expected_error = 51025, @sql = @t;

/* N6 — Wrong party type on a party-required account must fail (trigger 51025) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'RCT');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N6');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N6');
SET @t = @sql;
SET @t = @t + N'
    DECLARE @sup INT;
    SELECT @sup = party_id FROM dbo.acc_party
    WHERE company_id = @company_id AND party_type = N''SUPPLIER'' AND code = N''SMOKE-SUP'';
    IF @sup IS NULL
    BEGIN
        INSERT dbo.acc_party (company_id, party_type, code, name, home_branch_id, phone, is_active)
        VALUES (@company_id, N''SUPPLIER'', N''SMOKE-SUP'', N''Smoke Supplier'', @br_id, NULL, 1);
        SET @sup = SCOPE_IDENTITY();
    END
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, @sup, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''119001'';';
EXEC #acc_expect_error @test_name = N'N6 wrong party type rejected', @expected_error = 51027, @sql = @t;

/* N7 — Line on an already POSTED entry must fail (trigger 51021) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N7');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N7');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 3, account_id, @br_id, NULL, 0, 1, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
EXEC #acc_expect_error @test_name = N'N7 line on posted entry rejected', @expected_error = 51021, @sql = @t;

/* N8 — Hard delete of a POSTED entry must fail (trigger 51010) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N8');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N8');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
SET @t = @t + N'
    DELETE FROM dbo.acc_journal_entry WHERE entry_id = @e;';
EXEC #acc_expect_error @test_name = N'N8 posted entry not deletable', @expected_error = 51010, @sql = @t;

/* N9 — Editing an immutable field of a POSTED entry must fail (trigger 51011) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'N9');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'N9');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
SET @t = @t + N'
    UPDATE dbo.acc_journal_entry SET narration = N''hack'' WHERE entry_id = @e;';
EXEC #acc_expect_error @test_name = N'N9 posted entry immutable', @expected_error = 51011, @sql = @t;

/* N10 — LOCKED period cannot be reopened (trigger 51009) */
EXEC #acc_expect_error @test_name = N'N10 locked period cannot be reopened', @expected_error = 51009, @sql = N'
    DECLARE @pid INT = (SELECT TOP 1 period_id FROM dbo.acc_fiscal_period
                        WHERE status = N''CLOSED'' ORDER BY start_date DESC);
    UPDATE dbo.acc_fiscal_period SET status = N''LOCKED'' WHERE period_id = @pid;
    UPDATE dbo.acc_fiscal_period SET status = N''OPEN'' WHERE period_id = @pid;';

/* ---------------------------------------------------------------------------
   POSITIVE TESTS
   --------------------------------------------------------------------------- */

/* P1 — Full lifecycle: create -> post -> reverse (asserts) */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'P1');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'P1');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
SET @t = @t + N'
    IF NOT EXISTS (SELECT 1 FROM dbo.acc_journal_entry WHERE entry_id = @e AND status = N''POSTED'' AND voucher_no IS NOT NULL)
        THROW 55551, N''entry did not reach POSTED with a voucher no'', 1;

    DECLARE @orig BIGINT = @e, @orig_date DATE;
    SELECT @orig_date = entry_date FROM dbo.acc_journal_entry WHERE entry_id = @orig;

    -- reverse batch: insert reversal header, mirror lines Dr/Cr-swapped, post it, mark original REVERSED
    DECLARE @rv_id BIGINT;
    INSERT dbo.acc_journal_entry
        (company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, entry_date,
         narration, reference_no, source_system, source_ref, reversal_of, created_by)
    SELECT company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, @orig_date,
           N''Reversal of entry '' + CAST(entry_id AS NVARCHAR(20)), NULL,
           N''REVERSAL'', N''REV-'' + CAST(entry_id AS NVARCHAR(20)), entry_id,
           CAST(1 AS NVARCHAR(100))
    FROM dbo.acc_journal_entry WHERE entry_id = @orig;
    SET @rv_id = SCOPE_IDENTITY();

    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @rv_id, line_no, account_id, branch_id, party_id, credit, debit, N''REV '' + ISNULL(memo, N'''')
    FROM dbo.acc_journal_line WHERE entry_id = @orig;

    DECLARE @rc2 INT, @rd2 DECIMAL(18,2), @rcr2 DECIMAL(18,2), @rno2 INT;
    SELECT @rc2 = COUNT(*), @rd2 = ISNULL(SUM(debit),0), @rcr2 = ISNULL(SUM(credit),0)
    FROM dbo.acc_journal_line WHERE entry_id = @rv_id;
    IF @rc2 < 2 OR @rd2 <> @rcr2 THROW 51048, N''unbalanced reversal'', 1;

    UPDATE dbo.acc_voucher_sequence WITH (UPDLOCK, HOLDLOCK)
       SET @rno2 = last_no = last_no + 1
     WHERE company_id = @company_id AND voucher_type_id = @vt_id
       AND fiscal_year_id = @fy_id AND branch_id = @br_id;
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT dbo.acc_voucher_sequence (company_id, voucher_type_id, fiscal_year_id, branch_id, last_no)
        VALUES (@company_id, @vt_id, @fy_id, @br_id, 1);
        SET @rno2 = 1;
    END;
    UPDATE dbo.acc_journal_entry
       SET status = ''POSTED'', voucher_no = @rno2,
           posted_by = CAST(1 AS NVARCHAR(100)), posted_at = SYSUTCDATETIME()
     WHERE entry_id = @rv_id;

    UPDATE dbo.acc_journal_entry SET status = ''REVERSED'' WHERE entry_id = @orig;

    IF NOT EXISTS (SELECT 1 FROM dbo.acc_journal_entry WHERE entry_id = @rv_id AND status = N''POSTED''
                   AND source_system = N''REVERSAL'' AND source_ref = N''REV-'' + CAST(@orig AS NVARCHAR(20)))
        THROW 55552, N''reversal entry missing'', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.acc_journal_entry WHERE entry_id = @orig AND status = N''REVERSED'')
        THROW 55553, N''original not marked REVERSED'', 1;

    -- idempotent lookup must find NO active entry for the base ref after reversal
    IF EXISTS (SELECT 1 FROM dbo.acc_journal_entry
               WHERE source_system = N''SMOKE'' AND (source_ref = N''P1'' OR source_ref LIKE N''P1#%'')
                 AND status <> N''REVERSED'')
        THROW 55554, N''GetEntryId found an active entry after reversal'', 1;
';
EXEC #acc_expect_ok @test_name = N'P1 full lifecycle posted + reversed', @sql = @t;

/* P1b — Edit cycle: post base ref, reverse it, re-post the same ref. The
         second post must take a suffixed ref (P1b#1) and become the active
         entry that GetEntryId returns. Self-contained (P1 rolls back). */
SET @sql = REPLACE(@create, N'[BR]', N'HO');
SET @sql = REPLACE(@sql, N'[VT]', N'SAL');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'P1b');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'P1b');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 100, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 100, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';';
SET @t = @t + @post;
SET @t = @t + N'

    -- reverse base entry (@e) and mark it REVERSED
    DECLARE @rb BIGINT, @rb_date DATE;
    SELECT @rb_date = entry_date FROM dbo.acc_journal_entry WHERE entry_id = @e;
    INSERT dbo.acc_journal_entry
        (company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, entry_date,
         narration, reference_no, source_system, source_ref, reversal_of, created_by)
    SELECT company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, @rb_date,
           N''Reversal of entry '' + CAST(entry_id AS NVARCHAR(20)), NULL,
           N''REVERSAL'', N''REV-'' + CAST(entry_id AS NVARCHAR(20)), entry_id,
           CAST(1 AS NVARCHAR(100))
    FROM dbo.acc_journal_entry WHERE entry_id = @e;
    SET @rb = SCOPE_IDENTITY();
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @rb, line_no, account_id, branch_id, party_id, credit, debit, N''REV '' + ISNULL(memo, N'''')
    FROM dbo.acc_journal_line WHERE entry_id = @e;
    DECLARE @rn2 INT;
    UPDATE dbo.acc_voucher_sequence WITH (UPDLOCK, HOLDLOCK)
       SET @rn2 = last_no = last_no + 1
     WHERE company_id = @company_id AND voucher_type_id = @vt_id
       AND fiscal_year_id = @fy_id AND branch_id = @br_id;
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT dbo.acc_voucher_sequence (company_id, voucher_type_id, fiscal_year_id, branch_id, last_no)
        VALUES (@company_id, @vt_id, @fy_id, @br_id, 1);
        SET @rn2 = 1;
    END;
    UPDATE dbo.acc_journal_entry
       SET status = ''POSTED'', voucher_no = @rn2,
           posted_by = CAST(1 AS NVARCHAR(100)), posted_at = SYSUTCDATETIME()
     WHERE entry_id = @rb;
    UPDATE dbo.acc_journal_entry SET status = ''REVERSED'' WHERE entry_id = @e;

    -- re-create the same source ref -> must be suffixed P1b#2 (the repository
    -- pre-increments: base is revision 1, so the first collision becomes #2)
    SET @use_ref = N''P1b'';
    SET @n = 1;
    WHILE EXISTS (SELECT 1 FROM dbo.acc_journal_entry
                  WHERE source_system = N''SMOKE'' AND source_ref = @use_ref)
    BEGIN
        SET @n = @n + 1;
        SET @use_ref = N''P1b'' + N''#'' + CAST(@n AS NVARCHAR(12));
    END;
    DECLARE @m2 NVARCHAR(200) = N''expected suffixed ref P1b#2, got: '' + CAST(@use_ref AS NVARCHAR(50));
    IF @use_ref <> N''P1b#2'' THROW 55555, @m2, 1;

    DECLARE @e2 BIGINT;
    INSERT dbo.acc_journal_entry
        (company_id, voucher_type_id, branch_id, fiscal_year_id, period_id, entry_date,
         narration, reference_no, source_system, source_ref, created_by)
    VALUES (@company_id, @vt_id, @br_id, @fy_id, @pid, CAST(N''2026-09-20'' AS DATE),
            N''P1b re-post'', NULL, N''SMOKE'', @use_ref, CAST(1 AS NVARCHAR(100)));
    SET @e2 = SCOPE_IDENTITY();

    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e2, 1, account_id, @br_id, NULL, 150, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e2, 2, account_id, @br_id, NULL, 0, 150, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';

    DECLARE @rn3 INT;
    UPDATE dbo.acc_voucher_sequence WITH (UPDLOCK, HOLDLOCK)
       SET @rn3 = last_no = last_no + 1
     WHERE company_id = @company_id AND voucher_type_id = @vt_id
       AND fiscal_year_id = @fy_id AND branch_id = @br_id;
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT dbo.acc_voucher_sequence (company_id, voucher_type_id, fiscal_year_id, branch_id, last_no)
        VALUES (@company_id, @vt_id, @fy_id, @br_id, 1);
        SET @rn3 = 1;
    END;
    UPDATE dbo.acc_journal_entry
       SET status = ''POSTED'', voucher_no = @rn3,
           posted_by = CAST(1 AS NVARCHAR(100)), posted_at = SYSUTCDATETIME()
     WHERE entry_id = @e2;

    IF (SELECT TOP 1 entry_id FROM dbo.acc_journal_entry
        WHERE source_system = N''SMOKE'' AND (source_ref = N''P1b'' OR source_ref LIKE N''P1b#%'')
          AND status <> N''REVERSED'' ORDER BY entry_id DESC) <> @e2
        THROW 55557, N''GetEntryId did not return the latest active (suffixed) entry'', 1;
';
EXEC #acc_expect_ok @test_name = N'P1b edit cycle re-posts with suffixed ref', @sql = @t;

/* P2 — Reversing a DRAFT removes it (reverse batch, DRAFT branch) */
SET @sql = REPLACE(@create, N'[BR]', N'BT');
SET @sql = REPLACE(@sql, N'[VT]', N'JRN');
SET @sql = REPLACE(@sql, N'[SS]', N'SMOKE');
SET @sql = REPLACE(@sql, N'[SR]', N'P2');
SET @sql = REPLACE(@sql, N'[DATE]', N'2026-09-20');
SET @sql = REPLACE(@sql, N'[NAR]', N'P2');
SET @t = @sql;
SET @t = @t + N'
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 1, account_id, @br_id, NULL, 5, 0, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''111001'';
    INSERT dbo.acc_journal_line (entry_id, line_no, account_id, branch_id, party_id, debit, credit, memo)
    SELECT @e, 2, account_id, @br_id, NULL, 0, 5, NULL FROM dbo.acc_account
    WHERE company_id = @company_id AND code = N''410001'';

    DECLARE @ds NVARCHAR(20);
    SELECT @ds = status FROM dbo.acc_journal_entry WITH (UPDLOCK, ROWLOCK) WHERE entry_id = @e;
    IF @ds = N''DRAFT''
    BEGIN
        DELETE FROM dbo.acc_journal_line WHERE entry_id = @e;
        DELETE FROM dbo.acc_journal_entry WHERE entry_id = @e;
    END

    IF EXISTS (SELECT 1 FROM dbo.acc_journal_entry WHERE entry_id = @e)
        THROW 55558, N''draft entry still exists'', 1;
    IF EXISTS (SELECT 1 FROM dbo.acc_journal_line WHERE entry_id = @e)
        THROW 55559, N''draft lines still exist'', 1;
';
EXEC #acc_expect_ok @test_name = N'P2 reversing a DRAFT cleans it up', @sql = @t;

/* ---------------------------------------------------------------------------
   Summary
   --------------------------------------------------------------------------- */
PRINT N'Smoke tests complete (everything rolled back - nothing persisted).';
GO