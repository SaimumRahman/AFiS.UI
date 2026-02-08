namespace JM.UI.Entities.Model.SupplierPayments
{
    public class SupplierPaymentDetailsDTO
    {
        public int Id { get; set; }
        public int SupplierPaymentId { get; set; }
        public int PurchaseId { get; set; }
        public decimal AllocatedAmount { get; set; }
        
        // UI Helpers
        public string? PurchaseInvoiceNo { get; set; }
    }
}
