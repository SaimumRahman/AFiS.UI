using JM.UI.Entities.Model.PurchaseReturns;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.PurchaseReturns
{
    public partial class PurchaseReturnsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseReturnModelDTO PurchaseReturn { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<VoucherModelDTO> Vouchers { get; set; } = new List<VoucherModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Return" : "New Purchase Return";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadPurchaseReturn();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
                var vouchersTask = _serviceUnitOfWork.VoucherService.GetVouchers();

                await Task.WhenAll(suppliersTask, storesTask, vouchersTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
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

        private async Task LoadPurchaseReturn()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseReturnService.GetPurchaseReturnById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase Return not found.");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                    return;
                }

                PurchaseReturn = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase return: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseReturnsList");
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
                
                var result = await _serviceUnitOfWork.PurchaseReturnService.SaveUpdatePurchaseReturn(PurchaseReturn);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase Return updated successfully!" : "Purchase Return created successfully!");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase return: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseReturnsList");
        }
    }
}
