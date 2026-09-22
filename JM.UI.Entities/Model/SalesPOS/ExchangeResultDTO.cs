namespace JM.UI.Entities.Model.SalesPOS
{
    public class ExchangeResultDTO
    {
        public string InvoiceNo { get; set; } = "";
        public decimal ExchangeAmount { get; set; }
        public bool IsReturnExchange { get; set; } = true;
        public List<ExchangeItemDTO> ExchangeItems { get; set; } = new();
        /// <summary>
        /// Full cart-ready detail lines (copied from the original invoice) to be appended to the
        /// current sale's cart as exchange-return lines.
        /// </summary>
        public List<SaleDetailDTO> SaleLines { get; set; } = new();
    }

    public class ExchangeItemDTO
    {
        public int SalesDetailsId { get; set; }
        public decimal Qty { get; set; }
        public decimal Credit { get; set; }
    }
}
