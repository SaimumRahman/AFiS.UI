/* =========================================================================
   StockMaster_SalesRate_Repair_DBChanges.sql

   Fixes dbo.StockMaster rows whose SalesRate is 0/NULL even though the item
   has a valid global selling price.

   Why these rows exist:
     - The Stock-Out insert path hardcodes SalesRate = 0
       (StockRepository.cs, "@CurrentRate, 0, 0").
     - The transfer receive path uses "detail.UnitPrice ?? 0".
     - The stock-in insert uses "request.SalesRate ?? 0" (0 when not supplied).
     - The stock-out UPDATE path never touches SalesRate, and the stock-in
       UPDATE path only accepts a POSITIVE value, so an existing 0 is never
       repaired by later movements.

   Effect: the SalesPOS Product Search dialog reads sm.SalesRate as the price
   per store, so the store whose row kept 0 showed a price of 0.

   Repair rule: where a store's SalesRate is 0/NULL, set it to the item's
   global Items.SalePrice.

   Idempotent / safe to re-run.   Target: AFIS_MAH_LIVE
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

/* ---- 1. Preview the rows that will change ------------------------------ */
SELECT 'BEFORE' AS Phase,
       sm.StockID,
       sm.ItemID,
       i.Barcode,
       i.Name       AS ItemName,
       sm.StoreId,
       s.Name       AS StoreName,
       sm.SalesRate AS OldSalesRate,
       i.SalePrice  AS NewSalesRate
FROM dbo.StockMaster sm
INNER JOIN dbo.Items  i ON i.Id = sm.ItemID
LEFT  JOIN dbo.Stores s ON s.Id = sm.StoreId
WHERE sm.IsDeleted = 0
  AND ISNULL(sm.SalesRate, 0) = 0
  AND ISNULL(i.SalePrice, 0) > 0
ORDER BY sm.ItemID, sm.StoreId;

/* ---- 2. Repair --------------------------------------------------------- */
UPDATE sm
SET sm.SalesRate = i.SalePrice
FROM dbo.StockMaster sm
INNER JOIN dbo.Items i ON i.Id = sm.ItemID
WHERE sm.IsDeleted = 0
  AND ISNULL(sm.SalesRate, 0) = 0
  AND ISNULL(i.SalePrice, 0) > 0;

SELECT 'RowsUpdated' AS Phase, @@ROWCOUNT AS RowsUpdated;

/* ---- 3. Verify --------------------------------------------------------- */
SELECT 'AFTER' AS Phase,
       sm.ItemID,
       i.Barcode,
       sm.StoreId,
       s.Name     AS StoreName,
       sm.SalesRate,
       i.SalePrice
FROM dbo.StockMaster sm
INNER JOIN dbo.Items  i ON i.Id = sm.ItemID
LEFT  JOIN dbo.Stores s ON s.Id = sm.StoreId
WHERE sm.IsDeleted = 0
ORDER BY sm.ItemID, sm.StoreId;
