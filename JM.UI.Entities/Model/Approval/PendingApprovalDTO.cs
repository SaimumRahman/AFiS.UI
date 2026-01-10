using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Approval
{
    public class PendingApprovalDTO
    {
        public int PendingApprovalID { get; set; }
        public int EntityId { get; set; }
        public int WorkflowID { get; set; }
        public int CurrentLevel { get; set; }
        public int ApproverUserID { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool ReminderSent { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }

        // Navigation/Display Properties
        public string? WorkflowName { get; set; }
        public string? EntityDisplayName { get; set; } // e.g., Parcel number, request name, etc.
        public string? ApproverUserName { get; set; }
        public string? CurrentLevelName { get; set; }
        public string? Barcode { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
