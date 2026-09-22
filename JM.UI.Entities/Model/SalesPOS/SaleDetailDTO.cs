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
        /// <summary>
        /// The sale price originally loaded from the product. The user may raise the price in the
        /// cart but it cannot be reduced below this value.
        /// </summary>
        public decimal BaseUnitPrice { get; set; }
        public decimal Qty { get; set; }
        public decimal? ReturnedQty { get; set; }
        public decimal? Discount { get; set; }
        public int? IsBooking { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Vat { get; set; }
        public bool HasDiscount { get; set; }
        public int CreatedBy { get; set; }
        public int? StoreId { get; set; }

        /// <summary>
        /// True when this cart line is a returned item from a previous invoice (exchange flow).
        /// Return lines are credited from the original invoice, never written as new sale details
        /// nor issued stock; their quantity is restored via the exchange StockIn step on the API.
        /// </summary>
        public bool IsExchangeReturn { get; set; }

        /// <summary>
        /// Original SalesDetail row id the returned quantity belongs to (exchange flow).
        /// </summary>
        public int SourceSalesDetailsId { get; set; }

        /// <summary>
        /// Original invoice number this line was returned from (exchange flow).
        /// </summary>
        public string? SourceInvoiceNo { get; set; }

        /// <summary>
        /// Max quantity still returnable for an exchange-return line (original qty minus qty
        /// already returned on the source invoice). Used only by cart edit caps.
        /// </summary>
        public decimal AvailableQty { get; set; }

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
             BaseUnitPrice = product.UnitPrice,
             Qty = qty,
            Discount = product.Discount > 0 ? product.Discount : null,
            HasDiscount = product.HasDiscount,
            StoreId = product.StoreId,
            TotalAmount = product.UnitPrice * qty
        };
    }
}