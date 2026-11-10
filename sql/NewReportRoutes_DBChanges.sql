/*
    New Report Routes — seed
    -------------------------
    Adds the two new report pages introduced recently:
      - Customer-wise Sales Report  (/CustomerSalesReport)
      - Item Statement              (/ItemStatement)

    Idempotent: safe to run more than once. Grants access to the admin group
    (4016) by default; adjust the grant block to mirror another report's groups
    if a wider rollout is wanted.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/CustomerSalesReport')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Customer-wise Sales Report', N'/CustomerSalesReport', 1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/ItemStatement')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Item Statement', N'/ItemStatement', 1);
END

-- Grant both routes to the admin group (4016), if not already granted.
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath IN (N'/CustomerSalesReport', N'/ItemStatement')
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId
  );

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath IN (N'/CustomerSalesReport', N'/ItemStatement')
ORDER BY RoutePath;
