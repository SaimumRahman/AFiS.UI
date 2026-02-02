using JM.UI.Entities.Model.PurchaseItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseEntryRequestDTO
    {
        public PurchaseDTO Purchase { get; set; } = new();
        public List<PurchaseItemDTO> Items { get; set; } = new();
    }

}
