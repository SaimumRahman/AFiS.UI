SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ─────────────────────────────────────────────────────────────────────────────
--  SalesPOS drafts (and re-opened invoices) lost the per-line campaign discount:
--  SalesDetails only stored Discount (the raw value: a percentage for a % campaign,
--  a per-unit amount for a flat campaign). Without DiscountTypeId the POS could not
--  tell the two apart, and without HasDiscount it treated the line as an ordinary
--  line, so on reload the customer's membership discount overwrote the campaign.
--
--  Both columns are additive, NOT NULL with a DEFAULT so existing rows keep the
--  old behaviour (0 / 0 = "no campaign on this line").
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH('dbo.SalesDetails', 'DiscountTypeId') IS NULL
BEGIN
    ALTER TABLE dbo.SalesDetails
        ADD DiscountTypeId INT NOT NULL
            CONSTRAINT DF_SalesDetails_DiscountTypeId DEFAULT (0) WITH VALUES;
END

IF COL_LENGTH('dbo.SalesDetails', 'HasDiscount') IS NULL
BEGIN
    ALTER TABLE dbo.SalesDetails
        ADD HasDiscount BIT NOT NULL
            CONSTRAINT DF_SalesDetails_HasDiscount DEFAULT (0) WITH VALUES;
END

SELECT c.name, t.name AS DataType, c.is_nullable,
       ISNULL(dc.definition, N'') AS DefaultDefinition
FROM sys.columns c
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
WHERE c.object_id = OBJECT_ID('dbo.SalesDetails')
  AND c.name IN ('DiscountTypeId', 'HasDiscount');
