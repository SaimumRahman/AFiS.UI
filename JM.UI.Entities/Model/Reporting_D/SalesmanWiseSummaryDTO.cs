using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SalesmanWiseSummaryDTO
    {
        public string SalesmanName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string ProductSold { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal NetPaid { get; set; }
    }
}
