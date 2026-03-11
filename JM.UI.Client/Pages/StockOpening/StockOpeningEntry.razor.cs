using JM.UI.Client.Shared;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.ItemFeatures;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.StockOpening;
using JM.UI.Service.UnitOfWork;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Radzen.Blazor;

namespace JM.UI.Client.Pages.StockOpening
{
    public partial class StockOpeningEntryComponent : ComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public NotificationService notificationService { get; set; } = default!;

        protected string PageTitleOverride { get; set; } = "Add Stock Opening";

        // ─── Data ───────────────────────────────────────────────────────
        protected StockOpeningEntryDTO StockOpening { get; set; } = new();
        protected StockOpeningItemDTO CurrentItem { get; set; } = new();
        
        // ─── Lookup Data ────────────────────────────────────────────────
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();
        protected IEnumerable<ItemDTO> Items { get; set; } = new List<ItemDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();

        protected IEnumerable<ItemBrandDTO> Brands { get; set; } = new List<ItemBrandDTO>();
        protected IEnumerable<ItemFeatureDTO> Features { get; set; } = new List<ItemFeatureDTO>();
        protected IEnumerable<ItemOriginDTO> Origins { get; set; } = new List<ItemOriginDTO>();

        // ─── UI State ───────────────────────────────────────────────────
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsNewItemMode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;
        protected RadzenDataGrid<StockOpeningItemDTO> ItemsGrid = default!;

        protected List<string> ProductTypes = new()
        {
            "Sell Product", "Raw Material", "Both", "Consume", "Combo Package"
        };


        protected override async Task OnInitializedAsync()
        {
            PageTitleOverride = "Add Stock Opening";
            
            await LoadLookupData();
            InitializeOpening();
        }

        private void InitializeOpening()
        {
            StockOpening = new StockOpeningEntryDTO
            {
                TransectionDate = DateTime.Now,
                // created by handled in backed ideally or from token
            };

            CurrentItem = CreateNewItem();

            if (Stores != null && Stores.Any())
            {
                var central = Stores.FirstOrDefault(s => s.Name.Contains("Central", StringComparison.OrdinalIgnoreCase));
                if (central != null) StockOpening.StoreId = central.Id;
                else StockOpening.StoreId = Stores.First().Id;
            }
        }

        protected StockOpeningItemDTO CreateNewItem() => new()
        {
            IsSaleable = true,
            Quantity = 1,
            IsNewItem = false,
            CountStockByColor = false,
            CountStockBySize = false
        };

        private async Task LoadLookupData()
        {
            try
            {
                Stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
                Groups = await _serviceUnitOfWork.GroupService.GetGroups() ?? new List<GroupModelDTO>();
                Colors = await _serviceUnitOfWork.ColorsService.GetColorss() ?? new List<ColorsDTO>();
                Sizes = await _serviceUnitOfWork.SizesService.GetSizess() ?? new List<SizesDTO>();
                Units = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits() ?? new List<MesurementUnitModelDTO>();
                Brands = await _serviceUnitOfWork.ItemBrandService.GetItemBrands() ?? new List<ItemBrandDTO>();
                Origins = await _serviceUnitOfWork.ItemOriginService.GetItemOrigins() ?? new List<ItemOriginDTO>();
                AvailableItems = await _serviceUnitOfWork.ItemService.GetItems() ?? new List<ItemDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        // ==========================================
        // UI Actions
        // ==========================================

        protected async Task SearchByBarcode()
        {
            if (string.IsNullOrWhiteSpace(BarcodeSearchText)) return;

            var existingItem = AvailableItems.FirstOrDefault(i => i.Barcode == BarcodeSearchText);
            if (existingItem != null)
            {
                IsNewItemMode = false;
                DisableItemFields = true;
                
                CurrentItem.ItemId = existingItem.Id;
                CurrentItem.ItemName = existingItem.Name;
                CurrentItem.Barcode = existingItem.Barcode;
                CurrentItem.TradePrice = existingItem.PurchasePrice ?? 0;
                CurrentItem.SalePrice = existingItem.SalePrice ?? 0;
                CurrentItem.ColorId = existingItem.ColorId;
                CurrentItem.SizeId = existingItem.SizeId;
                
                notificationService.Notify(NotificationSeverity.Success, "Item Found", $"Item {existingItem.Name} loaded.");
            }
            else
            {
                try
                {
                    IsNewItemMode = true;
                    DisableItemFields = false;
                    CurrentItem.Barcode = BarcodeSearchText;
                    
                    if (AvailableItems != null && AvailableItems.Any())
                        CurrentItem.ItemId = AvailableItems.Max(i => i.Id) + 1;
                    else
                        CurrentItem.ItemId = 1;

                    notificationService.Notify(NotificationSeverity.Info, "New Item", "Barcode not found, ready to add new item.");
                }
                catch (Exception ex)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error generating ID", ex.Message);
                }
            }
        }

        protected void OnItemSelected(object value)
        {
            if (value is int itemId)
            {
                var existingItem = AvailableItems.FirstOrDefault(i => i.Id == itemId);
                if (existingItem != null)
                {
                    IsNewItemMode = false;
                    DisableItemFields = true;
                    
                    CurrentItem.ItemId = existingItem.Id;
                    CurrentItem.ItemName = existingItem.Name;
                    CurrentItem.Barcode = existingItem.Barcode;
                    CurrentItem.TradePrice = existingItem.PurchasePrice ?? 0;
                    CurrentItem.SalePrice = existingItem.SalePrice ?? 0;
                    CurrentItem.ColorId = existingItem.ColorId;
                    CurrentItem.SizeId = existingItem.SizeId;
                    
                    BarcodeSearchText = existingItem.Barcode;
                }
            }
        }

        protected async Task LoadSubGroups(object value)
        {
            if (value is int groupId)
            {
                SubGroups = await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId) ?? new List<SubGroupModelDTO>();
            }
        }

        protected void AddItemToGrid()
        {
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            var itemToAdd = new StockOpeningItemDTO
            {
                ItemId = CurrentItem.ItemId,
                ItemName = CurrentItem.ItemName,
                Barcode = CurrentItem.Barcode,
                ColorId = CurrentItem.ColorId,
                ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name,
                SizeId = CurrentItem.SizeId,
                SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name,
                Quantity = CurrentItem.Quantity,
                TradePrice = CurrentItem.TradePrice,
                MRP = CurrentItem.SalePrice,
                IsNewItem = IsNewItemMode,
                GroupId = CurrentItem.GroupId,
                SubGroupId = CurrentItem.SubGroupId,
                MesurementUnitId = CurrentItem.MesurementUnitId,
                BrandId = CurrentItem.BrandId,
                OriginId = CurrentItem.OriginId,
                SalePrice = CurrentItem.SalePrice,
                IsSaleable = CurrentItem.IsSaleable,
                CountStockByColor = CurrentItem.CountStockByColor,
                CountStockBySize = CurrentItem.CountStockBySize,
                ShadeNo = CurrentItem.ShadeNo,
                ProductType = string.IsNullOrWhiteSpace(CurrentItem.ProductType?.ToString()) ? 1 : 1
            };

            StockOpening.Items.Add(itemToAdd);
            ItemsGrid?.Reload();

            // Reset
            CurrentItem = CreateNewItem();
            BarcodeSearchText = string.Empty;
            IsNewItemMode = false;
            DisableItemFields = false;
            
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added to list.");
        }

        protected void DeleteItem(StockOpeningItemDTO item)
        {
            StockOpening.Items.Remove(item);
            ItemsGrid?.Reload();
        }

        private (bool IsValid, string Message) ValidateCurrentItem()
        {
            if (IsNewItemMode)
            {
                if (string.IsNullOrWhiteSpace(CurrentItem.ItemName)) return (false, "Item name is required for new items.");
                if (!CurrentItem.SubGroupId.HasValue || CurrentItem.SubGroupId.Value == 0) return (false, "Sub-group is required for new items.");
            }
            else
            {
                if (CurrentItem.ItemId == 0) return (false, "Please select or search an item first.");
            }

            if (string.IsNullOrWhiteSpace(CurrentItem.Barcode)) return (false, "Barcode is required.");
            if (CurrentItem.Quantity <= 0) return (false, "Quantity must be greater than 0.");
            if (CurrentItem.TradePrice <= 0) return (false, "Trade price (Purchase price) must be greater than 0.");
            
            if (CurrentItem.IsSaleable)
            {
                if (CurrentItem.SalePrice <= 0) return (false, "Sale price is required for saleable items.");
            }

            if (StockOpening.Items.Any(i => i.Barcode == CurrentItem.Barcode))
                return (false, "Item with this barcode already added to the list.");

            return (true, string.Empty);
        }

        protected async Task SaveStockOpening()
        {
            if (StockOpening.StoreId == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a store.");
                return;
            }

            if (!StockOpening.Items.Any())
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please add at least one item.");
                return;
            }

            try
            {
                IsProcessing = true;
                
                var result = await _serviceUnitOfWork.StockOpeningService.InsertStockOpening(StockOpening);

                if (result != null && result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Stock opening saved successfully.");
                    NavigationManager.NavigateTo("/StockOpeningList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result?.Message ?? "Failed to save stock opening.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Save failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/StockOpeningList");
        }
    }
}
