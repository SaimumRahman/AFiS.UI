namespace JM.UI.Entities.Model.SalesPOS
{
    public class SaleDetailDTO
    {
        public int Id { get; set; }
        public int SaleMasterId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public decimal Quantity { get; set; } = 1m;
        public decimal SalePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int? StoreId { get; set; }
        public string? ImagePath { get; set; }
        public decimal? StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
