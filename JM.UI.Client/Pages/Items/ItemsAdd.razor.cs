using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Items
{
    public partial class ItemsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected ItemModelDTO Item { get; set; } = new();
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
            await LoadLookups();
            if (IsEditMode)
            {
                await LoadItem();
            }
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

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.ItemService.SaveUpdateItem(Item);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Item updated successfully!" : "Item created successfully!");
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
