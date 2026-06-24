namespace JM.UI.Entities.Model.Coupon
{
    public class CouponCustomerBindDTO
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
    }
}
