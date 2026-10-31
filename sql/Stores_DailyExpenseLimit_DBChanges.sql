/* =====================================================================
   Stores.DailyExpenseLimit
   ---------------------------------------------------------------------
   Adds a per-store daily expense cap: the total amount of Daily Expenses
   that may be entered for that store on a single day.

   - NULL or 0 means "no limit" (default), so existing behaviour is kept.
   - Enforced in the API before an expense is created/updated.

   Idempotent: safe to run multiple times.
   Run with:  SET QUOTED_IDENTIFIER ON;  (handled below)
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Stores', 'DailyExpenseLimit') IS NULL
BEGIN
    ALTER TABLE dbo.Stores ADD DailyExpenseLimit DECIMAL(18,2) NULL;
END;
GO
