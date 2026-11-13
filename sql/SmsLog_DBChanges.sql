SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF OBJECT_ID(N'dbo.SmsLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SmsLog (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SentDate      DATETIME2        NOT NULL CONSTRAINT DF_SmsLog_SentDate DEFAULT (GETDATE()),
        MobileNo      NVARCHAR(30)     NOT NULL,
        ReferenceNo   NVARCHAR(50)     NULL,
        StoreId       INT              NULL,
        BranchName    NVARCHAR(100)    NULL,
        SmsType       NVARCHAR(50)     NULL,
        MessageBody   NVARCHAR(MAX)    NULL,
        Status        NVARCHAR(30)     NOT NULL CONSTRAINT DF_SmsLog_Status DEFAULT (N'Failed'),
        ErrorDetails  NVARCHAR(500)    NULL,
        HttpStatusCode INT              NULL,
        SentBy        INT              NULL,
        SentByName    NVARCHAR(100)    NULL
    );
    CREATE INDEX IX_SmsLog_SentDate   ON dbo.SmsLog (SentDate DESC);
    CREATE INDEX IX_SmsLog_StoreId    ON dbo.SmsLog (StoreId);
    CREATE INDEX IX_SmsLog_MobileNo   ON dbo.SmsLog (MobileNo);
END

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/SmsLog')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive)
    VALUES (N'SMS Log', N'/SmsLog', 1);
END

INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId
FROM dbo.Routes r
WHERE r.RoutePath = N'/SmsLog'
  AND NOT EXISTS (
        SELECT 1 FROM dbo.GroupRoutePermissions x
        WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId
  );

SELECT RouteId, RouteName, RoutePath, IsActive
FROM dbo.Routes
WHERE RoutePath = N'/SmsLog';
GO