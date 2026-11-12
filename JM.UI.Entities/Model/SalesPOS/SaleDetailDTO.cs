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
        public int DiscountTypeId { get; set; }
        public int? IsBooking { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Vat { get; set; }
        public bool HasDiscount { get; set; }
        public int CreatedBy { get; set; }
        public int? StoreId { get; set; }

        /// <summary>
        /// True (1) when this line is a returned item on a return/exchange document.
        /// Returned lines carry a negative TotalAmount and are never stock-issued.
        /// </summary>
        public int? IsReturn { get; set; }

        /// <summary>
        /// Item classification (read-only, not persisted). A zero unit price is allowed
        /// for Consumable / Raw Material items only.
        /// </summary>
        public bool IsConsume { get; set; }
        public bool IsRawMaterial { get; set; }
        public string? ProductType { get; set; }
        public string? GroupName { get; set; }

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
            DiscountTypeId = product.DiscountTypeId,
            HasDiscount = product.HasDiscount,
            StoreId = product.StoreId,
            IsConsume = product.IsConsume,
            IsRawMaterial = product.IsRawMaterial,
            ProductType = product.ProductType,
            GroupName = product.GroupName,
            TotalAmount = product.UnitPrice * qty
        };
    }
}