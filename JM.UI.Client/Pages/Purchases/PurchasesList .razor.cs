using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Purchases
{
    public partial class PurchaseListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<PurchaseSummaryDTO> PurchasesGrid = default!;
        protected IEnumerable<PurchaseSummaryDTO> Purchases { get; set; } = new List<PurchaseSummaryDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPurchases();
        }

        private async Task LoadPurchases()
        {
            try
            {
                IsLoading = true;
                Purchases = await _serviceUnitOfWork.PurchaseService.GetAllPurchases();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchases: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddPurchase()
        {
            NavigationManager.NavigateTo("/PurchaseEntry");
        }

        protected void EditPurchase(PurchaseSummaryDTO purchase)
        {
            NavigationManager.NavigateTo($"/PurchaseEntry/{purchase.Id}");
        }

        protected void ViewPurchase(PurchaseSummaryDTO purchase)
        {
            NavigationManager.NavigateTo($"/PurchaseEntry/{purchase.Id}");
        }

        protected async Task DeletePurchase(PurchaseSummaryDTO purchase)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete purchase '{purchase.BillInvoiceNumber}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.PurchaseService.DeletePurchase(purchase.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Purchase deleted successfully.");
                    await LoadPurchases();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete purchase.");
                }
            }
        }

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            PurchasesGrid?.Dispose();
        }
    }
}
