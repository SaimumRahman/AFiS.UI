using JM.UI.Entities.Model.PurchaseReturns;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.PurchaseReturns
{
    public partial class PurchaseReturnsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<PurchaseReturnModelDTO> PurchaseReturnsGrid = default!;
        protected IEnumerable<PurchaseReturnModelDTO> PurchaseReturnsList = new List<PurchaseReturnModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPurchaseReturns();
        }

        protected async Task LoadPurchaseReturns()
        {
            try
            {
                IsLoading = true;
                PurchaseReturnsList = await _serviceUnitOfWork.PurchaseReturnService.GetPurchaseReturns();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase returns: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddPurchaseReturn()
        {
            NavigationManager.NavigateTo("/PurchaseReturnsAdd");
        }

        protected void EditPurchaseReturn(PurchaseReturnModelDTO purchaseReturn)
        {
            NavigationManager.NavigateTo($"/PurchaseReturnsAdd/{purchaseReturn.Id}");
        }

        protected async Task DeletePurchaseReturn(PurchaseReturnModelDTO purchaseReturn)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Purchase Return ID #{purchaseReturn.Id}?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.PurchaseReturnService.DeletePurchaseReturn(purchaseReturn.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadPurchaseReturns();
            }
        }

        public void Dispose()
        {
            PurchaseReturnsGrid?.Dispose();
        }
    }
}
