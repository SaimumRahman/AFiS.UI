namespace JM.UI.Entities.Model.PurchaseOrderItems
{
    public class PurchaseOrderItemsDTO
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TradePrice { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }

        // Helper Properties
        public string? ItemName { get; set; }
        public string? ColorName { get; set; }
        public string? SizeName { get; set; }
    }
}
