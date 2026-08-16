namespace JM.UI.Entities.Model.Ecommerce
{
    public class EcommerceItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int? ItemCode { get; set; }
        public bool? IsSaleable { get; set; }
        public string? ProductType { get; set; }

        public int? GroupId { get; set; }
        public string? GroupName { get; set; }

        public int? SubGroupId { get; set; }
        public string? SubGroupName { get; set; }

        public int? DesignId { get; set; }
        public string? DesignName { get; set; }

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }

        public int? SizeId { get; set; }
        public string? SizeName { get; set; }

        public int? MesurementUnitId { get; set; }
        public string? UnitName { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? LastCostPrice { get; set; }
        public decimal CurrentStock { get; set; }

        public string? ReturnRefNo { get; set; }
        public string? ShadeNo { get; set; }
        public string? ImageBase64 { get; set; }
        public string? Catalogue { get; set; }

        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
    }
}