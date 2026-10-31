namespace JM.UI.Entities.Model.SalesPOS
{
    public class SalesReturnRequestDTO
    {
        public string ReturnInvoiceNo { get; set; } = "";
        public int StoreId { get; set; }
        public int CreatedBy { get; set; }
        public int? CustomerId { get; set; }
        public List<ReturnLineDTO> ReturnItems { get; set; } = new();
        public List<SaleDetailDTO> ExchangeItems { get; set; } = new();
        public decimal? VatAmount { get; set; }
        public int RefundMethodId { get; set; }
        public int PaymentMethodId { get; set; }
        public string? RefundTransactionNo { get; set; }
        public string? PaymentTransactionNo { get; set; }
    }

    public class ReturnLineDTO
    {
        public int SalesDetailsId { get; set; }
        public int ItemId { get; set; }
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal Discount { get; set; }
        public decimal Credit { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
