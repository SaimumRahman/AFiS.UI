SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- ═══════════════════════════════════════════════════════════════════════════
--  To-Do List — per-user read tracking
--
--  A row here means "this user has opened this task". It drives:
--    • the NEW badge (a task with no read row for the caller is NEW), and
--    • the "Read by …" list shown inside the task dialog.
--  Nothing is ever deleted from this table.
-- ═══════════════════════════════════════════════════════════════════════════

IF OBJECT_ID('dbo.TodoTaskRead','U') IS NULL
BEGIN
    CREATE TABLE dbo.TodoTaskRead (
        Id        int IDENTITY(1,1) PRIMARY KEY,
        TaskId    int          NOT NULL,
        UserId    int          NOT NULL,
        ReadAtUtc datetime2(0) NOT NULL CONSTRAINT DF_TodoTaskRead_ReadAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_TodoTaskRead_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTask(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TodoTaskRead_User FOREIGN KEY (UserId) REFERENCES dbo.core_users(UserId)
    );
    CREATE UNIQUE INDEX UX_TodoTaskRead_TaskUser ON dbo.TodoTaskRead(TaskId, UserId);
    CREATE INDEX IX_TodoTaskRead_User ON dbo.TodoTaskRead(UserId, TaskId);
END
GO

SELECT COUNT(1) AS TodoTaskReadRows FROM dbo.TodoTaskRead;
GO
