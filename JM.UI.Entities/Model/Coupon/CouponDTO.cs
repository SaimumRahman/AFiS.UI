namespace JM.UI.Entities.Model.Coupon
{
    public class CouponDTO
    {
        public int Id { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CouponTypeId { get; set; }
        public string? CouponTypeName { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimitTotal { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public int UsedCount { get; set; }
        public bool IsActive { get; set; }
        public bool ApplicableToAll { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<CouponItemDTO> CouponItems { get; set; } = new();
        public List<CouponCustomerBindDTO> CustomerBindings { get; set; } = new();
    }
}
