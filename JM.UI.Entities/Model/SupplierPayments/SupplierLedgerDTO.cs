using System;

namespace JM.UI.Entities.Model.SupplierPayments
{
    public class SupplierLedgerDTO
    {
        public string TransactionType { get; set; } = string.Empty;
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string? Remarks { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
