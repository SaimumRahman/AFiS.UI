using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.ItemWiseFeature;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Stock;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class BarcodeSearchResponseDTO
    {
        public bool Found { get; set; }
        public PurchaseItemDTO? Item { get; set; }
        public ItemDTO? ItemDetails { get; set; }
        public StockDTO? Stock { get; set; }
        public List<ItemWiseFeatureDTO?> itemWiseFeatures { get; set; }
        public string? Message { get; set; }
    }
}
