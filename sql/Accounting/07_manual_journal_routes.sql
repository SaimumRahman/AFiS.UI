/*
    Manual Journal (JRN) — route seed
    ----------------------------------
    Adds the two client routes and mirrors the existing Opening Balance access so
    groups that can already open Opening Balance can also open Manual Journal.
    Idempotent: safe to run more than once.

    After running, use Group Route Permission to fine-tune which groups may post
    journals. Adjust the grant block below if a different starting point is wanted.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/ManualJournalList')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Manual Journal List', N'/ManualJournalList', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/ManualJournalEntry')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Manual Journal Entry', N'/ManualJournalEntry', 1);
END

-- Grant both routes to every group that can already open Opening Balance.
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT src.GroupId, r.RouteId
FROM dbo.GroupRoutePermissions src
JOIN dbo.Routes ob ON ob.RouteId = src.RouteId AND ob.RoutePath = N'/OpeningBalance'
CROSS JOIN dbo.Routes r
WHERE r.RoutePath IN (N'/ManualJournalList', N'/ManualJournalEntry')
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = src.GroupId AND x.RouteId = r.RouteId
  );

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath LIKE N'/ManualJournal%'
ORDER BY RoutePath;
