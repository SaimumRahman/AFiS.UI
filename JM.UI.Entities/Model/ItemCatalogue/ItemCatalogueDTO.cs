using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.ItemCatalogue
{
    public class ItemCatalogueDTO
    {
        public int CatalogueId { get; set; }
        public string CatalogueName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
