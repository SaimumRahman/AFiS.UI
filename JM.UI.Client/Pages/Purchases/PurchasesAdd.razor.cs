using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Purchases
{
    public partial class PurchasesAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseModelDTO Purchase { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<PurchaseOrderModelDTO> PurchaseOrders { get; set; } = new List<PurchaseOrderModelDTO>();
        protected IEnumerable<VoucherModelDTO> Vouchers { get; set; } = new List<VoucherModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase" : "New Purchase";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadPurchase();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
                var poTask = _serviceUnitOfWork.PurchaseOrderService.GetPurchaseOrders();
                var vouchersTask = _serviceUnitOfWork.VoucherService.GetVouchers();

                await Task.WhenAll(suppliersTask, storesTask, poTask, vouchersTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
                PurchaseOrders = await poTask;
                Vouchers = await vouchersTask;
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

        private async Task LoadPurchase()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseService.GetPurchaseById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase not found.");
                    NavigationManager.NavigateTo("/PurchasesList");
                    return;
                }

                Purchase = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
                NavigationManager.NavigateTo("/PurchasesList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void CalculateTotals()
        {
            // Example calculation logic
            // Assuming Total is manually entered (sum of items) or we calculate NetTotal from Total + charges - discounts
            
            // If discount percentage is updated, calculate discount amount
            if (Purchase.DiscountPercentage > 0 && Purchase.Total > 0)
            {
                Purchase.Discount = (Purchase.Total * Purchase.DiscountPercentage) / 100;
            }
            
            // If VAT percentage is updated, calculate VAT amount
            if (Purchase.VATPercentage > 0 && Purchase.Total > 0)
            {
                 // VAT usually on amount after discount? or before? assuming after for now or on gross total.
                 // Simple VAT on Total for standard implementations unless specified otherwise
                 Purchase.VAT = (Purchase.Total * Purchase.VATPercentage) / 100;
            }

            Purchase.NetTotal = (Purchase.Total + Purchase.LabourCharge + Purchase.TransportCost + Purchase.VAT) - Purchase.Discount;
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                
                // Ensure calculations are fresh
                CalculateTotals();

                // Set edit info
                if (IsEditMode)
                {
                    Purchase.EditedDate = DateTime.Now;
                    // Purchase.EditedBy = ... (get from auth context if needed)
                }

                var result = await _serviceUnitOfWork.PurchaseService.SaveUpdatePurchase(Purchase);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase updated successfully!" : "Purchase created successfully!");
                    NavigationManager.NavigateTo("/PurchasesList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchasesList");
        }
    }
}
