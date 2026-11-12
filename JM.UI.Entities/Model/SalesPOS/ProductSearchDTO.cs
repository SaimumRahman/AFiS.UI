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
        public bool IsOtherStore { get; set; }

        /// <summary>
        /// Item classification. Lets the POS allow a zero selling price for
        /// Consumable / Raw Material items (e.g. shopping bags given with a purchase).
        /// </summary>
        public bool IsConsume { get; set; }
        public bool IsRawMaterial { get; set; }
        public string? ProductType { get; set; }
        public string? GroupName { get; set; }
    }
}
