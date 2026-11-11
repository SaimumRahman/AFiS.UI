SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/SalesBookingReport')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Sales & Booking Report', N'/SalesBookingReport', 1);
END

INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/SalesBookingReport'
  AND NOT EXISTS (
        SELECT 1 FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId
  );

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath = N'/SalesBookingReport';
