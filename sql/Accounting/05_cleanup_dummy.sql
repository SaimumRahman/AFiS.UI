/* ============================================================================
   05_cleanup_dummy.sql — Remove the seed/dummy data, CAPITAL/snapshot only,
   NOT auto-run. Written against the REAL schema in AFIS_MAH_LIVE.

   Removes:
     1. Seed/mock GL entries      (source_system IN 'SEED','REVERSAL' + their
        linked entries) and their journal lines
     2. Sample/mock parties       (name LIKE '%Mock%', only when unreferenced)
     3. Seed voucher counters     (acc_voucher_sequence reset from remaining
        entries, i.e. to 0)
     4. Seed legacy ledger maps   (acc_legacy_ledger_map — fully re-seeded)
     5. Audit rows for the tables above

   KEEPS all master config: acc_company (1), acc_branch (6), acc_voucher_type
   (6), acc_fiscal_year/acc_fiscal_period (24), acc_account chart (97).

   The guard trigger acc_trg_journal_entry_guard rejects deleting non-DRAFT
   entries, so it (and the audit trigger) are disabled only for the DELETE and
   re-enabled immediately after. Everything runs in one transaction.
   ============================================================================ */
SET NOCOUNT ON;
BEGIN TRAN;

BEGIN TRY
    /* -----------------------------------------------------------------------
       0. Collect entry ids to remove (SEED + REVERSAL + anything linked)
       ----------------------------------------------------------------------- */
    IF OBJECT_ID(N'tempdb..#acc_demo') IS NOT NULL DROP TABLE #acc_demo;
    CREATE TABLE #acc_demo (entry_id BIGINT PRIMARY KEY);

    INSERT INTO #acc_demo (entry_id)
    SELECT entry_id FROM dbo.acc_journal_entry
    WHERE source_system IN (N'SEED', N'REVERSAL');

    INSERT INTO #acc_demo (entry_id)
    SELECT entry_id FROM dbo.acc_journal_entry
    WHERE reversal_of IN (SELECT entry_id FROM #acc_demo)
      AND entry_id NOT IN (SELECT entry_id FROM #acc_demo);

    DECLARE @demo_cnt INT = (SELECT COUNT(*) FROM #acc_demo);

    -- preview everything that will be deleted (nothing is deleted yet)
    SELECT N'== demo entries to delete ==' AS info;
    SELECT e.entry_id, e.voucher_type_id, e.branch_id, e.fiscal_year_id, e.period_id,
           e.voucher_no, e.entry_date, e.source_system, e.source_ref, e.status, e.reversal_of
    FROM dbo.acc_journal_entry e JOIN #acc_demo d ON d.entry_id = e.entry_id
    ORDER BY e.entry_id;

    PRINT N'[05] demo entries: ' + CAST(@demo_cnt AS NVARCHAR(10));

    /* -----------------------------------------------------------------------
       1. Journal lines of those entries (must be removed before the entries)
       ----------------------------------------------------------------------- */
    DELETE l
    FROM dbo.acc_journal_line l
    JOIN #acc_demo d ON d.entry_id = l.entry_id;
    PRINT N'[05] journal lines removed: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    /* -----------------------------------------------------------------------
       2. Entries (guard/audit triggers disabled only for this DELETE)
       ----------------------------------------------------------------------- */
    DISABLE TRIGGER dbo.acc_trg_journal_entry_guard ON dbo.acc_journal_entry;
    DISABLE TRIGGER dbo.acc_trg_journal_entry_audit ON dbo.acc_journal_entry;

    DELETE e
    FROM dbo.acc_journal_entry e
    JOIN #acc_demo d ON d.entry_id = e.entry_id;
    PRINT N'[05] journal entries removed: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    ENABLE TRIGGER dbo.acc_trg_journal_entry_guard ON dbo.acc_journal_entry;
    ENABLE TRIGGER dbo.acc_trg_journal_entry_audit ON dbo.acc_journal_entry;

    DROP TABLE #acc_demo;

    /* -----------------------------------------------------------------------
       3. Mock parties (only when not referenced by any remaining journal line)
       ----------------------------------------------------------------------- */
    SELECT N'== mock parties to delete ==' AS info;
    SELECT p.party_id, p.party_type, p.code, p.name, p.home_branch_id
    FROM dbo.acc_party p WHERE p.name LIKE N'%Mock%';

    DELETE p
    FROM dbo.acc_party p
    WHERE p.name LIKE N'%Mock%'
      AND NOT EXISTS (SELECT 1 FROM dbo.acc_journal_line l WHERE l.party_id = p.party_id);
    PRINT N'[05] mock parties removed: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    /* -----------------------------------------------------------------------
       4. Reset voucher counters from whatever entries remain (0 after this run)
       ----------------------------------------------------------------------- */
    UPDATE s
    SET last_no = COALESCE((
            SELECT MAX(e.voucher_no)
            FROM dbo.acc_journal_entry e
            WHERE e.company_id = s.company_id
              AND e.voucher_type_id = s.voucher_type_id
              AND e.fiscal_year_id = s.fiscal_year_id
              AND e.branch_id = s.branch_id
        ), 0)
    FROM dbo.acc_voucher_sequence s;
    PRINT N'[05] voucher counters reset: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    /* -----------------------------------------------------------------------
       5. Seed legacy-ledger maps (all rows; re-seed after the real mapping)
       ----------------------------------------------------------------------- */
    SELECT N'== legacy ledger maps to delete ==' AS info;
    SELECT m.company_id, m.tally_ledger_name, m.tally_parent_group, m.account_id,
           m.branch_id, m.party_id, m.notes
    FROM dbo.acc_legacy_ledger_map m ORDER BY m.tally_ledger_name;

    DELETE FROM dbo.acc_legacy_ledger_map;
    PRINT N'[05] legacy ledger map rows removed: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    /* -----------------------------------------------------------------------
       6. Audit rows for the cleaned tables
       ----------------------------------------------------------------------- */
    DELETE FROM dbo.acc_audit_log
    WHERE table_name IN (N'acc_journal_entry', N'acc_journal_line', N'acc_party');
    PRINT N'[05] audit rows removed: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

    COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK;
    PRINT N'[05] ERROR — nothing was changed. ' + ERROR_MESSAGE();
    THROW;
END CATCH;

/* ---------------------------------------------------------------------------
   Verify clean state (master config must remain untouched)
   --------------------------------------------------------------------------- */
SELECT N'journal entries'    AS item, COUNT(*) AS cnt FROM dbo.acc_journal_entry
UNION ALL SELECT N'journal lines', COUNT(*) FROM dbo.acc_journal_line
UNION ALL SELECT N'parties',        COUNT(*) FROM dbo.acc_party
UNION ALL SELECT N'voucher seqs with last_no>0', COUNT(*) FROM dbo.acc_voucher_sequence WHERE last_no > 0
UNION ALL SELECT N'legacy map rows', COUNT(*) FROM dbo.acc_legacy_ledger_map
UNION ALL SELECT N'audit rows', COUNT(*) FROM dbo.acc_audit_log
UNION ALL SELECT N'company',  COUNT(*) FROM dbo.acc_company
UNION ALL SELECT N'branches', COUNT(*) FROM dbo.acc_branch
UNION ALL SELECT N'voucher types', COUNT(*) FROM dbo.acc_voucher_type
UNION ALL SELECT N'fiscal periods', COUNT(*) FROM dbo.acc_fiscal_period
UNION ALL SELECT N'accounts (chart)', COUNT(*) FROM dbo.acc_account;

PRINT N'[05] Demo data cleanup complete. Master config retained.';
GO