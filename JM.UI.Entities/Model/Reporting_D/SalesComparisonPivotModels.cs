using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SalesComparisonStoreGroup
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
    }

    public class SalesComparisonCell
    {
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal Net { get; set; }
    }

    public class SalesComparisonPivotRow
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;

        public Dictionary<string, SalesComparisonCell> Cells { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public decimal CombinedTotal { get; set; }
        public decimal CombinedDiscount { get; set; }
        public decimal CombinedNet { get; set; }

        public SalesComparisonCell GetCell(string? storeName)
        {
            var key = (storeName ?? string.Empty).Trim();
            return Cells.TryGetValue(key, out var cell) ? cell : new SalesComparisonCell();
        }
    }
}
