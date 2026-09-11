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
        public List<ExchangeLineEdit> ExchangeItems { get; set; } = new();

        public decimal TotalCredit => ExchangeItems.Sum(e => e.Credit);

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
                ExchangeItems = ExchangeSale.SaleDetails
                    .Where(d => (d.IsBooking ?? 0) != 1 && (d.Qty - (d.ReturnedQty ?? 0)) > 0)
                    .Select(d => new ExchangeLineEdit
                    {
                        SalesDetailsId = d.SalesDetailsId,
                        ProductName = d.ProductName,
                        Barcode = d.Barcode,
                        UnitPrice = d.UnitPrice,
                        TotalQty = d.Qty,
                        ReturnedQty = d.ReturnedQty ?? 0,
                        Discount = d.Discount ?? 0,
                        IsBooking = d.IsBooking ?? 0
                    })
                    .ToList();
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Not Found",
                    "Invoice not found", 3000);
            }
        }

        protected void OnReturnQtyChanged(decimal value)
        {
            StateHasChanged();
        }

        protected void ApplyExchange()
        {
            var selected = ExchangeItems.Where(e => e.ReturnQty > 0).ToList();
            if (selected.Count == 0)
            {
                NotificationService.Notify(NotificationSeverity.Warning, "No Items",
                    "Select at least one item to exchange", 3000);
                return;
            }

            DialogService.Close(new ExchangeResultDTO
            {
                InvoiceNo = ExchangeInvoiceNo,
                ExchangeAmount = TotalCredit,
                IsReturnExchange = true,
                ExchangeItems = selected.Select(e => new ExchangeItemDTO
                {
                    SalesDetailsId = e.SalesDetailsId,
                    Qty = e.ReturnQty,
                    Credit = e.Credit
                }).ToList()
            });
        }

        protected void Cancel()
        {
            DialogService.Close(null);
        }
    }

    public class ExchangeLineEdit
    {
        public int SalesDetailsId { get; set; }
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal Discount { get; set; }
        public decimal ReturnQty { get; set; }
        public int IsBooking { get; set; }
        public decimal Available => TotalQty - ReturnedQty;
        public decimal Credit => ReturnQty > 0
            ? Math.Round(UnitPrice * ReturnQty - Discount * (ReturnQty / (TotalQty > 0 ? TotalQty : 1)), 2)
            : 0;
    }
}