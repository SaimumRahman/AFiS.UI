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
    public partial class PurchasesListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<PurchaseModelDTO> PurchasesGrid = default!;
        protected IEnumerable<PurchaseModelDTO> PurchasesList = new List<PurchaseModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPurchases();
        }

        protected async Task LoadPurchases()
        {
            try
            {
                IsLoading = true;
                PurchasesList = await _serviceUnitOfWork.PurchaseService.GetPurchases();
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
            NavigationManager.NavigateTo("/PurchasesAdd");
        }

        protected void EditPurchase(PurchaseModelDTO purchase)
        {
            NavigationManager.NavigateTo($"/PurchasesAdd/{purchase.Id}");
        }

        protected async Task DeletePurchase(PurchaseModelDTO purchase)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Purchase ID #{purchase.Id}?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.PurchaseService.DeletePurchase(purchase.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadPurchases();
            }
        }

        public void Dispose()
        {
            PurchasesGrid?.Dispose();
        }
    }
}
