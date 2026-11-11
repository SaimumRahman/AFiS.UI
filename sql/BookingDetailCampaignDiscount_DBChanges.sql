SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ─────────────────────────────────────────────────────────────────────────────
--  Same gap as SalesDetails: a booking only stored the raw Discount value, so a
--  booking reopened from the POS Bookings list looked like an ordinary line and
--  the customer's membership discount overwrote the campaign discount.
--  Both columns are additive, NOT NULL with a DEFAULT so existing rows keep the
--  old behaviour (0 / 0 = "no campaign on this line").
-- ─────────────────────────────────────────────────────────────────────────────

IF COL_LENGTH('dbo.BookingDetail', 'DiscountTypeId') IS NULL
BEGIN
    ALTER TABLE dbo.BookingDetail
        ADD DiscountTypeId INT NOT NULL
            CONSTRAINT DF_BookingDetail_DiscountTypeId DEFAULT (0) WITH VALUES;
END

IF COL_LENGTH('dbo.BookingDetail', 'HasDiscount') IS NULL
BEGIN
    ALTER TABLE dbo.BookingDetail
        ADD HasDiscount BIT NOT NULL
            CONSTRAINT DF_BookingDetail_HasDiscount DEFAULT (0) WITH VALUES;
END

SELECT c.name, t.name AS DataType, c.is_nullable,
       ISNULL(dc.definition, N'') AS DefaultDefinition
FROM sys.columns c
INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
WHERE c.object_id = OBJECT_ID('dbo.BookingDetail')
  AND c.name IN ('DiscountTypeId', 'HasDiscount');
