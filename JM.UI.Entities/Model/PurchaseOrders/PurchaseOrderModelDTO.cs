using JM.UI.Entities.Model.PurchaseOrderItems;
using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.PurchaseOrders
{
    public class PurchaseOrderModelDTO
    {
        public int Id { get; set; }
        public DateTime PurchaseOrderDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public int StoreId { get; set; }
        public string? Remarks { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal VAT { get; set; }
        public decimal VATPercentage { get; set; }
        public int PurchaseOrderStatus { get; set; }

        public List<PurchaseOrderItemsDTO> PurchaseOrderItems { get; set; } = new();

        // UI Helpers
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
    }
}
