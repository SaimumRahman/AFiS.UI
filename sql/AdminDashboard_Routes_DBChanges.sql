/* =========================================================================
   AdminDashboard_Routes_DBChanges.sql
   -------------------------------------------------------------------------
   Route + permission grant for the Admin MD Dashboard. The page is for the
   Managing Director / Admin only, i.e. any group with GroupsRole.IsSystem = 1.
   Idempotent.
   ========================================================================= */
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/AdminDashboard')
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'Admin MD Dashboard', N'/AdminDashboard', 1);

INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT gr.GroupId, r.RouteId
FROM dbo.GroupsRole gr
CROSS JOIN dbo.Routes r
WHERE r.RoutePath = N'/AdminDashboard'
  AND ISNULL(gr.IsSystem, 0) = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x
                  WHERE x.GroupId = gr.GroupId AND x.RouteId = r.RouteId);

SELECT r.RouteId, r.RouteName, r.RoutePath, gr.GroupId, gr.GroupName
FROM dbo.Routes r
JOIN dbo.GroupRoutePermissions gp ON gp.RouteId = r.RouteId
JOIN dbo.GroupsRole gr ON gr.GroupId = gp.GroupId
WHERE r.RoutePath = N'/AdminDashboard';
GO
PRINT 'AdminDashboard_Routes_DBChanges.sql complete.';
GO
