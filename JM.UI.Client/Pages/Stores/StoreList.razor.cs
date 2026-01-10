
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Store
{
    public partial class StoreListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<StoreDTO> StoresGrid = default!;
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadStores();
        }

        private async Task LoadStores()
        {
            try
            {
                IsLoading = true;
                Stores = await _serviceUnitOfWork.StoreService.GetStores();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stores: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddStore()
        {
            NavigationManager.NavigateTo("/StoreAdd");
        }

        protected void EditStore(StoreDTO store)
        {
            NavigationManager.NavigateTo($"/StoreAdd/{store.Id}");
        }

        protected async Task DeleteStore(StoreDTO store)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete store '{store.Name}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.StoreService.DeleteStore(store.Id);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Store deleted successfully.");
                    await LoadStores();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete store.");
                }
            }
        }

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            StoresGrid?.Dispose();
        }
    }
}