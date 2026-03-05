namespace JM.UI.Entities.Model.PurchaseItems
{
    public class PurchaseItemDTO
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? GroupName { get; set; }
        public string? SubGroupName { get; set; }
        public string? ShadeNo { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public string? Barcode { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? ProductPricePercentage { get; set; }
        public decimal? OtherCost { get; set; }
        public decimal? CarryingCost { get; set; }
        public decimal? OperationalCost { get; set; }
        public decimal? VatPercentage { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsSaleable { get; set; }
        public decimal? SalePrice { get; set; }
        public string? ProductType { get; set; } // SellProduct, RawMaterial, Both, Consume, ComboPackage
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }

        // NEW: Flag to indicate if this is a new item being created
        public bool IsNewItem { get; set; }

        // NEW: Additional fields for creating new item
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? DesignId { get; set; }
        public string? DesignName { get; set; }
        public string? MaterialType { get; set; }
        public string? Origin { get; set; }
        public string? Features { get; set; }
        public string? BrandColor { get; set; }
        public int? MesurementUnitId { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }
        public string? Catalogue { get; set; }

    }

}
