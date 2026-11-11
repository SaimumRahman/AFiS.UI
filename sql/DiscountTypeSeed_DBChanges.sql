SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ─────────────────────────────────────────────────────────────────────────────
--  DiscountType lookup was empty, so the campaign screens rendered both the
--  per-row and the bulk discount-type selectors with no options at all and every
--  line silently saved as DiscountTypeId = 1. The POS reads this id to decide
--  whether DiscountValue is a percentage (1) or a flat per-unit amount (2), so
--  the ids are part of the contract and must stay 1 and 2.
-- ─────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM dbo.DiscountType WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT dbo.DiscountType ON;
    INSERT INTO dbo.DiscountType (Id, TypeName) VALUES (1, N'Percentage');
    SET IDENTITY_INSERT dbo.DiscountType OFF;
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.DiscountType WHERE Id = 1 AND TypeName = N'Percentage')
BEGIN
    UPDATE dbo.DiscountType SET TypeName = N'Percentage' WHERE Id = 1;
END

IF NOT EXISTS (SELECT 1 FROM dbo.DiscountType WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT dbo.DiscountType ON;
    INSERT INTO dbo.DiscountType (Id, TypeName) VALUES (2, N'Flat');
    SET IDENTITY_INSERT dbo.DiscountType OFF;
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.DiscountType WHERE Id = 2 AND TypeName = N'Flat')
BEGIN
    UPDATE dbo.DiscountType SET TypeName = N'Flat' WHERE Id = 2;
END

SELECT Id, TypeName FROM dbo.DiscountType ORDER BY Id;
