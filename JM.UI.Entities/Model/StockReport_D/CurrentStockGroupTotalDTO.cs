using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockReport_D
{
    public class CurrentStockGroupTotalDTO
    {
        public string? GroupName { get; set; }
        public string? ProductType { get; set; }

        // Dynamic store totals — key = store Code/Name
        public Dictionary<string, decimal> StoreQty { get; set; } = new();

        public decimal TotalQty { get; set; }
        public decimal TotalTk { get; set; }
    }
}
