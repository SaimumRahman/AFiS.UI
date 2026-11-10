using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SalesComparisonDTO
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;

        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        public decimal TotalSales { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetSales { get; set; }
    }
}
