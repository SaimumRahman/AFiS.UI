/* =====================================================================
   Items.OldBarcode
   ---------------------------------------------------------------------
   Purchase Entry gains a nullable "Previous Barcode" field that is shown
   only when the purchase is an Opening Balance entry. The entered value
   is persisted on the item master as OldBarcode.

   Target DB: AFIS_MAH_LIVE (production), schema dbo.
   Idempotent.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH('dbo.Items', 'OldBarcode') IS NULL
BEGIN
    ALTER TABLE dbo.Items
        ADD OldBarcode NVARCHAR(200) NULL;
    PRINT 'Added dbo.Items.OldBarcode';
END
ELSE
    PRINT 'dbo.Items.OldBarcode already exists';
GO
