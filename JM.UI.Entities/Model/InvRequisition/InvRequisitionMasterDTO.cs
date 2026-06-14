using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.InvRequisition
{
    public class InvRequisitionMasterDTO
    {
        public int RequisitionID { get; set; }
        public string RequisitionNo { get; set; } = string.Empty;
        public DateTime RequisitionDate { get; set; }
        public int? RequisitionTypeID { get; set; }
        public int? RequisitionBy { get; set; }
        public string? RequisitionByName { get; set; }
        public string? Remarks { get; set; }
        public int? ToStore { get; set; }
        public string? ToStoreName { get; set; }
        public int? FromStore { get; set; }
        public string? FromStoreName { get; set; }
        public bool IsUrgent { get; set; }
        public int? StatusID { get; set; }
        public string? StatusName { get; set; }
        public int? StatusBy { get; set; }
        public DateTime? StatusDate { get; set; }
        public bool IsComplete { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateOn { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
        public bool IsDeleted { get; set; }
        public string StatusComments { get; set; }
        public List<InvRequisitionDetailDTO> Details { get; set; } = new();
    }
}
