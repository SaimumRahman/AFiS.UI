
using JM.UI.Entities.Model.Accounts;
using JM.UI.Entities.Model.VoucherDetails;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.VoucherDetails
{
    public partial class VoucherDetailsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected VoucherDetailsModelDTO VoucherDetail { get; set; } = new();
        protected IEnumerable<VoucherModelDTO> VouchersList { get; set; } = new List<VoucherModelDTO>();
        protected IEnumerable<AccountModelDTO> AccountsList { get; set; } = new List<AccountModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Voucher Detail" : "New Voucher Detail";
        protected string PageIcon => IsEditMode ? "edit" : "add_circle";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadVoucherDetail();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var vouchersTask = _serviceUnitOfWork.VoucherService.GetVouchers();
                var accountsTask = _serviceUnitOfWork.AccountsService.GetAccounts();

                await Task.WhenAll(vouchersTask, accountsTask);

                VouchersList = await vouchersTask;
                AccountsList = await accountsTask;
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

        private async Task LoadVoucherDetail()
        {
            try
            {
                IsLoading = true;
                // Assuming GetVoucherDetailsById exists in service
                var result = await _serviceUnitOfWork.VoucherDetailsService.GetVoucherDetailsById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Voucher Detail not found.");
                    NavigationManager.NavigateTo("/VoucherDetailsList");
                    return;
                }

                VoucherDetail = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load voucher detail: {ex.Message}");
                NavigationManager.NavigateTo("/VoucherDetailsList");
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
                
                var result = await _serviceUnitOfWork.VoucherDetailsService.SaveUpdateVoucherDetails(VoucherDetail);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Voucher Detail updated successfully!" : "Voucher Detail created successfully!");
                    NavigationManager.NavigateTo("/VoucherDetailsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save voucher detail: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/VoucherDetailsList");
        }
    }
}
