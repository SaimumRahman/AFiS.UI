using System.Collections.Generic;

namespace JM.UI.Entities.Model.SalesPOS
{
    public class ProductSearchDTO
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? StockQuantity { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public string? ImagePath { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }

        public List<ProductStockInfo> StockByBranch { get; set; } = new();
    }

    public class ProductStockInfo
    {
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public decimal StockQuantity { get; set; }
    }
}
