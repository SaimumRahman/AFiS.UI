using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Items
{
    public class CreateItemRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public string? ShadeNo { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public string? MaterialType { get; set; }
        public string? Origin { get; set; }
        public string? Features { get; set; }
        public string? BrandColor { get; set; }
        public decimal? ProductPricePercentage { get; set; }
        public int MesurementUnitId { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }
        public decimal SalePrice { get; set; }
        public decimal WholeSalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public string? ProductType { get; set; }
    }
}
