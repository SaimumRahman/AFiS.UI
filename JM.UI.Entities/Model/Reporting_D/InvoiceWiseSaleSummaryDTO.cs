using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class InvoiceWiseSaleSummaryDTO
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal DiscAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal DueAmount { get; set; }
    }
}
