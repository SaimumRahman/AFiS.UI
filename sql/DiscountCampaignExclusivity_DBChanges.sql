/* =====================================================================
   Discount campaign exclusivity
   ---------------------------------------------------------------------
   An item may be in at most ONE active campaign per store, where "at the
   same time" means the campaigns' date ranges share at least one day.

   - Conflict key is (ItemId, StoreId).
   - Only IsActive = 1 campaigns block; inactive ones never do.
   - Dates are compared at DAY precision (a campaign covers its end date),
     which is how the stock ledger already reads them.
   - Enforcement lives in the save transaction
     (InsertUpdateDiscountManagerHandler -> DiscountManagerRepo).
     This script only adds the supporting indexes and reports any overlap
     that already exists. It never deletes data.
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------------ */
/* 1. Lookup index for the conflict check (also used by the POS).      */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_DiscountManagerDetails_Item_Store'
      AND object_id = OBJECT_ID(N'dbo.DiscountManagerDetails')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_DiscountManagerDetails_Item_Store
        ON dbo.DiscountManagerDetails (ItemId, StoreId)
        INCLUDE (DiscountManagerId, DiscountValue, DiscountTypeId, CurrentSalePrice);
END
GO

/* ------------------------------------------------------------------ */
/* 2. Index on the campaign validity window.                           */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_DiscountManager_Dates'
      AND object_id = OBJECT_ID(N'dbo.DiscountManager')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_DiscountManager_Dates
        ON dbo.DiscountManager (IsActive, StartDate, EndDate);
END
GO

/* ------------------------------------------------------------------ */
/* 3. Report existing overlaps (read-only). Each row is a product that */
/*    sits in two active campaigns whose dates share a day, for the    */
/*    same store. Expected: no rows.                                    */
/* ------------------------------------------------------------------ */
SELECT
      dmd.ItemId
    , i.Name                                                  AS ItemName
    , dmd.StoreId
    , s.Name                                                  AS StoreName
    , dm.Id                                                   AS CampaignId
    , dm.DiscountName
    , CONVERT(varchar(10), dm.StartDate, 120)                 AS StartDate
    , CONVERT(varchar(10), dm.EndDate,   120)                 AS EndDate
    , dmd.DiscountValue
    , dmd.DiscountTypeId
FROM dbo.DiscountManagerDetails dmd
INNER JOIN dbo.DiscountManager dm ON dm.Id = dmd.DiscountManagerId
LEFT  JOIN dbo.Items            i  ON i.Id  = dmd.ItemId
LEFT  JOIN dbo.Stores           s  ON s.Id  = dmd.StoreId
WHERE dm.IsActive = 1
  AND EXISTS (
        SELECT 1
        FROM dbo.DiscountManagerDetails d2
        INNER JOIN dbo.DiscountManager m2 ON m2.Id = d2.DiscountManagerId
        WHERE d2.ItemId = dmd.ItemId
          AND ISNULL(d2.StoreId, 0) = ISNULL(dmd.StoreId, 0)
          AND d2.DiscountManagerId <> dmd.DiscountManagerId
          AND m2.IsActive = 1
          AND CAST(m2.StartDate AS date) <= CAST(dm.EndDate AS date)
          AND CAST(m2.EndDate   AS date) >= CAST(dm.StartDate AS date)
  )
ORDER BY dmd.ItemId, dmd.StoreId, dm.StartDate;
GO
