/* =====================================================================
   Stores.ExchangeDay
   ---------------------------------------------------------------------
   Adds a per-store "Exchange Day" window: the maximum age (in days) of an
   invoice that may be exchanged at that store.

   - Existing stores are backfilled to a default of 30 days.
   - The column is then made NOT NULL so every store always has a value.
   - A default constraint (30) is added so any non-application insert also
     gets a value.

   Idempotent: safe to run multiple times.
   Run with:  SET QUOTED_IDENTIFIER ON;  (handled below)
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Stores', 'ExchangeDay') IS NULL
BEGIN
    ALTER TABLE dbo.Stores ADD ExchangeDay INT NULL;
END;
GO

-- Backfill existing stores that have no value yet.
UPDATE dbo.Stores
SET ExchangeDay = 30
WHERE ExchangeDay IS NULL;
GO

-- Enforce a value going forward.
ALTER TABLE dbo.Stores ALTER COLUMN ExchangeDay INT NOT NULL;
GO

-- Safety default for inserts that do not supply the column.
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Stores_ExchangeDay')
BEGIN
    ALTER TABLE dbo.Stores
        ADD CONSTRAINT DF_Stores_ExchangeDay DEFAULT (30) FOR ExchangeDay;
END;
GO
