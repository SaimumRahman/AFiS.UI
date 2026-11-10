namespace JM.UI.Entities.Model.SalesPOS
{
    public class PaymentTransactionDTO
    {
        public int PayTranId { get; set; }
        public string? TransactionNo { get; set; }
        public int SalesMasterId { get; set; }
        public int? BookingMasterId { get; set; }
        public DateTime? BusinessDate { get; set; }
        public int? CounterId { get; set; }
        public decimal? PaidAmount { get; set; }
        public int TransactionTypeId { get; set; }
        public int CreatedBy { get; set; }
    }
}