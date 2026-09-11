namespace JM.UI.Entities.Model.SalesPOS
{
    public class ExchangeResultDTO
    {
        public string InvoiceNo { get; set; } = "";
        public decimal ExchangeAmount { get; set; }
        public bool IsReturnExchange { get; set; } = true;
        public List<ExchangeItemDTO> ExchangeItems { get; set; } = new();
    }

    public class ExchangeItemDTO
    {
        public int SalesDetailsId { get; set; }
        public decimal Qty { get; set; }
        public decimal Credit { get; set; }
    }
}
