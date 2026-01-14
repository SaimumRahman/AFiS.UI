using JM.UI.Entities.Model.VoucherDetails;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.VoucherDetails
{
    public partial class VoucherDetailsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected VoucherDetailsModelDTO VoucherDetail { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Voucher Entry" : "Add Voucher Entry";
        protected string PageIcon => IsEditMode ? "edit" : "post_add";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            if (IsEditMode)
            {
                await LoadVoucherDetail();
            }
        }

        private async Task LoadVoucherDetail()
        {
            try
            {
                IsLoading = true;
                var detail = await _serviceUnitOfWork.VoucherDetailsService.GetVoucherDetailsById(Id!.Value);

                if (detail == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Entry not found.");
                    NavigationManager.NavigateTo("/VoucherDetailsList");
                    return;
                }

                VoucherDetail = detail;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load entry: {ex.Message}");
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
                        IsEditMode ? "Entry updated successfully!" : "Entry saved successfully!");
                    NavigationManager.NavigateTo("/VoucherDetailsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save entry: {ex.Message}");
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
