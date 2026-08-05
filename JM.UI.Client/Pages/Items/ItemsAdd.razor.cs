using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Items
{
    public partial class ItemsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected ItemDTO Item { get; set; } = new();
        protected IEnumerable<SubGroupModelDTO> SubGroupsList = new List<SubGroupModelDTO>();
        protected IEnumerable<MesurementUnitModelDTO> MeasurementUnitsList = new List<MesurementUnitModelDTO>();
        protected IEnumerable<SupplierModelDTO> SuppliersList = new List<SupplierModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoadingByComponent { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Item" : "New Item";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            // Creating new items is no longer supported from this page.
            if (!IsEditMode)
            {
                NavigationManager.NavigateTo("/ItemsList");
                return;
            }

            await LoadLookups();
            await LoadItem();
        }

        private async Task LoadLookups()
        {
            try
            {
                SubGroupsList = await _serviceUnitOfWork.SubGroupService.GetSubGroups();
                MeasurementUnitsList = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits();
                SuppliersList = await _serviceUnitOfWork.SupplierService.GetSuppliers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        private async Task LoadItem()
        {
            try
            {
                IsLoadingByComponent = true;
                var result = await _serviceUnitOfWork.ItemService.GetItemById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Item not found.");
                    NavigationManager.NavigateTo("/ItemsList");
                    return;
                }

                Item = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load item: {ex.Message}");
                NavigationManager.NavigateTo("/ItemsList");
            }
            finally
            {
                IsLoadingByComponent = false;
            }
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && int.TryParse(result.Value, out int value))
                    return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
            }

            return 0;
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                var currentUserId = await GetLocalStorageInt("UserId");

                var updateItem = new UpdateItemDTO
                {
                    Id = Item.Id,
                    Name = Item.Name,
                    SalePrice = Item.SalePrice,
                    MesurementUnitId = Item.MesurementUnitId,
                    AlarmLevel = Item.AlarmLevel,
                    LastModifiedBy = currentUserId,
                    LastModifiedDate = DateTime.UtcNow
                };

                var result = await _serviceUnitOfWork.ItemService.UpdateItem(updateItem);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        "Item updated successfully!");
                    NavigationManager.NavigateTo("/ItemsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save item: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/ItemsList");
        }
    }
}
