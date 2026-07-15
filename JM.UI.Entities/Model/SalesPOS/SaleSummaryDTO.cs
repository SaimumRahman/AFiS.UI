using System;

namespace JM.UI.Entities.Model.SalesPOS
{
    public class SaleSummaryDTO
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime SalesDate { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? SalesType { get; set; }
        public string? SalesPersonName { get; set; }
        public int TotalItems { get; set; }
    }
}
