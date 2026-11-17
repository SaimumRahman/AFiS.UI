using JM.UI.Entities.Model.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Discount
{
    public class DiscountGridItem
    {
        public ItemDTO Item { get; set; } = new();
        public bool Selected { get; set; }
        public string Source { get; set; } = "";
        public decimal DiscountValue { get; set; }
        public int DiscountTypeId { get; set; } = 1;
        public string DiscountTypeName { get; set; } = "Percentage";
        public decimal CurrentSalePrice { get; set; }

        /// <summary>
        /// Campaign names this product will be moved out of when the campaign is saved.
        /// Populated only after the user answered "Yes" to the conflict dialog.
        /// </summary>
        public List<string> MovedFromCampaigns { get; set; } = new();
    }
}
