using JM.UI.Entities.Model.PurchaseItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class BarcodeSearchResponseDTO
    {
        public bool Found { get; set; }
        public PurchaseItemDTO? Item { get; set; }
        public string? Message { get; set; }
    }
}
