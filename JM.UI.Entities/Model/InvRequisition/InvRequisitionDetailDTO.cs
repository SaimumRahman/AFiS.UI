using System;

namespace JM.UI.Entities.Model.InvRequisition
{
    public class InvRequisitionDetailDTO
    {
        public int RequisitionDetailID { get; set; }
        public int RequisitionID { get; set; }
        public int ItemID { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public decimal Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }
        public string? Remarks { get; set; }
        public bool IsComplete { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateOn { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
