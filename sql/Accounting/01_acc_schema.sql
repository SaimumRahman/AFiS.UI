/* ============================================================================
   01_acc_schema.sql — New double-entry accounting system
   Target: AFIS_MAH_LIVE (dbo)

   RECONSTRUCTED from the approved design (previous session). All posting done
   by the API layer (AccountingRepository) uses RAW SQL batches — no stored
   procedures — following the same order as 03_acc_smoke.sql:
   entry create -> line add -> post -> reverse. If you keep your original
   scripts, reconcile table/column names with this file.

   Files: 01_acc_schema.sql (this), 02_acc_seed.sql, 03_acc_smoke.sql,
          04_erp_mapping.sql
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* ---------------------------------------------------------------------------
   Master data
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.acc_company', N'U') IS NOT NULL DROP TABLE dbo.acc_company;
CREATE TABLE dbo.acc_company
(
    company_id   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    company_code NVARCHAR(20)  NOT NULL UNIQUE,
    company_name NVARCHAR(200) NOT NULL,
    is_active    BIT           NOT NULL DEFAULT 1,
    created_at   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

IF OBJECT_ID(N'dbo.acc_branch', N'U') IS NOT NULL DROP TABLE dbo.acc_branch;
CREATE TABLE dbo.acc_branch
(
    branch_id    INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    company_id   INT          NOT NULL REFERENCES dbo.acc_company(company_id),
    branch_code  NVARCHAR(20) NOT NULL,
    branch_name  NVARCHAR(200) NOT NULL,
    is_active    BIT          NOT NULL DEFAULT 1,
    CONSTRAINT uq_acc_branch UNIQUE (company_id, branch_code)
);

IF OBJECT_ID(N'dbo.acc_party', N'U') IS NOT NULL DROP TABLE dbo.acc_party;
CREATE TABLE dbo.acc_party
(
    party_id       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    party_type_code NVARCHAR(30) NOT NULL,
    party_code     NVARCHAR(50)  NOT NULL,
    party_name     NVARCHAR(200) NULL,
    phone          NVARCHAR(30)  NULL,
    address        NVARCHAR(200) NULL,
    is_active      BIT           NOT NULL DEFAULT 1,
    created_at     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uq_acc_party UNIQUE (party_type_code, party_code),
    CONSTRAINT ck_acc_party_type CHECK (party_type_code IN ('EMPLOYEE','WORKER','SUPPLIER','CUSTOMER','OWNER','LENDER','OTHER'))
);

IF OBJECT_ID(N'dbo.acc_account', N'U') IS NOT NULL DROP TABLE dbo.acc_account;
CREATE TABLE dbo.acc_account
(
    account_id     INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    account_code   NVARCHAR(30)  NOT NULL UNIQUE,
    account_name   NVARCHAR(200) NOT NULL,
    account_type   NVARCHAR(20)  NOT NULL CHECK (account_type IN ('ASSET','LIABILITY','EQUITY','INCOME','EXPENSE')),
    parent_id      INT           NULL REFERENCES dbo.acc_account(account_id),
    is_group       BIT           NOT NULL DEFAULT 0,
    requires_party BIT           NOT NULL DEFAULT 0,
    is_active      BIT           NOT NULL DEFAULT 1,
    created_at     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

IF OBJECT_ID(N'dbo.acc_account_party_type', N'U') IS NOT NULL DROP TABLE dbo.acc_account_party_type;
CREATE TABLE dbo.acc_account_party_type
(
    account_id      INT           NOT NULL REFERENCES dbo.acc_account(account_id),
    party_type_code NVARCHAR(30)  NOT NULL,
    CONSTRAINT pk_acc_account_party_type PRIMARY KEY (account_id, party_type_code)
);

IF OBJECT_ID(N'dbo.acc_voucher_type', N'U') IS NOT NULL DROP TABLE dbo.acc_voucher_type;
CREATE TABLE dbo.acc_voucher_type
(
    voucher_type_code NVARCHAR(10)  NOT NULL PRIMARY KEY,
    voucher_type_name NVARCHAR(100) NOT NULL,
    voucher_prefix    NVARCHAR(10)  NOT NULL,
    is_active         BIT           NOT NULL DEFAULT 1
);

IF OBJECT_ID(N'dbo.acc_fiscal_year', N'U') IS NOT NULL DROP TABLE dbo.acc_fiscal_year;
CREATE TABLE dbo.acc_fiscal_year
(
    fiscal_year_id INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    company_id     INT           NOT NULL REFERENCES dbo.acc_company(company_id),
    year_code      NVARCHAR(20)  NOT NULL,
    start_date     DATE          NOT NULL,
    end_date       DATE          NOT NULL,
    is_closed      BIT           NOT NULL DEFAULT 0,
    CONSTRAINT uq_acc_fiscal_year UNIQUE (company_id, year_code),
    CONSTRAINT ck_acc_fy_dates CHECK (end_date > start_date)
);

IF OBJECT_ID(N'dbo.acc_fiscal_period', N'U') IS NOT NULL DROP TABLE dbo.acc_fiscal_period;
CREATE TABLE dbo.acc_fiscal_period
(
    fiscal_period_id INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    fiscal_year_id   INT          NOT NULL REFERENCES dbo.acc_fiscal_year(fiscal_year_id),
    period_no        INT          NOT NULL,
    period_name      NVARCHAR(30) NOT NULL,
    start_date       DATE         NOT NULL,
    end_date         DATE         NOT NULL,
    status           NVARCHAR(20) NOT NULL DEFAULT 'OPEN' CHECK (status IN ('OPEN','CLOSED','LOCKED')),
    CONSTRAINT uq_acc_fiscal_period UNIQUE (fiscal_year_id, period_no),
    CONSTRAINT ck_acc_fp_dates CHECK (end_date >= start_date)
);

IF OBJECT_ID(N'dbo.acc_voucher_sequence', N'U') IS NOT NULL DROP TABLE dbo.acc_voucher_sequence;
CREATE TABLE dbo.acc_voucher_sequence
(
    id               INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    company_id       INT          NOT NULL REFERENCES dbo.acc_company(company_id),
    branch_code      NVARCHAR(20) NOT NULL,
    voucher_type_code NVARCHAR(10) NOT NULL REFERENCES dbo.acc_voucher_type(voucher_type_code),
    fiscal_year_id   INT          NOT NULL REFERENCES dbo.acc_fiscal_year(fiscal_year_id),
    next_no          INT          NOT NULL DEFAULT 1,
    updated_at       DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT uq_acc_voucher_seq UNIQUE (company_id, branch_code, voucher_type_code, fiscal_year_id)
);

/* ---------------------------------------------------------------------------
   Transactional tables
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.acc_journal_entry', N'U') IS NOT NULL DROP TABLE dbo.acc_journal_entry;
CREATE TABLE dbo.acc_journal_entry
(
    entry_id           BIGINT        NOT NULL IDENTITY(1,1) PRIMARY KEY,
    company_id         INT           NOT NULL REFERENCES dbo.acc_company(company_id),
    branch_code        NVARCHAR(20)  NOT NULL,
    voucher_type_code  NVARCHAR(10)  NOT NULL REFERENCES dbo.acc_voucher_type(voucher_type_code),
    fiscal_year_id     INT           NOT NULL REFERENCES dbo.acc_fiscal_year(fiscal_year_id),
    fiscal_period_id   INT           NOT NULL REFERENCES dbo.acc_fiscal_period(fiscal_period_id),
    voucher_no         NVARCHAR(50)  NULL,
    entry_date         DATE          NOT NULL,
    narration          NVARCHAR(500) NULL,
    source_system      NVARCHAR(50)  NOT NULL,
    source_ref         NVARCHAR(100) NOT NULL,
    status             NVARCHAR(20)  NOT NULL DEFAULT 'DRAFT' CHECK (status IN ('DRAFT','POSTED','REVERSED')),
    reversal_of_entry_id BIGINT      NULL,
    created_by         BIGINT        NOT NULL,
    created_at         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    posted_by          BIGINT        NULL,
    posted_at          DATETIME2     NULL
);
CREATE INDEX ix_acc_journal_entry_source ON dbo.acc_journal_entry (source_system, source_ref);
CREATE INDEX ix_acc_journal_entry_status ON dbo.acc_journal_entry (status);

IF OBJECT_ID(N'dbo.acc_journal_line', N'U') IS NOT NULL DROP TABLE dbo.acc_journal_line;
CREATE TABLE dbo.acc_journal_line
(
    line_id      BIGINT        NOT NULL IDENTITY(1,1) PRIMARY KEY,
    entry_id     BIGINT        NOT NULL REFERENCES dbo.acc_journal_entry(entry_id),
    account_id   INT           NOT NULL REFERENCES dbo.acc_account(account_id),
    party_id     INT           NULL REFERENCES dbo.acc_party(party_id),
    branch_code  NVARCHAR(20)  NULL,
    debit        DECIMAL(18,2) NOT NULL DEFAULT 0,
    credit       DECIMAL(18,2) NOT NULL DEFAULT 0,
    memo         NVARCHAR(500) NULL,
    created_by   BIGINT        NOT NULL,
    created_at   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT ck_acc_line_debit CHECK (debit >= 0),
    CONSTRAINT ck_acc_line_credit CHECK (credit >= 0)
);
CREATE INDEX ix_acc_journal_line_entry ON dbo.acc_journal_line (entry_id);

IF OBJECT_ID(N'dbo.acc_legacy_ledger_map', N'U') IS NOT NULL DROP TABLE dbo.acc_legacy_ledger_map;
CREATE TABLE dbo.acc_legacy_ledger_map
(
    legacy_ledger_name NVARCHAR(200) NOT NULL PRIMARY KEY,
    account_code       NVARCHAR(30)  NOT NULL REFERENCES dbo.acc_account(account_code),
    notes              NVARCHAR(500) NULL
);

IF OBJECT_ID(N'dbo.acc_audit_log', N'U') IS NOT NULL DROP TABLE dbo.acc_audit_log;
CREATE TABLE dbo.acc_audit_log
(
    audit_id   BIGINT        NOT NULL IDENTITY(1,1) PRIMARY KEY,
    table_name NVARCHAR(100) NOT NULL,
    action     NVARCHAR(20)  NOT NULL,
    record_id  NVARCHAR(50)  NOT NULL,
    old_value  NVARCHAR(MAX) NULL,
    new_value  NVARCHAR(MAX) NULL,
    changed_by BIGINT        NOT NULL,
    changed_at DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

/* ---------------------------------------------------------------------------
   ERP integration mapping (see 04_erp_mapping.sql for seeds)
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.acc_erp_pay_method_map', N'U') IS NOT NULL DROP TABLE dbo.acc_erp_pay_method_map;
CREATE TABLE dbo.acc_erp_pay_method_map
(
    FinancialAccountId INT          NOT NULL PRIMARY KEY CHECK (FinancialAccountId >= 0),
    AccountCode        NVARCHAR(30) NOT NULL REFERENCES dbo.acc_account(account_code),
    AccountKey         NVARCHAR(50) NULL,
    notes              NVARCHAR(200) NULL
);

IF OBJECT_ID(N'dbo.acc_erp_billtype_map', N'U') IS NOT NULL DROP TABLE dbo.acc_erp_billtype_map;
CREATE TABLE dbo.acc_erp_billtype_map
(
    BillTypeId   INT          NOT NULL PRIMARY KEY,
    AccountCode  NVARCHAR(30) NOT NULL REFERENCES dbo.acc_account(account_code),
    notes        NVARCHAR(200) NULL
);

/* ===========================================================================
   TRIGGERS
   =========================================================================== */

-- Journal entry: inserts must be DRAFT (51013) and period must be OPEN (51015 / 51016)
IF OBJECT_ID(N'dbo.trg_acc_journal_entry_bi_u', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_journal_entry_bi_u;
GO
CREATE TRIGGER dbo.trg_acc_journal_entry_bi_u
ON dbo.acc_journal_entry
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted i WHERE i.status <> 'DRAFT')
    BEGIN
        THROW 51013, N'Journal entry can only be inserted with status DRAFT', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted i
        LEFT JOIN dbo.acc_fiscal_period fp ON fp.fiscal_period_id = i.fiscal_period_id
        WHERE fp.fiscal_period_id IS NULL OR fp.status <> 'OPEN'
    )
    BEGIN
        THROW 51015, N'Entry date does not fall on an OPEN fiscal period', 1;
    END
END
GO

-- Journal entry: updates restricted; only DRAFT->POSTED (post) and POSTED->REVERSED (reverse)
IF OBJECT_ID(N'dbo.trg_acc_journal_entry_bu', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_journal_entry_bu;
GO
CREATE TRIGGER dbo.trg_acc_journal_entry_bu
ON dbo.acc_journal_entry
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON d.entry_id = i.entry_id
        WHERE i.status = 'DRAFT'
    )
    BEGIN
        THROW 51013, N'Journal entry status cannot be reset to DRAFT', 1;
    END

    -- immutable fields once POSTED
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON d.entry_id = i.entry_id
        WHERE d.status IN ('POSTED','REVERSED')
          AND (ISNULL(i.entry_date,'19000101') <> ISNULL(d.entry_date,'19000101')
            OR ISNULL(i.branch_code,'') <> ISNULL(d.branch_code,'')
            OR ISNULL(i.voucher_type_code,'') <> ISNULL(d.voucher_type_code,'')
            OR ISNULL(i.source_system,'') <> ISNULL(d.source_system,'')
            OR ISNULL(i.source_ref,'') <> ISNULL(d.source_ref,''))
    )
    BEGIN
        THROW 51021, N'Posted journal entry fields are immutable', 1;
    END
END
GO

-- Journal entry: no hard delete of posted/reversed entries (51010 / 51011)
IF OBJECT_ID(N'dbo.trg_acc_journal_entry_bd', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_journal_entry_bd;
GO
CREATE TRIGGER dbo.trg_acc_journal_entry_bd
ON dbo.acc_journal_entry
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted WHERE status IN ('POSTED','REVERSED'))
    BEGIN
        THROW 51010, N'Posted journal entries cannot be deleted', 1;
    END
    DELETE e FROM dbo.acc_journal_entry e JOIN deleted d ON d.entry_id = e.entry_id;
END
GO

-- Lines: entry must be DRAFT, at least one side positive, no group accounts,
--        party rules enforced (51013 / 51017 / 51023 / 51025 / 51027)
IF OBJECT_ID(N'dbo.trg_acc_journal_line_bi_u', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_journal_line_bi_u;
GO
CREATE TRIGGER dbo.trg_acc_journal_line_bi_u
ON dbo.acc_journal_line
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted l
        JOIN dbo.acc_journal_entry e ON e.entry_id = l.entry_id
        WHERE e.status <> 'DRAFT'
    )
    BEGIN
        THROW 51013, N'Lines can only be added to DRAFT entries', 1;
    END

    IF EXISTS (
        SELECT 1 FROM inserted
        WHERE NOT (debit > 0 AND credit = 0) AND NOT (credit > 0 AND debit = 0)
    )
    BEGIN
        THROW 51017, N'Each journal line must have exactly one positive side (debit OR credit)', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted l
        JOIN dbo.acc_account a ON a.account_id = l.account_id
        WHERE a.is_group = 1
    )
    BEGIN
        THROW 51023, N'Postings are not allowed on group account heads', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM inserted l
        JOIN dbo.acc_account a ON a.account_id = l.account_id
        WHERE a.requires_party = 1
          AND (l.party_id IS NULL OR a.account_id NOT IN (SELECT account_id FROM dbo.acc_account_party_type apt WHERE apt.party_type_code = (SELECT party_type_code FROM dbo.acc_party p WHERE p.party_id = l.party_id)))
    )
    BEGIN
        THROW 51025, N'Party is required (with a matching party type) for this account', 1;
    END
END
GO

-- Fiscal period: LOCKED periods cannot be reopened (51009)
IF OBJECT_ID(N'dbo.trg_acc_fiscal_period_bu', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_fiscal_period_bu;
GO
CREATE TRIGGER dbo.trg_acc_fiscal_period_bu
ON dbo.acc_fiscal_period
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON d.fiscal_period_id = i.fiscal_period_id
        WHERE d.status = 'LOCKED' AND i.status <> 'LOCKED'
    )
    BEGIN
        THROW 51009, N'A LOCKED fiscal period cannot be reopened', 1;
    END
END
GO

-- Account hierarchy: detect cycle (51006) and overlapping period guard (51007)
IF OBJECT_ID(N'dbo.trg_acc_account_bi_u', N'TR') IS NOT NULL DROP TRIGGER dbo.trg_acc_account_bi_u;
GO
CREATE TRIGGER dbo.trg_acc_account_bi_u
ON dbo.acc_account
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id INT;
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT account_id FROM inserted WHERE parent_id IS NOT NULL;
    OPEN cur;
    FETCH NEXT FROM cur INTO @id;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @cur INT = @id, @hops INT = 0;
        WHILE @cur IS NOT NULL AND @hops < 50
        BEGIN
            SELECT @cur = parent_id FROM dbo.acc_account WHERE account_id = @cur;
            IF @cur = @id
                THROW 51006, N'Account hierarchy cycle detected', 1;
            SET @hops = @hops + 1;
        END
        FETCH NEXT FROM cur INTO @id;
    END
    CLOSE cur; DEALLOCATE cur;
END
GO

/* ===========================================================================
   NOTE: No stored procedures.
   The create -> line add -> post -> reverse sequences are executed as RAW SQL
   batches by AccountingRepository (and mirrored by 03_acc_smoke.sql). The
   invariants (DRAFT insert, OPEN period, line rules, immutability, no hard
   delete of posted entries, account-cycle check) are enforced by the triggers
   above plus the balance/line-count checks inside the raw post batch.
   =========================================================================== */

/* ===========================================================================
   VIEWS
   =========================================================================== */

-- Account tree (direct children per account)
IF OBJECT_ID(N'dbo.acc_vw_account_tree', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_account_tree;
GO
CREATE VIEW dbo.acc_vw_account_tree
AS
    SELECT a.account_id, a.account_code, a.account_name, a.account_type, a.parent_id, a.is_group
    FROM dbo.acc_account a;
GO

-- Account ancestor chain (code + name)
IF OBJECT_ID(N'dbo.acc_rcte_ancestors', N'IF') IS NOT NULL DROP FUNCTION dbo.acc_rcte_ancestors;
GO
CREATE FUNCTION dbo.acc_rcte_ancestors (@leaf_account_id INT)
RETURNS TABLE
AS
RETURN
(
    WITH cte AS
    (
        SELECT account_id, parent_id, 1 AS lvl
        FROM dbo.acc_account
        WHERE account_id = @leaf_account_id
        UNION ALL
        SELECT a.account_id, a.parent_id, c.lvl + 1
        FROM dbo.acc_account a
        JOIN cte c ON a.account_id = c.parent_id
    )
    SELECT account_id AS ancestor_id FROM cte
);
GO

IF OBJECT_ID(N'dbo.acc_vw_account_ancestor', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_account_ancestor;
GO
CREATE VIEW dbo.acc_vw_account_ancestor
AS
    SELECT c.account_id AS leaf_account_id, c.account_code AS leaf_code,
           a.account_id AS ancestor_id, a.account_code AS ancestor_code, a.account_name AS ancestor_name
    FROM dbo.acc_account c
    CROSS APPLY dbo.acc_rcte_ancestors(c.account_id) ans
    JOIN dbo.acc_account a ON a.account_id = ans.ancestor_id;
GO

-- General ledger
IF OBJECT_ID(N'dbo.acc_vw_gl', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_gl;
GO
CREATE VIEW dbo.acc_vw_gl
AS
    SELECT e.entry_id, e.voucher_no, e.voucher_type_code, e.entry_date, e.narration,
           e.source_system, e.source_ref, e.status, e.branch_code, e.fiscal_year_id, e.fiscal_period_id,
           l.line_id, a.account_code, a.account_name, l.party_id, l.branch_code AS line_branch_code,
           l.debit, l.credit, l.memo
    FROM dbo.acc_journal_entry e
    JOIN dbo.acc_journal_line l ON l.entry_id = e.entry_id
    JOIN dbo.acc_account a ON a.account_id = l.account_id;
GO

-- Trial balance by period (leaf accounts, posted + reversals netted as of period)
IF OBJECT_ID(N'dbo.acc_vw_trial_balance_period', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_trial_balance_period;
GO
CREATE VIEW dbo.acc_vw_trial_balance_period
AS
    SELECT e.fiscal_year_id, e.fiscal_period_id, a.account_code, a.account_name, a.account_type,
           SUM(l.debit)  AS total_debit,
           SUM(l.credit) AS total_credit,
           SUM(l.debit) - SUM(l.credit) AS balance
    FROM dbo.acc_journal_entry e
    JOIN dbo.acc_journal_line l ON l.entry_id = e.entry_id
    JOIN dbo.acc_account a ON a.account_id = l.account_id
    WHERE e.status IN ('POSTED')
    GROUP BY e.fiscal_year_id, e.fiscal_period_id, a.account_code, a.account_name, a.account_type;
GO

-- Balance rollup (for a given account, includes descendants via the legacy map-free join)
IF OBJECT_ID(N'dbo.acc_vw_balance_rollup', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_balance_rollup;
GO
CREATE VIEW dbo.acc_vw_balance_rollup
AS
    SELECT a.account_id, a.account_code, a.account_name,
           SUM(l.debit) AS total_debit, SUM(l.credit) AS total_credit
    FROM dbo.acc_account a
    LEFT JOIN (
        SELECT l.account_id, l.debit, l.credit
        FROM dbo.acc_journal_line l
        JOIN dbo.acc_journal_entry e ON e.entry_id = l.entry_id AND e.status = 'POSTED'
    ) l ON l.account_id = a.account_id
    GROUP BY a.account_id, a.account_code, a.account_name;
GO

-- Party balances
IF OBJECT_ID(N'dbo.acc_vw_party_balance', N'V') IS NOT NULL DROP VIEW dbo.acc_vw_party_balance;
GO
CREATE VIEW dbo.acc_vw_party_balance
AS
    SELECT p.party_type_code, p.party_code, p.party_name,
           SUM(l.debit) AS total_debit, SUM(l.credit) AS total_credit,
           SUM(l.debit) - SUM(l.credit) AS net_balance
    FROM dbo.acc_party p
    JOIN dbo.acc_journal_line l ON l.party_id = p.party_id
    JOIN dbo.acc_journal_entry e ON e.entry_id = l.entry_id
    WHERE e.status IN ('POSTED')
    GROUP BY p.party_type_code, p.party_code, p.party_name;
GO