namespace JM.UI.Entities.Model.Ecommerce
{
    public class EcommerceFilterRequestDTO
    {
        public int? StoreId { get; set; }
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? DesignId { get; set; }
        public int? BrandId { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public string? ReturnRefNo { get; set; }
        public string? Barcode { get; set; }
    }
}