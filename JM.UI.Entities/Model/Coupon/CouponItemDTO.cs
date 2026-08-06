namespace JM.UI.Entities.Model.Coupon
{
    public class CouponItemDTO
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? Barcode { get; set; }
        public decimal? MinQty { get; set; }
        public decimal? ItemDiscountOverride { get; set; }
        public int? AssignedBy { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
