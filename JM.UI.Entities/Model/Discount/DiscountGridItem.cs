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
    }
}
