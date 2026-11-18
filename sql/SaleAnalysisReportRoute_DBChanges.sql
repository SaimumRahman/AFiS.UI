SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/SaleAnalysisReport')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Sale Analysis Report', N'/SaleAnalysisReport', 1);
END

INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/SaleAnalysisReport'
  AND NOT EXISTS (
        SELECT 1 FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId
  );

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath = N'/SaleAnalysisReport';