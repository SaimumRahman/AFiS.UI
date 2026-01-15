using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.PurchaseOrders
{
    public partial class PurchaseOrdersAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseOrderModelDTO PurchaseOrder { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Order" : "New Purchase Order";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadPurchaseOrder();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                // Fetch Suppliers and Stores in parallel
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();

                await Task.WhenAll(suppliersTask, storesTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPurchaseOrder()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseOrderService.GetPurchaseOrderById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase Order not found.");
                    NavigationManager.NavigateTo("/PurchaseOrdersList");
                    return;
                }

                PurchaseOrder = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase order: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseOrdersList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                
                // Calculate VAT amount if percentage is present but amount is 0 (optional logic, can be enhanced)
                if (PurchaseOrder.VATPercentage > 0 && PurchaseOrder.VAT == 0)
                {
                    // Logic would depend on total amount which isn't in header directly usually, 
                    // but assumes backend might calculate or user enters it manually.
                }

                var result = await _serviceUnitOfWork.PurchaseOrderService.SaveUpdatePurchaseOrder(PurchaseOrder);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase Order updated successfully!" : "Purchase Order created successfully!");
                    NavigationManager.NavigateTo("/PurchaseOrdersList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase order: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseOrdersList");
        }
    }
}
