using System;

namespace JM.UI.Entities.Model.SalesPOS
{
    public class PaymentTransactionDTO
    {
        public int Id { get; set; }
        public int SaleMasterId { get; set; }
        public string? PaymentType { get; set; } // Cash, MFS, Card
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
