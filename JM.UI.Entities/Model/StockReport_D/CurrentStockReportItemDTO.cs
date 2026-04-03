using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockReport_D
{
    public class CurrentStockReportItemDTO
    {
        public int SL { get; set; }
        public string? ProductCode { get; set; }
        public string? ItemName { get; set; }
        public string? Color { get; set; }
        public string? Features { get; set; }
        public string? UoM { get; set; }

        // Dynamic store quantities — key = store Code/Name
        public Dictionary<string, decimal> StoreQty { get; set; } = new();

        public decimal TotalQty { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalTk { get; set; }

        public string? GroupName { get; set; }
        public string? ProductType { get; set; }
    }
}
