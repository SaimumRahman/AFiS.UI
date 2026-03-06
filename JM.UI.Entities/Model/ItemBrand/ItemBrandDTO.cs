using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.ItemBrand
{
    public class ItemBrandDTO
    {
        public int BrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
