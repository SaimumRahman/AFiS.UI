using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Stock
{
    public class StockDTO
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal LastCostPrice { get; set; }
        public decimal AverageCostPrice { get; set; }
        public int? SizeId { get; set; }
        public int? ColorId { get; set; }
        public int StoreId { get; set; }
    }

}
