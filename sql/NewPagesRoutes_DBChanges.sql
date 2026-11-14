SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ============================================================
-- Routes for newly added pages
-- ============================================================

-- SMS Settings
IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/SmsSettings')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'SMS Settings', N'/SmsSettings', 1);
END
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/SmsSettings'
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId);

-- SMS Log
IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/SmsLog')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'SMS Log', N'/SmsLog', 1);
END
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/SmsLog'
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId);

-- Audit Log
IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/AuditLog')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'Audit Log', N'/AuditLog', 1);
END
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/AuditLog'
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId);

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath IN (N'/SmsSettings', N'/SmsLog')
ORDER BY RouteId;
GO