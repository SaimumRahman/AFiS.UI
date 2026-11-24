/* =========================================================================
   Treasury_Routes_DBChanges.sql
   -------------------------------------------------------------------------
   Adds the four new entry pages and grants them to every group that can
   already open Manual Journal (same access pattern as 07_manual_journal_routes.sql).
   Idempotent.
   ========================================================================= */
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/ContraEntry')
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'Contra Entry', N'/ContraEntry', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/PosSettlement')
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'POS / MFS Settlement', N'/PosSettlement', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/CashSwap')
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'Cash Swap', N'/CashSwap', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/BranchLoan')
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'Branch Loan', N'/BranchLoan', 1);

-- Grant to every group that can already open Manual Journal.
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT g.GroupId, r.RouteId
FROM (SELECT DISTINCT src.GroupId
      FROM dbo.GroupRoutePermissions src
      JOIN dbo.Routes mj ON mj.RouteId = src.RouteId AND mj.RoutePath = N'/ManualJournalList') g
CROSS JOIN dbo.Routes r
WHERE r.RoutePath IN (N'/ContraEntry', N'/PosSettlement', N'/CashSwap', N'/BranchLoan')
  AND NOT EXISTS (
        SELECT 1 FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = g.GroupId AND x.RouteId = r.RouteId
  );

-- Remove any accidental duplicate grants (one row per group per route).
;WITH cte AS (
    SELECT GroupId, RouteId,
           ROW_NUMBER() OVER (PARTITION BY GroupId, RouteId ORDER BY (SELECT 0)) AS rn
    FROM dbo.GroupRoutePermissions
)
DELETE FROM cte WHERE rn > 1;

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath IN (N'/ContraEntry', N'/PosSettlement', N'/CashSwap', N'/BranchLoan')
ORDER BY RoutePath;
GO
PRINT 'Treasury_Routes_DBChanges.sql complete.';
GO
