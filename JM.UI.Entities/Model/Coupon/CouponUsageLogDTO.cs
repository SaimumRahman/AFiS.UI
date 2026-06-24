namespace JM.UI.Entities.Model.Coupon
{
    public class CouponUsageLogDTO
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public string? CouponCode { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int? UsedBy { get; set; }
        public string? UsedByName { get; set; }
        public DateTime UsedAt { get; set; }
        public decimal DiscountGiven { get; set; }
        public string? OrderReference { get; set; }
    }
}
