using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Todo
{
    public class TodoCurrentUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public int? StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public int? DesignationId { get; set; }
        public string DesignationName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool CanTagBranches { get; set; }
        public List<int> GroupIds { get; set; } = new();
    }

    public class TodoLookupItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extra { get; set; } = string.Empty;
    }

    public class TodoLookupsDTO
    {
        public TodoCurrentUserDTO Me { get; set; } = new();
        public List<TodoLookupItemDTO> Stores { get; set; } = new();
        public List<TodoLookupItemDTO> Groups { get; set; } = new();
        public List<TodoLookupItemDTO> Designations { get; set; } = new();
        public List<TodoLookupItemDTO> Users { get; set; } = new();
        public List<TodoLookupItemDTO> Flags { get; set; } = new();
    }

    public class TodoTaskListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int Flag { get; set; }
        public string FlagName { get; set; } = "General";
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DueAtUtc { get; set; }
        public int RepeatType { get; set; }
        public int RepeatInterval { get; set; }
        public int RepeatUnit { get; set; }
        public int Status { get; set; }
        public int? CompletedBy { get; set; }
        public string CompletedByName { get; set; } = string.Empty;
        public DateTime? CompletedAtUtc { get; set; }
        public bool IsPrivate { get; set; }
        public bool AllBranches { get; set; }
        public int Version { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsDueToday { get; set; }
        public int UpdateCount { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSeeEditHistory { get; set; }

        // Bangladesh wall-clock values computed by the API payload (the server
        // always sends the *AtLocal fields), with a local fallback so a missing
        // field never renders as year 1.
        public DateTime CreatedAtLocal { get; set; }
        public DateTime? DueAtLocal { get; set; }
        public DateTime? CompletedAtLocal { get; set; }
        public string RepeatText { get; set; } = string.Empty;
        public bool IsRepeating => RepeatType != 0;

        public List<string> BranchNames { get; set; } = new();
        public List<string> GroupNames { get; set; } = new();
        public List<string> DesignationNames { get; set; } = new();
        public List<string> UserNames { get; set; } = new();

        public List<int> BranchIds { get; set; } = new();
        public List<int> GroupIds { get; set; } = new();
        public List<int> DesignationIds { get; set; } = new();
        public List<int> UserIds { get; set; } = new();
    }

    public class TodoCountsDTO
    {
        public int Open { get; set; }
        public int Overdue { get; set; }
        public int DueToday { get; set; }
        public int Completed { get; set; }
        public int Total { get; set; }
    }

    public class TodoDashboardDTO
    {
        public List<TodoTaskListItemDTO> Items { get; set; } = new();
        public TodoCountsDTO Counts { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class TodoTaskUpdateDTO
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime CreatedAtLocal { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public int UpdateType { get; set; }
        public string UpdateTypeName { get; set; } = string.Empty;
    }

    public class TodoEditLogDTO
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime CreatedAtLocal { get; set; }
        public string EditBatchId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class TodoTaskDetailDTO
    {
        public TodoTaskListItemDTO Task { get; set; } = new();
        public List<TodoTaskUpdateDTO> Updates { get; set; } = new();
        public List<TodoEditLogDTO> EditHistory { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanSeeEditHistory { get; set; }
    }

    public class TodoActivityLogDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime CreatedAtLocal { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }

    public class TodoDeletedTaskDTO
    {
        public int Id { get; set; }
        public int OriginalTaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int Flag { get; set; }
        public string FlagName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DueAtUtc { get; set; }
        public int Status { get; set; }
        public bool IsPrivate { get; set; }
        public bool AllBranches { get; set; }
        public int DeletedBy { get; set; }
        public string DeletedByName { get; set; } = string.Empty;
        public DateTime DeletedAtUtc { get; set; }
        public bool IsRestored { get; set; }
        public int? RestoredBy { get; set; }
        public DateTime? RestoredAtUtc { get; set; }
        public string AudienceText { get; set; } = string.Empty;
        public string SnapshotJson { get; set; } = string.Empty;

        public DateTime CreatedAtLocal { get; set; }
        public DateTime? DueAtLocal { get; set; }
        public DateTime DeletedAtLocal { get; set; }
        public DateTime? RestoredAtLocal { get; set; }
    }

    public class TodoTaskSaveDTO
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int Flag { get; set; } = 1;

        /// <summary>Due date/time entered by the user, in Bangladesh wall-clock time.</summary>
        public DateTime? DueAtLocal { get; set; }

        public int RepeatType { get; set; }
        public int RepeatInterval { get; set; } = 1;
        public int RepeatUnit { get; set; }
        public bool IsPrivate { get; set; }
        public bool AllBranches { get; set; }
        public List<int> BranchIds { get; set; } = new();
        public List<int> GroupIds { get; set; } = new();
        public List<int> DesignationIds { get; set; } = new();
        public List<int> UserIds { get; set; } = new();
    }

    public class TodoPostUpdateDTO
    {
        public int TaskId { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class TodoResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public bool Conflict { get; set; }
        public bool Forbidden { get; set; }
        public DateTime? NextDueAtUtc { get; set; }
        public DateTime? NextDueAtLocal { get; set; }
    }

    public class TodoPagedDTO<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }
}
