using System;

namespace JM.UI.Entities.Model.StockOpening
{
    public class StockOpeningItemDTO
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? HexCode { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public decimal Quantity { get; set; }
        public decimal TradePrice { get; set; } // Purchase Price equivalent
        public decimal MRP { get; set; } // Sale Price equivalent
        
        // These fields are needed if we generate a new item on the fly during opening
        public bool IsNewItem { get; set; }
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? MesurementUnitId { get; set; }
        public string? MesurementUnitName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? OriginId { get; set; }
        public string? OriginName { get; set; }
        public string? Features { get; set; }
        public decimal SalePrice { get; set; }
        public bool IsSaleable { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }
        public string? ShadeNo { get; set; }
        public string? Catalogue { get; set; }
        public int? ProductType { get; set; }

        public decimal TotalAmount => Quantity * TradePrice;
    }
}
