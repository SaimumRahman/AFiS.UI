using JM.UI.Entities.Model.SalesPOS;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Dialog.SalesPOS
{
    public partial class PaymentDialogComponent : ComponentBase
    {
        [Inject] public DialogService DialogService { get; set; } = default!;

        [Parameter] public decimal NetPayable { get; set; }

        protected decimal PaymentAmount { get; set; }
        protected string PaymentType { get; set; } = "Cash";
        protected string TransactionId { get; set; } = "";
        protected List<PaymentTransactionDTO> TempPayments { get; set; } = new();
        protected decimal RemainingAmount => NetPayable - TempPayments.Sum(p => p.Amount);
        protected bool IsBookingPayment { get; set; }

        protected List<string> PaymentTypeOptions { get; set; } = new() { "Cash", "MFS", "Card" };

        protected override void OnInitialized()
        {
            PaymentAmount = NetPayable;
        }

        protected void AddTempPayment()
        {
            if (PaymentAmount <= 0) return;
            TempPayments.Add(new PaymentTransactionDTO
            {
                PaymentType = PaymentType,
                Amount = Math.Min(PaymentAmount, RemainingAmount > 0 ? RemainingAmount : PaymentAmount),
                TransactionId = string.IsNullOrWhiteSpace(TransactionId) ? null : TransactionId,
                ReferenceNo = PaymentType == "MFS" || PaymentType == "Card" ? TransactionId : null,
                PaymentDate = DateTime.Now
            });
            PaymentType = "Cash";
            TransactionId = "";
            PaymentAmount = RemainingAmount;
        }

        protected void RemoveTempPayment(PaymentTransactionDTO payment)
        {
            TempPayments.Remove(payment);
        }

        protected void ConfirmPayment()
        {
            DialogService.Close(new PaymentResultDTO
            {
                Payments = TempPayments.ToList(),
                IsBookingPayment = IsBookingPayment
            });
        }

        protected void Cancel()
        {
            DialogService.Close(null);
        }
    }
}
