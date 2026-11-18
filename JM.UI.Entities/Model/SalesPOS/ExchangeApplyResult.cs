namespace JM.UI.Entities.Model.SalesPOS
{
    public class ExchangeApplyResult
    {
        public string ReturnInvoiceNo { get; set; } = "";
        public decimal ExchangeAmount { get; set; }
        /// <summary>Customer on the invoice being exchanged (0 when the invoice has none).</summary>
        public int InvoiceCustomerId { get; set; }
        public string? InvoiceCustomerName { get; set; }
        public List<ReturnedItemDTO> ReturnedItems { get; set; } = new();
    }
}