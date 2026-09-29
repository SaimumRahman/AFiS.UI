/* ============================================================================
   02_acc_seed.sql — Seed data for the new accounting system (AFIS_MAH_LIVE)
   Reconstructed from the approved design; a coherent subset of the full chart
   of accounts. Reconcile with your original seed if you keep one.
   Run AFTER 01_acc_schema.sql.
   ============================================================================ */
SET NOCOUNT ON;

/* ---------------------------------------------------------------------------
   1. Company
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_company WHERE company_code = N'AFHO')
    INSERT INTO dbo.acc_company (company_code, company_name, is_active)
    VALUES (N'AFHO', N'Asian Fashion House Ltd.', 1);

DECLARE @companyId INT = (SELECT company_id FROM dbo.acc_company WHERE company_code = N'AFHO');

/* ---------------------------------------------------------------------------
   2. Branches (codes mirror Stores.Code used by the API mapper)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_branch WHERE company_id = @companyId)
BEGIN
    INSERT INTO dbo.acc_branch (company_id, branch_code, branch_name) VALUES
    (@companyId, N'HO',  N'Head Office'),
    (@companyId, N'BT',  N'Banani Tailors'),
    (@companyId, N'GEC', N'GEC Store'),
    (@companyId, N'HLS', N'Hall Store'),
    (@companyId, N'NM',  N'Nabil Modern'),
    (@companyId, N'FAC', N'Factory');
END

/* ---------------------------------------------------------------------------
   3. Voucher types
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_voucher_type WHERE voucher_type_code = N'SAL')
BEGIN
    INSERT INTO dbo.acc_voucher_type (voucher_type_code, voucher_type_name, voucher_prefix) VALUES
    (N'PMT', N'Payment',    N'PMT'),
    (N'RCT', N'Receipt',    N'RCT'),
    (N'JRN', N'Journal',    N'JRN'),
    (N'CTR', N'Contra',     N'CTR'),
    (N'SAL', N'Sales',      N'SAL'),
    (N'PUR', N'Purchase',   N'PUR');
END

/* ---------------------------------------------------------------------------
   4. Fiscal years + periods
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_fiscal_year WHERE company_id = @companyId AND year_code = N'FY2025-26')
BEGIN
    INSERT INTO dbo.acc_fiscal_year (company_id, year_code, start_date, end_date, is_closed)
    VALUES (@companyId, N'FY2025-26', N'2025-07-01', N'2026-06-30', 1),
           (@companyId, N'FY2026-27', N'2026-07-01', N'2027-06-30', 0);
END

DECLARE @fy25 INT = (SELECT fiscal_year_id FROM dbo.acc_fiscal_year WHERE company_id = @companyId AND year_code = N'FY2025-26');
DECLARE @fy26 INT = (SELECT fiscal_year_id FROM dbo.acc_fiscal_year WHERE company_id = @companyId AND year_code = N'FY2026-27');
DECLARE @yr  DATE, @m INT;

IF NOT EXISTS (SELECT 1 FROM dbo.acc_fiscal_period WHERE fiscal_year_id = @fy26)
BEGIN
    DECLARE cur_fy CURSOR LOCAL FAST_FORWARD FOR SELECT year_code FROM dbo.acc_fiscal_year WHERE company_id = @companyId;
    DECLARE @yc NVARCHAR(20);
    OPEN cur_fy;
    FETCH NEXT FROM cur_fy INTO @yc;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @m = 1;
        WHILE @m <= 12
        BEGIN
            DECLARE @pstart DATE = DATEFROMPARTS(CASE WHEN @m BETWEEN 7 AND 12 THEN 2026 ELSE 2027 END,
                                                CASE WHEN @m BETWEEN 7 AND 12 THEN @m - 6 ELSE @m + 6 END, 1);
            DECLARE @pend DATE = DATEADD(DAY, -1, DATEADD(MONTH, 1, @pstart));
            DECLARE @fyId INT = (SELECT fiscal_year_id FROM dbo.acc_fiscal_year WHERE company_id = @companyId AND year_code = @yc);
            DECLARE @periodStatus NVARCHAR(20) = CASE
                WHEN @yc = N'FY2025-26' THEN N'CLOSED'
                WHEN @m <= 3 THEN N'OPEN'          -- Jul-Sep 2026 open for current work
                ELSE N'CLOSED'
            END;
            INSERT INTO dbo.acc_fiscal_period (fiscal_year_id, period_no, period_name, start_date, end_date, status)
            VALUES (@fyId, @m, N'P' + CONVERT(NVARCHAR(2), @m), @pstart, @pend, @periodStatus);
            SET @m = @m + 1;
        END
        FETCH NEXT FROM cur_fy INTO @yc;
    END
    CLOSE cur_fy; DEALLOCATE cur_fy;
END

/* ---------------------------------------------------------------------------
   5. Chart of accounts (group heads + the leaf accounts the API writes to)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_account WHERE account_code = N'1000')
BEGIN
    INSERT INTO dbo.acc_account (account_code, account_name, account_type, parent_id, is_group) VALUES
    (N'1000', N'Assets',             N'ASSET',    NULL, 1),
    (N'2000', N'Liabilities',        N'LIABILITY', NULL, 1),
    (N'3000', N'Owner''s Equity',    N'EQUITY',   NULL, 1),
    (N'4000', N'Income',             N'INCOME',   NULL, 1),
    (N'5000', N'Expenses',           N'EXPENSE',  NULL, 1);

    INSERT INTO dbo.acc_account (account_code, account_name, account_type, parent_id, is_group, requires_party)
    VALUES
    (N'111001', N'Cash',                      N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'112001', N'bKash',                     N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'112002', N'Nagad',                     N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'112003', N'Rocket',                    N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'112004', N'Upay',                      N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'113001', N'POS Clearing',              N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'114001', N'Bank Account',              N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'121001', N'Raw Materials Stock',       N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'122001', N'Finished Goods Stock',      N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 0),
    (N'119001', N'Sundry Debtors',            N'ASSET',    (SELECT account_id FROM dbo.acc_account WHERE account_code = N'1000'), 0, 1),
    (N'210001', N'Sundry Creditors',          N'LIABILITY',(SELECT account_id FROM dbo.acc_account WHERE account_code = N'2000'), 0, 1),
    (N'300002', N'Owner Drawings',            N'EQUITY',   (SELECT account_id FROM dbo.acc_account WHERE account_code = N'3000'), 0, 1),
    (N'314001', N'Opening Balance Equity',    N'EQUITY',   (SELECT account_id FROM dbo.acc_account WHERE account_code = N'3000'), 0, 0),
    (N'410001', N'Readymade Sales',           N'INCOME',   (SELECT account_id FROM dbo.acc_account WHERE account_code = N'4000'), 0, 0),
    (N'410002', N'Tailoring Income',          N'INCOME',   (SELECT account_id FROM dbo.acc_account WHERE account_code = N'4000'), 0, 0),
    (N'510001', N'Fabric Purchase',           N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'510002', N'Raw Material Purchase',     N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530001', N'Miscellaneous Expense',     N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530002', N'Electricity Expense',       N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530003', N'Rent Expense',              N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530004', N'Wages & Salary',            N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530005', N'Transportation Expense',    N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0),
    (N'530006', N'Entertainment Expense',     N'EXPENSE',  (SELECT account_id FROM dbo.acc_account WHERE account_code = N'5000'), 0, 0);

    INSERT INTO dbo.acc_account_party_type (account_id, party_type_code) VALUES
    ((SELECT account_id FROM dbo.acc_account WHERE account_code = N'119001'), N'CUSTOMER'),
    ((SELECT account_id FROM dbo.acc_account WHERE account_code = N'210001'), N'SUPPLIER'),
    ((SELECT account_id FROM dbo.acc_account WHERE account_code = N'300002'), N'OWNER');
END

/* ---------------------------------------------------------------------------
   6. Parties (sample)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_party WHERE party_type_code = N'SUPPLIER' AND party_code = N'S-001')
BEGIN
    INSERT INTO dbo.acc_party (party_type_code, party_code, party_name, is_active) VALUES
    (N'SUPPLIER', N'S-001',  N'Asian Fabrics Ltd.',   1),
    (N'SUPPLIER', N'S-002',  N'Button House',         1),
    (N'CUSTOMER', N'C-1',    N'Walk-in Customer',     1),
    (N'CUSTOMER', N'C-2',    N'Corporate Client',     1),
    (N'EMPLOYEE', N'E-04',   N'Demo Employee',        1),
    (N'WORKER',   N'W-1002', N'Demo Worker',          1),
    (N'OWNER',    N'O-001',  N'Owner',                1),
    (N'LENDER',   N'L-001',  N'Bank Loan',            1);
END

/* ---------------------------------------------------------------------------
   7. Sample entries (raw SQL create -> line -> post, same as the API)
   --------------------------------------------------------------------------- */

IF NOT EXISTS (SELECT 1 FROM dbo.acc_journal_entry WHERE source_system = N'SALE' AND source_ref = N'SALE-DEMO-0001')
BEGIN
    DECLARE @companyIdSeed INT = (SELECT company_id FROM dbo.acc_company WHERE company_code = N'AFHO');
    DECLARE @fyIdSeed INT = (SELECT fy.fiscal_year_id FROM dbo.acc_fiscal_year fy WHERE fy.company_id = @companyIdSeed AND fy.year_code = N'FY2026-27');
    DECLARE @eSeed BIGINT;

    /* 7.1 Cash sale 5,000 (BT) */
    INSERT INTO dbo.acc_journal_entry (company_id, branch_code, voucher_type_code, fiscal_year_id, fiscal_period_id,
                                       entry_date, narration, source_system, source_ref, status, created_by)
    SELECT @companyIdSeed, N'BT', N'SAL', @fyIdSeed, fp.fiscal_period_id,
           N'2026-09-15', N'Sales - DEMO-0001 [BT]', N'SALE', N'SALE-DEMO-0001', N'DRAFT', 1
    FROM dbo.acc_fiscal_period fp
    WHERE fp.fiscal_year_id = @fyIdSeed AND N'2026-09-15' BETWEEN fp.start_date AND fp.end_date;
    SET @eSeed = SCOPE_IDENTITY();
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'111001'), 5000, 0, 1);
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'410001'), 0, 5000, 1);
    INSERT INTO dbo.acc_voucher_sequence (company_id, branch_code, voucher_type_code, fiscal_year_id, next_no, updated_at)
    VALUES (@companyIdSeed, N'BT', N'SAL', @fyIdSeed, 2, SYSUTCDATETIME());
    UPDATE dbo.acc_journal_entry
    SET status = N'POSTED', voucher_no = N'SAL-2026-000001', posted_by = 1, posted_at = SYSUTCDATETIME()
    WHERE entry_id = @eSeed;

    /* 7.2 Fabric purchase 30,000 (10,000 cash, 20,000 due to S-001) */
    INSERT INTO dbo.acc_journal_entry (company_id, branch_code, voucher_type_code, fiscal_year_id, fiscal_period_id,
                                       entry_date, narration, source_system, source_ref, status, created_by)
    SELECT @companyIdSeed, N'BT', N'PUR', @fyIdSeed, fp.fiscal_period_id,
           N'2026-09-16', N'Purchase - DEMO-PUR-0002', N'PUR', N'PUR-DEMO-PUR-0002', N'DRAFT', 1
    FROM dbo.acc_fiscal_period fp
    WHERE fp.fiscal_year_id = @fyIdSeed AND N'2026-09-16' BETWEEN fp.start_date AND fp.end_date;
    SET @eSeed = SCOPE_IDENTITY();
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'510001'), 30000, 0, 1);
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'111001'), 0, 10000, 1);
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, party_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'210001'),
            (SELECT party_id FROM dbo.acc_party WHERE party_type_code = N'SUPPLIER' AND party_code = N'S-001'), 0, 20000, 1);
    INSERT INTO dbo.acc_voucher_sequence (company_id, branch_code, voucher_type_code, fiscal_year_id, next_no, updated_at)
    VALUES (@companyIdSeed, N'BT', N'PUR', @fyIdSeed, 2, SYSUTCDATETIME());
    UPDATE dbo.acc_journal_entry
    SET status = N'POSTED', voucher_no = N'PUR-2026-000001', posted_by = 1, posted_at = SYSUTCDATETIME()
    WHERE entry_id = @eSeed;

    /* 7.3 Electricity expense 2,500 cash (HO) */
    INSERT INTO dbo.acc_journal_entry (company_id, branch_code, voucher_type_code, fiscal_year_id, fiscal_period_id,
                                       entry_date, narration, source_system, source_ref, status, created_by)
    SELECT @companyIdSeed, N'HO', N'PMT', @fyIdSeed, fp.fiscal_period_id,
           N'2026-09-18', N'Expense - Electricity bill', N'EXP', N'EXP-DEMO-0003', N'DRAFT', 1
    FROM dbo.acc_fiscal_period fp
    WHERE fp.fiscal_year_id = @fyIdSeed AND N'2026-09-18' BETWEEN fp.start_date AND fp.end_date;
    SET @eSeed = SCOPE_IDENTITY();
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'530002'), 2500, 0, 1);
    INSERT INTO dbo.acc_journal_line (entry_id, account_id, debit, credit, created_by)
    VALUES (@eSeed, (SELECT account_id FROM dbo.acc_account WHERE account_code = N'111001'), 0, 2500, 1);
    INSERT INTO dbo.acc_voucher_sequence (company_id, branch_code, voucher_type_code, fiscal_year_id, next_no, updated_at)
    VALUES (@companyIdSeed, N'HO', N'PMT', @fyIdSeed, 2, SYSUTCDATETIME());
    UPDATE dbo.acc_journal_entry
    SET status = N'POSTED', voucher_no = N'PMT-2026-000001', posted_by = 1, posted_at = SYSUTCDATETIME()
    WHERE entry_id = @eSeed;
END

/* ---------------------------------------------------------------------------
   8. Close FY2025-26
   --------------------------------------------------------------------------- */
IF @fy25 IS NOT NULL
BEGIN
    UPDATE dbo.acc_fiscal_period SET status = N'CLOSED' WHERE fiscal_year_id = @fy25;
END

/* ---------------------------------------------------------------------------
   9. Tally legacy ledger map samples
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.acc_legacy_ledger_map WHERE legacy_ledger_name = N'Cash A/c')
BEGIN
    INSERT INTO dbo.acc_legacy_ledger_map (legacy_ledger_name, account_code, notes) VALUES
    (N'Cash A/c',                 N'111001', N'Tally legacy -> new model'),
    (N'Bank A/c',                 N'114001', N'Tally legacy -> new model'),
    (N'bKash A/c',                N'112001', N'Tally legacy -> new model'),
    (N'Sundry Debtors',           N'119001', N'Tally legacy -> new model'),
    (N'Sundry Creditors',         N'210001', N'Tally legacy -> new model'),
    (N'Sales A/c',                N'410001', N'Tally legacy -> new model'),
    (N'Purchase A/c',             N'510001', N'Tally legacy -> new model'),
    (N'Main Boss Cash Withdrawal', N'300002', N'Tally legacy -> Owner Drawings');
END

PRINT N'Seed complete.';
GO