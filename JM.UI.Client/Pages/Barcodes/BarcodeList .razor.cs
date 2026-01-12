using JM.UI.Entities.Model.Barcodes;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Barcodes
{
    public partial class BarcodeListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<BarcodeModelDTO> BarcodesGrid = default!;
        protected IEnumerable<BarcodeModelDTO> Barcodes = new List<BarcodeModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadBarcodes();
        }

        protected async Task LoadBarcodes()
        {
            try
            {
                IsLoading = true;
                Barcodes = await _serviceUnitOfWork
                    .BarcodeService
                    .GetBarcodes();
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Failed to load barcodes: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddBarcode()
        {
            NavigationManager.NavigateTo("/BarcodeAdd");
        }

        protected void EditBarcode(BarcodeModelDTO barcode)
        {
            NavigationManager.NavigateTo($"/BarcodeAdd/{barcode.Id}");
        }

        protected async Task DeleteBarcode(BarcodeModelDTO barcode)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete barcode ID '{barcode.Id}'?",
                "Confirm Delete"
            );

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork
                    .BarcodeService
                    .DeleteBarcode(barcode.Id);

                notificationService.Notify(
                    result.IsSuccessStatus
                        ? NotificationSeverity.Success
                        : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error",
                    result.Message
                );

                if (result.IsSuccessStatus)
                    await LoadBarcodes();
            }
        }

        public void Dispose()
        {
            BarcodesGrid?.Dispose();
        }
    }
}
