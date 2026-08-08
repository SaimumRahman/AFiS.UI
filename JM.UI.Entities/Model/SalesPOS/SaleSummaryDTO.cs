using System;

namespace JM.UI.Entities.Model.SalesPOS
{
    public class SaleSummaryDTO
    {
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public decimal TotalBill { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal? TotalVat { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsBooking { get; set; }
        public bool? IsDraft { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalItems { get; set; }
        public int? StoreId { get; set; }
    }
}