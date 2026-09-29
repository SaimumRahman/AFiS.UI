namespace JM.UI.Entities.Model.SalesPOS
{
    public class ExchangeApplyResult
    {
        public string ReturnInvoiceNo { get; set; } = "";
        public decimal ExchangeAmount { get; set; }
        public List<ReturnedItemDTO> ReturnedItems { get; set; } = new();
    }
}