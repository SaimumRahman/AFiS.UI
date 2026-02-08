namespace JM.UI.Entities.Model.SupplierPayments
{
    public class SupplierOutstandingDTO
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierContactNo { get; set; }
        public string? SupplierAddress { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal OutstandingBalance { get; set; }
    }
}
