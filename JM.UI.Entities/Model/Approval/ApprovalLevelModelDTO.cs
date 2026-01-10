using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Approval
{
    public class ApprovalLevelModelDTO
    {
        public int Id { get; set; }
        public int WorkflowID { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? LevelDescription { get; set; }
        public bool IsParallelApproval { get; set; } = false;
        public int RequiredApprovers { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }

        // Navigation properties
        public string? WorkflowName { get; set; }
    }
}
