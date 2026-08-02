namespace JM.UI.Entities.Model.SalesPOS
{
    public class SaleDetailDTO
    {
        public int SalesDetailsId { get; set; }
        public int SalesMasterId { get; set; }
        public int ItemId { get; set; }
        public string? Barcode { get; set; }
        public string? ReturnRefNo { get; set; }
        public string? ProductName { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? UomId { get; set; }
        public string? UomName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Vat { get; set; }
        public int CreatedBy { get; set; }

        public static SaleDetailDTO FromProductSearch(ProductSearchDTO product, decimal qty = 1) => new()
        {
            ItemId = product.ItemId,
            Barcode = product.Barcode,
            ReturnRefNo = product.ReturnRefNo,
            ProductName = product.ProductName,
            SalesPersonId = product.SalesPersonId,
            SalesPersonName = product.SalesPersonName,
            UomId = product.UomId,
            UomName = product.UomName,
            UnitPrice = product.UnitPrice,
            Qty = qty,
            TotalAmount = product.UnitPrice * qty
        };
    }
}