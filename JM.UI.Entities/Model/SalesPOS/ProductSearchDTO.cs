namespace JM.UI.Entities.Model.SalesPOS
{
    public class ProductSearchDTO
    {
        public int ItemId { get; set; }
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public string? ReturnRefNo { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal StockQuantity { get; set; }
        public int? UomId { get; set; }
        public string? UomName { get; set; }
        public string? StoreName { get; set; }
        public int? StoreId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SalesPersonId { get; set; }
        public string? ItemName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Discount { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal Vat { get; set; }
        public bool HasDiscount { get; set; }
    }
}
