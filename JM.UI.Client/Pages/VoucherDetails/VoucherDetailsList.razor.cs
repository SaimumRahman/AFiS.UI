using JM.UI.Entities.Model.VoucherDetails;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.VoucherDetails
{
    public partial class VoucherDetailsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<VoucherDetailsModelDTO> VoucherDetailsGrid = default!;
        protected IEnumerable<VoucherDetailsModelDTO> VoucherDetailsList = new List<VoucherDetailsModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadVoucherDetails();
        }

        protected async Task LoadVoucherDetails()
        {
            try
            {
                IsLoading = true;
                VoucherDetailsList = await _serviceUnitOfWork
                    .VoucherDetailsService
                    .GetVoucherDetails();
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Failed to load voucher details: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddVoucherDetail()
        {
            NavigationManager.NavigateTo("/VoucherDetailsAdd");
        }

        protected void EditVoucherDetail(VoucherDetailsModelDTO detail)
        {
            NavigationManager.NavigateTo($"/VoucherDetailsAdd/{detail.Id}");
        }

        protected async Task DeleteVoucherDetail(VoucherDetailsModelDTO detail)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete voucher detail ID '{detail.Id}'?",
                "Confirm Delete"
            );

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork
                    .VoucherDetailsService
                    .DeleteVoucherDetails(detail.Id);

                notificationService.Notify(
                    result.IsSuccessStatus
                        ? NotificationSeverity.Success
                        : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error",
                    result.Message
                );

                if (result.IsSuccessStatus)
                    await LoadVoucherDetails();
            }
        }

        public void Dispose()
        {
            VoucherDetailsGrid?.Dispose();
        }
    }
}
