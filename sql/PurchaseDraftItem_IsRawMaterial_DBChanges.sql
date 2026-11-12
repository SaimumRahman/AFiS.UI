/* =====================================================================
   PurchaseDraftItems.IsRawMaterial
   ---------------------------------------------------------------------
   Purchase Entry has three independent item-type checkboxes
   (Saleable / Consume / Raw Materials). PurchaseDraftItems stored
   IsSaleable and IsConsume but not IsRawMaterial, so saving a draft and
   re-opening it silently cleared the Raw Materials flag before the
   purchase was posted to Items.

   Target DB: AFIS_MAH_LIVE (production), schema dbo.
   Idempotent.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH('dbo.PurchaseDraftItems', 'IsRawMaterial') IS NULL
BEGIN
    ALTER TABLE dbo.PurchaseDraftItems
        ADD IsRawMaterial BIT NOT NULL CONSTRAINT DF_PurchaseDraftItems_IsRawMaterial DEFAULT (0);
    PRINT 'Added dbo.PurchaseDraftItems.IsRawMaterial';
END
ELSE
    PRINT 'dbo.PurchaseDraftItems.IsRawMaterial already exists';
GO
