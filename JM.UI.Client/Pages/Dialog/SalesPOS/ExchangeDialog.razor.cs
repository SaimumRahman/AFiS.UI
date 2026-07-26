using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Service.UnitOfWork;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace JM.UI.Client.Pages.Dialog.SalesPOS
{
    public partial class ExchangeDialogComponent : ComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService DialogService { get; set; } = default!;
        [Inject] public NotificationService NotificationService { get; set; } = default!;

        public string ExchangeInvoiceNo { get; set; } = "";
        public SaleMasterDTO? ExchangeSale { get; set; }
        public List<SaleDetailDTO> ExchangeItems { get; set; } = new();

        protected async Task OnExchangeSearchKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await SearchExchangeInvoice();
        }

        protected async Task SearchExchangeInvoice()
        {
            if (string.IsNullOrWhiteSpace(ExchangeInvoiceNo)) return;
            ExchangeSale = await _serviceUnitOfWork.SaleService.GetSaleByInvoiceNo(ExchangeInvoiceNo.Trim());
            if (ExchangeSale != null)
            {
                ExchangeItems = ExchangeSale.SaleDetails.Where(d => d.IsDeleted != true).ToList();
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Not Found",
                    "Invoice not found", 3000);
            }
        }

        protected void SelectExchangeItem(SaleDetailDTO item)
        {
            DialogService.Close(new ExchangeResultDTO
            {
                InvoiceNo = ExchangeInvoiceNo,
                ExchangeAmount = item.TotalAmount,
                IsReturnExchange = true
            });
        }

        protected void Cancel()
        {
            DialogService.Close(null);
        }
    }
}
