namespace JM.UI.Entities.Model.SalesPOS
{
    public class ExchangeResultDTO
    {
        public string InvoiceNo { get; set; } = "";
        public decimal ExchangeAmount { get; set; }
        public bool IsReturnExchange { get; set; } = true;
    }
}
