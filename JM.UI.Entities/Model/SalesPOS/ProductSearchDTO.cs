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
        public string? SalesPersonName { get; set; }
        public int? SalesPersonId { get; set; }
    }
}
