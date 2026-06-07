using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.PurchaseItems
{
    public class PreviewItemRow
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int? ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string SizeName { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? DesignId { get; set; }
        public int? BrandId { get; set; }
        public decimal BasePurchasePrice { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int? OriginId { get; set; }
        public string OriginName { get; set; } = string.Empty;
        public List<int> FeatureIds { get; set; } = new();
        public string FeaturesDisplay { get; set; } = string.Empty;
        public int? MesurementUnitId { get; set; }
        public string MesurementUnitName { get; set; } = string.Empty;
        public int? CatalogueId { get; set; }
        public string CatalogueName { get; set; } = string.Empty;
        public string? MaterialType { get; set; }
        public string ProductType { get; set; } = "Sell Product";
        public string ImageBase64 { get; set; } 
        public bool IsSaleable { get; set; } = true;
        public bool IsConsume { get; set; }
        public bool IsNewItem { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }
        // Editable pricing fields
        public decimal Quantity { get; set; } = 0;
        public decimal StockQuantity { get; set; } = 0;
        public decimal PurchasePrice { get; set; } = 0;
        public decimal SalePrice { get; set; } = 0;
        public decimal? OtherCost { get; set; }
        public decimal? CarryingCost { get; set; }
        public decimal? TransportCost { get; set; }
        public decimal? OperationalCost { get; set; }
        public decimal? VatPercentage { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public string ShadeNo { get; set; }
    }
}
