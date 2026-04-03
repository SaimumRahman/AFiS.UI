using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockReport_D
{
    public class CurrentStockReportGroupDTO
    {
        public string? GroupName { get; set; }
        public string? ProductType { get; set; }
        public List<CurrentStockReportItemDTO> Items { get; set; } = new();
        public CurrentStockGroupTotalDTO GroupTotal { get; set; } = new();
    }
}
