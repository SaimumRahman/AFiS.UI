using System.Collections.Generic;
using JM.UI.Entities.Model.PurchaseReturnItems;

namespace JM.UI.Entities.Model.PurchaseReturns
{
    public class PurchaseReturnModelDTO
    {
        public int Id { get; set; }
        public DateTime ReturnDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public int StoreId { get; set; }
        public int StorePurchaseReturnId { get; set; }
        public string? Remarks { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int VoucherId { get; set; }

        // UI Helpers
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public int? VoucherNo { get; set; }

        public List<PurchaseReturnItemModelDTO> Items { get; set; } = new();
    }
}
