using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Ecommerce;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Ecommerce
{
    public partial class EcommerceItemsComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<EcommerceItemDTO> ItemsGrid = default!;
        protected IEnumerable<EcommerceItemDTO> ItemsList = new List<EcommerceItemDTO>();
        protected bool IsLoading;

        protected EcommerceStoreDTO? ActiveStore { get; set; }
        protected IEnumerable<StoreDTO> StoresList = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> GroupsList = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroupsList = new List<SubGroupModelDTO>();
        protected IEnumerable<DesignModelDTO> DesignsList = new List<DesignModelDTO>();
        protected IEnumerable<ItemBrandDTO> BrandsList = new List<ItemBrandDTO>();
        protected IEnumerable<ColorsDTO> ColorsList = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> SizesList = new List<SizesDTO>();

        protected EcommerceFilterRequestDTO Filter { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadFilterLookups();
            await ResolveEcommerceStore();
            await LoadItems();
        }

        private async Task LoadFilterLookups()
        {
            try
            {
                var groupsTask = _serviceUnitOfWork.GroupService.GetGroups();
                var brandsTask = _serviceUnitOfWork.ItemBrandService.GetItemBrands();
                var colorsTask = _serviceUnitOfWork.ColorsService.GetColorss();
                var sizesTask = _serviceUnitOfWork.SizesService.GetSizess();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();

                await Task.WhenAll(groupsTask, brandsTask, colorsTask, sizesTask, storesTask);

                GroupsList = groupsTask.Result ?? new List<GroupModelDTO>();
                BrandsList = brandsTask.Result ?? new List<ItemBrandDTO>();
                ColorsList = colorsTask.Result ?? new List<ColorsDTO>();
                SizesList = sizesTask.Result ?? new List<SizesDTO>();
                StoresList = storesTask.Result ?? new List<StoreDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load filter lookups: {ex.Message}");
            }
        }

        private async Task ResolveEcommerceStore()
        {
            try
            {
                ActiveStore = await _serviceUnitOfWork.EcommerceService.GetEcommerceStore(null);

                if (ActiveStore == null)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Ecommerce Branch",
                        "No ecommerce branch found. Please select the branch from the filter panel.");
                    ActiveStore = new EcommerceStoreDTO { Name = "Select Branch" };
                    Filter.StoreId = null;
                }
                else
                {
                    Filter.StoreId = ActiveStore.Id;
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to resolve ecommerce branch: {ex.Message}");
            }
        }

        protected async Task OnGroupChange(object value)
        {
            Filter.SubGroupId = null;
            Filter.DesignId = null;
            DesignsList = new List<DesignModelDTO>();

            if (value is int groupId && groupId > 0)
            {
                try
                {
                    SubGroupsList = (await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId))
                                    ?? new List<SubGroupModelDTO>();
                }
                catch (Exception ex)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load sub-groups: {ex.Message}");
                }
            }
            else
            {
                SubGroupsList = new List<SubGroupModelDTO>();
            }
        }

        protected async Task OnSubGroupChange(object value)
        {
            Filter.DesignId = null;

            if (value is int subGroupId && subGroupId > 0)
            {
                try
                {
                    DesignsList = (await _serviceUnitOfWork.DesignService.LoadDesignsBySubGroup(subGroupId))
                                  ?? new List<DesignModelDTO>();
                }
                catch (Exception ex)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load designs: {ex.Message}");
                }
            }
            else
            {
                DesignsList = new List<DesignModelDTO>();
            }
        }

        protected async Task ApplyFilters()
        {
            if (!Filter.StoreId.HasValue || Filter.StoreId.Value <= 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Ecommerce Branch", "Please select an ecommerce branch.");
                return;
            }

            ActiveStore = await _serviceUnitOfWork.EcommerceService.GetEcommerceStore(Filter.StoreId);
            await LoadItems();
        }

        protected async Task ClearFilters()
        {
            Filter = new EcommerceFilterRequestDTO
            {
                StoreId = (ActiveStore != null && ActiveStore.Id > 0) ? ActiveStore.Id : (int?)null
            };
            SubGroupsList = new List<SubGroupModelDTO>();
            DesignsList = new List<DesignModelDTO>();
            await LoadItems();
        }

        private async Task LoadItems()
        {
            try
            {
                IsLoading = true;
                ItemsList = await _serviceUnitOfWork.EcommerceService.GetEcommerceItems(Filter);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load ecommerce items: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected string GetStoreDisplayName()
            => ActiveStore == null || ActiveStore.Id == 0
                ? "N/A"
                : string.IsNullOrWhiteSpace(ActiveStore.Name)
                    ? $"Branch #{ActiveStore.Id}"
                    : ActiveStore.Name;

        public void Dispose()
        {
            ItemsGrid?.Dispose();
        }
    }
}