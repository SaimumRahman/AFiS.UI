using JM.UI.Entities.Model.Vouchers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Vouchers
{
    public partial class VoucherListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<VoucherModelDTO> VouchersGrid = default!;
        protected IEnumerable<VoucherModelDTO> VouchersList = new List<VoucherModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadVouchers();
        }

        protected async Task LoadVouchers()
        {
            try
            {
                IsLoading = true;
                VouchersList = await _serviceUnitOfWork.VoucherService.GetVouchers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load vouchers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddVoucher()
        {
            NavigationManager.NavigateTo("/VoucherAdd");
        }

        protected void EditVoucher(VoucherModelDTO voucher)
        {
            NavigationManager.NavigateTo($"/VoucherAdd/{voucher.Id}");
        }

        protected async Task DeleteVoucher(VoucherModelDTO voucher)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Voucher No '{voucher.VoucherNo}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.VoucherService.DeleteVoucher(voucher.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadVouchers();
            }
        }

        public void Dispose()
        {
            VouchersGrid?.Dispose();
        }
    }
}
