using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockReport_D
{
    public class CurrentStockReportResponseDTO
    {
        public List<CurrentStockReportGroupDTO> Groups { get; set; } = new();
        public decimal GrandTotalQty { get; set; }
        public decimal GrandTotalTk { get; set; }

        // Ordered list of store codes/names for column rendering
        public List<string> StoreBranches { get; set; } = new();

        // Grand total per store — key = store Code/Name
        public Dictionary<string, decimal> GrandTotalByStore { get; set; } = new();
    }
}
