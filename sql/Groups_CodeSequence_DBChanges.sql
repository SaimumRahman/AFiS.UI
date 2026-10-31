-- =============================================
-- Groups - auto-generated Code (sequence)
-- Mirrors the SubGroups approach (dbo.SQ_SubGroupCode)
-- Target: AFIS_MAH_LIVE (idempotent - safe to re-run)
-- =============================================

-- 1) Create the sequence used to generate Group Code on insert
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'SQ_GroupCode' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE SEQUENCE dbo.SQ_GroupCode
        AS INT
        START WITH 1
        INCREMENT BY 1;
END
GO

-- Optional: verify
-- SELECT name, start_value, increment, current_value FROM sys.sequences WHERE name = 'SQ_GroupCode';
