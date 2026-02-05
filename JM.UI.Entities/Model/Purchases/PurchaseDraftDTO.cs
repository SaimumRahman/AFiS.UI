using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseDraftDTO
    {
        public int Id { get; set; }
        public string DraftName { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? BillInvoiceNumber { get; set; }
        public string? BillInvoiceName { get; set; }
        public bool IsVatIncluded { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int? LastModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public List<PurchaseDraftItemDTO> DraftItems { get; set; } = new();
    }
}
