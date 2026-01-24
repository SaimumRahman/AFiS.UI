using JM.UI.Entities.Model.PurchaseItems;
using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseModelDTO
    {
        public int Id { get; set; }
        public int StorePurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public string? InvoiceNo { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int StoreId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public int VoucherId { get; set; }

        // Financials
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal VAT { get; set; }
        public decimal VATPercentage { get; set; }
        public decimal LabourCharge { get; set; }
        public decimal TransportCost { get; set; }
        public decimal Total { get; set; }
        public decimal NetTotal { get; set; }

        public List<PurchaseItemsDTO> PurchaseItems { get; set; } = new();

        // UI Helpers
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? PurchaseOrderNo { get; set; }
    }
}
