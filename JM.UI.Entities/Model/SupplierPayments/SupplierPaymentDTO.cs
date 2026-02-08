using System;

namespace JM.UI.Entities.Model.SupplierPayments
{
    public class SupplierPaymentDTO
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public int StoreId { get; set; }

        public decimal PaymentAmount { get; set; }
        public string? PaymentMethod { get; set; } = "Cash"; // Default to Cash
        public string? ReferenceNo { get; set; }

        public int? BankId { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }

        public int? VoucherId { get; set; }
        public string? Remarks { get; set; }
        public string? UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }

        // UI Helpers
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? BankName { get; set; }
    }
}
