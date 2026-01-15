using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.PurchaseOrders
{
    public partial class PurchaseOrdersListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<PurchaseOrderModelDTO> PurchaseOrdersGrid = default!;
        protected IEnumerable<PurchaseOrderModelDTO> PurchaseOrdersList = new List<PurchaseOrderModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPurchaseOrders();
        }

        protected async Task LoadPurchaseOrders()
        {
            try
            {
                IsLoading = true;
                PurchaseOrdersList = await _serviceUnitOfWork.PurchaseOrderService.GetPurchaseOrders();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase orders: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddPurchaseOrder()
        {
            NavigationManager.NavigateTo("/PurchaseOrdersAdd");
        }

        protected void EditPurchaseOrder(PurchaseOrderModelDTO order)
        {
            NavigationManager.NavigateTo($"/PurchaseOrdersAdd/{order.Id}");
        }

        protected async Task DeletePurchaseOrder(PurchaseOrderModelDTO order)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Purchase Order #{order.Id}?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.PurchaseOrderService.DeletePurchaseOrder(order.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadPurchaseOrders();
            }
        }

        public void Dispose()
        {
            PurchaseOrdersGrid?.Dispose();
        }
    }
}
