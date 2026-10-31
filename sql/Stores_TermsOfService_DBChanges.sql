/* =====================================================================
   Stores.TermsOfService
   ---------------------------------------------------------------------
   Per-store invoice "Terms & Conditions" text, printed on the POS sales
   invoice PDF. Optional; long multi-line content (NVARCHAR(MAX)).

   Idempotent: safe to run multiple times.
   Run with:  SET QUOTED_IDENTIFIER ON;  (handled below)
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Stores', 'TermsOfService') IS NULL
BEGIN
    ALTER TABLE dbo.Stores ADD TermsOfService NVARCHAR(MAX) NULL;
END;
GO
