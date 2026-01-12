using JM.UI.Entities.Model.Barcodes;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Barcodes
{
    public partial class BarcodeAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected BarcodeModelDTO Barcode { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Barcode" : "Add New Barcode";
        protected string PageIcon => IsEditMode ? "edit" : "qr_code_scanner";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            if (IsEditMode)
            {
                await LoadBarcode();
            }
        }

        private async Task LoadBarcode()
        {
            try
            {
                IsLoading = true;
                var barcode = await _serviceUnitOfWork.BarcodeService.GetBarcodeById(Id!.Value);

                if (barcode == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Barcode not found.");
                    NavigationManager.NavigateTo("/BarcodeList");
                    return;
                }

                Barcode = barcode;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load barcode: {ex.Message}");
                NavigationManager.NavigateTo("/BarcodeList");
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
                var result = await _serviceUnitOfWork.BarcodeService.SaveUpdateBarcode(Barcode);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Barcode updated successfully!" : "Barcode saved successfully!");
                    NavigationManager.NavigateTo("/BarcodeList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save barcode: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/BarcodeList");
        }
    }
}
