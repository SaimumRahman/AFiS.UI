using JM.UI.Entities.Model.Vouchers;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Vouchers
{
    public partial class VoucherAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected VoucherModelDTO Voucher { get; set; } = new();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Voucher" : "Add Voucher";
        protected string PageIcon => IsEditMode ? "edit" : "post_add";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadStores();

            if (IsEditMode)
            {
                await LoadVoucher();
            }
            else
            {
                Voucher.VoucherDate = DateTime.Now;
                // You can set default store or user here if needed
            }
        }

        private async Task LoadStores()
        {
            try
            {
                Stores = await _serviceUnitOfWork.StoreService.GetStores();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stores: {ex.Message}");
            }
        }

        private async Task LoadVoucher()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.VoucherService.GetVoucherById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Voucher not found.");
                    NavigationManager.NavigateTo("/VoucherList");
                    return;
                }

                Voucher = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load voucher: {ex.Message}");
                NavigationManager.NavigateTo("/VoucherList");
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
                var result = await _serviceUnitOfWork.VoucherService.SaveUpdateVoucher(Voucher);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Voucher updated successfully!" : "Voucher saved successfully!");
                    NavigationManager.NavigateTo("/VoucherList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save voucher: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/VoucherList");
        }
    }
}
