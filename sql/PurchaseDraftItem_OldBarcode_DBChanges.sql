/* =====================================================================
   PurchaseDraftItems.OldBarcode
   ---------------------------------------------------------------------
   Mirrors Items.OldBarcode on the purchase draft line so a draft saved
   with a "Previous Barcode" (Opening Balance) keeps the value when it is
   reopened.

   Target DB: AFIS_MAH_LIVE (production), schema dbo.
   Idempotent.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH('dbo.PurchaseDraftItems', 'OldBarcode') IS NULL
BEGIN
    ALTER TABLE dbo.PurchaseDraftItems
        ADD OldBarcode NVARCHAR(200) NULL;
    PRINT 'Added dbo.PurchaseDraftItems.OldBarcode';
END
ELSE
    PRINT 'dbo.PurchaseDraftItems.OldBarcode already exists';
GO
