using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class BookingReportDTO
    {
        public string BookingId { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueBalance { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}