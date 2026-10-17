/* =========================================================================
   GroupRoutePermissions_Deduplicate_DBChanges.sql

   Removes duplicate dbo.GroupRoutePermissions rows, keeping the lowest Id per
   (GroupId, RouteId) pair.

   Why the duplicates exist:
     GroupRoutePermissionAdd.razor.cs never assigned ExistingRouteIds, so
     Save() computed:
         routesToAdd    = SelectedRouteIds.Except(ExistingRouteIds)  -> ALL selected
         routesToRemove = ExistingRouteIds.Except(SelectedRouteIds)  -> always empty
     Every save therefore re-inserted every already-checked route, while no
     uncheck was ever deleted. The code is fixed; this script cleans the data.

   Idempotent / safe to re-run.   Target: AFIS_MAH_LIVE
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

/* ---- 1. Duplicate pairs that will be trimmed (summary) ----------------- */
SELECT 'BEFORE' AS Phase,
       COUNT(*)                        AS TotalRows,
       COUNT(DISTINCT CAST(GroupId AS varchar(20)) + ':' + CAST(RouteId AS varchar(20))) AS DistinctPairs,
       COUNT(*) - COUNT(DISTINCT CAST(GroupId AS varchar(20)) + ':' + CAST(RouteId AS varchar(20))) AS RowsToDelete
FROM dbo.GroupRoutePermissions;

/* ---- 2. Trim duplicates, keeping the lowest Id ------------------------- */
DELETE grp
FROM dbo.GroupRoutePermissions grp
INNER JOIN (
    SELECT GroupId, RouteId, MIN(Id) AS KeepId
    FROM dbo.GroupRoutePermissions
    GROUP BY GroupId, RouteId
) k
    ON k.GroupId = grp.GroupId
   AND k.RouteId = grp.RouteId
WHERE grp.Id <> k.KeepId;

SELECT 'RowsDeleted' AS Phase, @@ROWCOUNT AS RowsDeleted;

/* ---- 3. Verify --------------------------------------------------------- */
SELECT 'AFTER_Totals' AS Phase,
       COUNT(*) AS TotalRows,
       COUNT(DISTINCT CAST(GroupId AS varchar(20)) + ':' + CAST(RouteId AS varchar(20))) AS DistinctPairs
FROM dbo.GroupRoutePermissions;

SELECT 'AFTER_Duplicates' AS Phase,
       GroupId,
       RouteId,
       COUNT(*) AS Copies
FROM dbo.GroupRoutePermissions
GROUP BY GroupId, RouteId
HAVING COUNT(*) > 1
ORDER BY GroupId, RouteId;
