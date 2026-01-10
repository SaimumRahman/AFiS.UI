using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Approval
{
    public class ApprovalLevelApproverModelDTO
    {
        public int Id { get; set; }
        public int ApprovalLevelID { get; set; }
        public int UserID { get; set; } 
        public int ApproverOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime? AssignedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? LastModifiedBy { get; set; }

        // Navigation/Display Properties
        public string? ApprovalLevelName { get; set; }
        public string? WorkflowName { get; set; }
        public string? UserName { get; set; }
    }
}
