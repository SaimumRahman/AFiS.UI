namespace JM.UI.Entities.Model.InvRequisition
{
    public class InvRequisitionPreviewRow
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
    }
}
