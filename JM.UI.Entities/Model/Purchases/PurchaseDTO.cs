using JM.UI.Entities.Model.PurchaseItems;
using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseDTO
    {
        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SystemInvoiceNo { get; set; }
        public string? BillInvoiceNumber { get; set; }
        public string? BillInvoiceName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; } = 0m;
        public decimal? VatAmount { get; set; } = 0m;

        public bool IsVatIncluded { get; set; }
        public decimal? OtherCostTotal { get; set; }
        public decimal? CarryingCostTotal { get; set; }
        public decimal? OperationalCostTotal { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation
        public List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
    }
}
