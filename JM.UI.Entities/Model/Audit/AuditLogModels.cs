using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Audit
{
    public class AuditLogDTO
    {
        public long AuditId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public char Op { get; set; }
        public long? RowId { get; set; }
        public string? BusinessKey { get; set; }
        public int? StoreId { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public int? ChangedBy { get; set; }
        public string? ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? AppUser { get; set; }
        public string? HostName { get; set; }
    }

    public class AuditLogPageResult
    {
        public List<AuditLogDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
