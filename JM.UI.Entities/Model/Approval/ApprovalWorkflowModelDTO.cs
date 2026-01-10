using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Approval
{
    public class ApprovalWorkflowModelDTO
    {
        public int Id { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string? WorkflowDescription { get; set; }
        public string? EntityType { get; set; }
        public int TotalLevels { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? LastModifiedBy { get; set; }
    }
}
