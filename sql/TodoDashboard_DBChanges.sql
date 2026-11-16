SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- core_users is an Identity heap (PK is the nvarchar Id column), so the numeric
-- UserId has no unique key to be referenced by a foreign key. Add one so the
-- To-Do tables can hold proper relations to the owning user.
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('dbo.core_users') AND type = 'UQ')
BEGIN
    ALTER TABLE dbo.core_users ADD CONSTRAINT UQ_core_users_UserId UNIQUE (UserId);
END
GO

IF OBJECT_ID('dbo.TodoTask','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTask (
        Id                 int IDENTITY(1,1) PRIMARY KEY,
        Title              nvarchar(200)  NOT NULL,
        Details            nvarchar(max)  NULL,
        Flag               tinyint        NOT NULL CONSTRAINT DF_TodoTask_Flag DEFAULT (1),
        CreatedBy          int            NOT NULL,
        CreatedAtUtc       datetime2(0)   NOT NULL CONSTRAINT DF_TodoTask_CreatedAt DEFAULT (SYSUTCDATETIME()),
        DueAtUtc           datetime2(0)   NULL,
        RepeatType         tinyint        NOT NULL CONSTRAINT DF_TodoTask_RepeatType DEFAULT (0),
        RepeatInterval     int            NOT NULL CONSTRAINT DF_TodoTask_RepeatInterval DEFAULT (1),
        RepeatUnit         tinyint        NOT NULL CONSTRAINT DF_TodoTask_RepeatUnit DEFAULT (0),
        Status             tinyint        NOT NULL CONSTRAINT DF_TodoTask_Status DEFAULT (0),
        CompletedBy        int            NULL,
        CompletedAtUtc     datetime2(0)   NULL,
        IsPrivate          bit            NOT NULL CONSTRAINT DF_TodoTask_IsPrivate DEFAULT (0),
        AllBranches        bit            NOT NULL CONSTRAINT DF_TodoTask_AllBranches DEFAULT (0),
        Version            int            NOT NULL CONSTRAINT DF_TodoTask_Version DEFAULT (1),
        IsDeleted          bit            NOT NULL CONSTRAINT DF_TodoTask_IsDeleted DEFAULT (0),
        ModifiedBy         int            NULL,
        ModifiedAtUtc      datetime2(0)   NULL,
        CONSTRAINT FK_TodoTask_CreatedBy   FOREIGN KEY (CreatedBy)   REFERENCES dbo.core_users(UserId),
        CONSTRAINT FK_TodoTask_CompletedBy FOREIGN KEY (CompletedBy) REFERENCES dbo.core_users(UserId)
    );
    CREATE INDEX IX_TodoTask_CreatedBy  ON dbo.TodoTask(CreatedBy);
    CREATE INDEX IX_TodoTask_Status_Due ON dbo.TodoTask(Status, DueAtUtc);
    CREATE INDEX IX_TodoTask_Due        ON dbo.TodoTask(DueAtUtc);
    CREATE INDEX IX_TodoTask_IsDeleted  ON dbo.TodoTask(IsDeleted, Status);
END
GO

IF OBJECT_ID('dbo.TodoTaskBranch','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskBranch (
        Id      int IDENTITY(1,1) PRIMARY KEY,
        TaskId  int NOT NULL,
        StoreId int NOT NULL,
        CONSTRAINT FK_TodoTaskBranch_Task  FOREIGN KEY (TaskId)  REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskBranch_Store FOREIGN KEY (StoreId) REFERENCES dbo.Stores(Id)
    );
    CREATE UNIQUE INDEX UX_TodoTaskBranch_TaskStore ON dbo.TodoTaskBranch(TaskId, StoreId);
    CREATE INDEX IX_TodoTaskBranch_Store ON dbo.TodoTaskBranch(StoreId, TaskId);
END
GO

IF OBJECT_ID('dbo.TodoTaskRole','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskRole (
        Id      int IDENTITY(1,1) PRIMARY KEY,
        TaskId  int NOT NULL,
        GroupId int NOT NULL,
        CONSTRAINT FK_TodoTaskRole_Task  FOREIGN KEY (TaskId)  REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskRole_Group FOREIGN KEY (GroupId) REFERENCES dbo.GroupsRole(GroupId)
    );
    CREATE UNIQUE INDEX UX_TodoTaskRole_TaskGroup ON dbo.TodoTaskRole(TaskId, GroupId);
    CREATE INDEX IX_TodoTaskRole_Group ON dbo.TodoTaskRole(GroupId, TaskId);
END
GO

IF OBJECT_ID('dbo.TodoTaskDesignation','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskDesignation (
        Id            int IDENTITY(1,1) PRIMARY KEY,
        TaskId        int NOT NULL,
        DesignationId int NOT NULL,
        CONSTRAINT FK_TodoTaskDesig_Task FOREIGN KEY (TaskId)        REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskDesig_Des  FOREIGN KEY (DesignationId) REFERENCES dbo.Designations(Id)
    );
    CREATE UNIQUE INDEX UX_TodoTaskDesig_TaskDes ON dbo.TodoTaskDesignation(TaskId, DesignationId);
    CREATE INDEX IX_TodoTaskDesig_Des ON dbo.TodoTaskDesignation(DesignationId, TaskId);
END
GO

IF OBJECT_ID('dbo.TodoTaskUser','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskUser (
        Id     int IDENTITY(1,1) PRIMARY KEY,
        TaskId int NOT NULL,
        UserId int NOT NULL,
        CONSTRAINT FK_TodoTaskUser_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskUser_User FOREIGN KEY (UserId) REFERENCES dbo.core_users(UserId)
    );
    CREATE UNIQUE INDEX UX_TodoTaskUser_TaskUser ON dbo.TodoTaskUser(TaskId, UserId);
    CREATE INDEX IX_TodoTaskUser_User ON dbo.TodoTaskUser(UserId, TaskId);
END
GO

IF OBJECT_ID('dbo.TodoTaskUpdate','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskUpdate (
        Id           int IDENTITY(1,1) PRIMARY KEY,
        TaskId       int            NOT NULL,
        UserId       int            NOT NULL,
        CreatedAtUtc datetime2(0)   NOT NULL CONSTRAINT DF_TodoTaskUpdate_CreatedAt DEFAULT (SYSUTCDATETIME()),
        Remarks      nvarchar(1000) NOT NULL,
        UpdateType   tinyint        NOT NULL,
        CONSTRAINT FK_TodoTaskUpdate_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskUpdate_User FOREIGN KEY (UserId) REFERENCES dbo.core_users(UserId)
    );
    CREATE INDEX IX_TodoTaskUpdate_Task ON dbo.TodoTaskUpdate(TaskId, CreatedAtUtc);
END
GO

IF OBJECT_ID('dbo.TodoTaskEditLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskEditLog (
        Id           int IDENTITY(1,1) PRIMARY KEY,
        TaskId       int              NOT NULL,
        UserId       int              NOT NULL,
        CreatedAtUtc datetime2(0)     NOT NULL CONSTRAINT DF_TodoTaskEditLog_CreatedAt DEFAULT (SYSUTCDATETIME()),
        EditBatchId  uniqueidentifier NOT NULL,
        FieldName    nvarchar(50)     NOT NULL,
        OldValue     nvarchar(400)    NULL,
        NewValue     nvarchar(400)    NULL,
        CONSTRAINT FK_TodoTaskEditLog_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskEditLog_User FOREIGN KEY (UserId) REFERENCES dbo.core_users(UserId)
    );
    CREATE INDEX IX_TodoTaskEditLog_Task ON dbo.TodoTaskEditLog(TaskId, CreatedAtUtc);
END
GO

IF OBJECT_ID('dbo.TodoActivityLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoActivityLog (
        Id                int IDENTITY(1,1) PRIMARY KEY,
        UserId            int           NOT NULL,
        CreatedAtUtc      datetime2(0)  NOT NULL CONSTRAINT DF_TodoActivityLog_CreatedAt DEFAULT (SYSUTCDATETIME()),
        Action            nvarchar(40)  NOT NULL,
        TaskId            int           NULL,
        TaskTitleSnapshot nvarchar(200) NULL,
        Detail            nvarchar(400) NULL,
        CONSTRAINT FK_TodoActivityLog_User FOREIGN KEY (UserId) REFERENCES dbo.core_users(UserId)
    );
    CREATE INDEX IX_TodoActivityLog_User ON dbo.TodoActivityLog(UserId, CreatedAtUtc);
    CREATE INDEX IX_TodoActivityLog_CreatedAt ON dbo.TodoActivityLog(CreatedAtUtc);
END
GO

IF OBJECT_ID('dbo.TodoDeletedTask','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoDeletedTask (
        Id              int IDENTITY(1,1) PRIMARY KEY,
        OriginalTaskId  int           NOT NULL,
        Title           nvarchar(200) NOT NULL,
        Details         nvarchar(max) NULL,
        Flag            tinyint       NOT NULL,
        CreatedBy       int           NOT NULL,
        CreatedAtUtc    datetime2(0)  NOT NULL,
        DueAtUtc        datetime2(0)  NULL,
        RepeatType      tinyint       NOT NULL,
        RepeatInterval  int           NOT NULL,
        RepeatUnit      tinyint       NOT NULL,
        Status          tinyint       NOT NULL,
        CompletedBy     int           NULL,
        CompletedAtUtc  datetime2(0)  NULL,
        IsPrivate       bit           NOT NULL,
        AllBranches     bit           NOT NULL,
        Version         int           NOT NULL,
        SnapshotJson    nvarchar(max) NULL,
        DeletedBy       int           NOT NULL,
        DeletedAtUtc    datetime2(0)  NOT NULL CONSTRAINT DF_TodoDeletedTask_DeletedAt DEFAULT (SYSUTCDATETIME()),
        IsRestored      bit           NOT NULL CONSTRAINT DF_TodoDeletedTask_IsRestored DEFAULT (0),
        RestoredBy      int           NULL,
        RestoredAtUtc   datetime2(0)  NULL,
        CONSTRAINT FK_TodoDeletedTask_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.core_users(UserId),
        CONSTRAINT FK_TodoDeletedTask_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES dbo.core_users(UserId)
    );
    CREATE INDEX IX_TodoDeletedTask_DeletedAt ON dbo.TodoDeletedTask(DeletedAtUtc);
    CREATE INDEX IX_TodoDeletedTask_Restored  ON dbo.TodoDeletedTask(IsRestored);
END
GO

IF OBJECT_ID('dbo.TodoDeletedTaskAudience','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoDeletedTaskAudience (
        Id            int IDENTITY(1,1) PRIMARY KEY,
        DeletedTaskId int           NOT NULL,
        AudienceType  tinyint       NOT NULL,
        RefId         int           NOT NULL,
        RefName       nvarchar(200) NULL,
        CONSTRAINT FK_TodoDelAud_DeletedTask FOREIGN KEY (DeletedTaskId) REFERENCES dbo.TodoDeletedTask(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_TodoDelAud_DeletedTask ON dbo.TodoDeletedTaskAudience(DeletedTaskId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Routes WHERE RoutePath = N'/TodoDashboard')
BEGIN
    INSERT dbo.Routes (RouteName, RoutePath, IsActive) VALUES (N'To-Do List', N'/TodoDashboard', 1);
END
GO

INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT 4016, r.RouteId FROM dbo.Routes r
WHERE r.RoutePath = N'/TodoDashboard'
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x WHERE x.GroupId = 4016 AND x.RouteId = r.RouteId);
GO

-- Everyone uses the To-Do list (they can be given tasks and post updates), so the
-- page is granted to every role group, not only to Admin.
INSERT dbo.GroupRoutePermissions (GroupId, RouteId)
SELECT gr.GroupId, r.RouteId
FROM dbo.GroupsRole gr
CROSS JOIN dbo.Routes r
WHERE r.RoutePath = N'/TodoDashboard'
  AND NOT EXISTS (SELECT 1 FROM dbo.GroupRoutePermissions x WHERE x.GroupId = gr.GroupId AND x.RouteId = r.RouteId);
GO

SELECT RouteId, RouteName, RoutePath FROM dbo.Routes WHERE RoutePath = N'/TodoDashboard';
GO
