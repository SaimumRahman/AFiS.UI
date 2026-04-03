using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockReport_D
{
    public class CurrentStockReportFilterDTO
    {
        public int? StoreId { get; set; }
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public string? ProductType { get; set; }
    }
}
