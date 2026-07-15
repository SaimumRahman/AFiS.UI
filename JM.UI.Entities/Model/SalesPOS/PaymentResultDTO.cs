namespace JM.UI.Entities.Model.SalesPOS
{
    public class PaymentResultDTO
    {
        public List<PaymentTransactionDTO> Payments { get; set; } = new();
        public bool IsBookingPayment { get; set; }
    }
}
