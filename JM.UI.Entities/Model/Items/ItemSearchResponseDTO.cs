using JM.UI.Entities.Model.Stock;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Items
{
    public class ItemSearchResponseDTO
    {
        public bool Found { get; set; }
        public ItemDTO? Item { get; set; }
        public StockDTO? Stock { get; set; }
        public string? Message { get; set; }
    }
}
