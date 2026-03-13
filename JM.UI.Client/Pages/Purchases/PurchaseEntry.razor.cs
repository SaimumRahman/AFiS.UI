using JM.UI.Client.Pages.Dialog;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Entities.Model.ItemFeatures;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.ItemWiseFeature;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Purchases
{
    public partial class PurchaseEntryComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }
        [Parameter] public int? DraftId { get; set; }
        protected bool IsDraftMode => DraftId.HasValue && DraftId.Value > 0;
        protected bool IsProductNameFieldChange { get; set; } = false;

        // ─── Image Upload ────────────────────────────────────────────
        protected string CurrentItemImageBase64 { get; set; } = string.Empty;
        protected string CurrentItemImageMimeType { get; set; } = "image/jpeg";

        // ─── Catalogue ───────────────────────────────────────────────
        protected IEnumerable<ItemCatalogueDTO> Catalogues { get; set; } = new List<ItemCatalogueDTO>();
        protected string CatalogueSearchText { get; set; } = string.Empty;
        protected List<ItemCatalogueDTO> CatalogueSuggestions { get; set; } = new();
        protected int? SelectedCatalogueId { get; set; }
        protected bool IsNewCatalogue { get; set; } = false;

        // ─── Purchase Data ──────────────────────────────────────────────
        protected PurchaseDTO Purchase { get; set; } = new();
        protected List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
        protected PurchaseItemDTO CurrentItem { get; set; } = new();
        protected PurchaseItemDTO? _editingItem = null;

        // ─── Preview Items (inner editable grid) ────────────────────────
        protected List<PurchaseItemDTO> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<PurchaseItemDTO> PreviewGrid = default!;

        // ─── Shared price/qty fields that propagate to all preview rows ──
        protected decimal SharedQty { get; set; } = 1;
        protected decimal SharedPurchasePrice { get; set; } = 0;
        protected decimal? SharedOtherCost { get; set; }
        protected decimal? SharedCarryingCost { get; set; }
        protected decimal? SharedVatPercentage { get; set; }
        protected decimal? SharedSalePrice { get; set; }

        // ─── Barcode input (text only, no dropdown) ─────────────────────
        // Stores the user-typed barcode text into CurrentItem.Barcode.
        // Actual search is fired from OnBarcodeInputChanged() (Change event on
        // the razor textbox) so it only runs when the user finishes typing
        // (on blur/enter), not on every keystroke via the setter.
        private string _barcodeInputText = string.Empty;
        protected string BarcodeInputText
        {
            get => _barcodeInputText;
            set
            {
                if (_barcodeInputText != value)
                {
                    _barcodeInputText = value;
                    CurrentItem.Barcode = value;
                }
            }
        }

        // Called from the razor Change event on the Barcode textbox.
        // Barcode alone is not enough — Color AND Size must also be selected.
        // If they are missing, a warning guides the user; no search is fired.
        protected async Task OnBarcodeInputChanged(string value)
        {
            BarcodeInputText = value;

            if (string.IsNullOrWhiteSpace(value))
            {
                PreviewItems.Clear();
                PreviewGrid?.Reload();
                DisableItemFields = false;
                IsNewItemMode = false;
                StateHasChanged();
                return;
            }

            // Guard — inform user what else is needed; TryAutoSearchAsync will
            // also guard, but showing the message here gives earlier feedback.
            if (!CurrentItem.ColorId.HasValue || !CurrentItem.SizeId.HasValue)
            {
                notificationService.Notify(NotificationSeverity.Info, "Select Color & Size",
                    "Barcode entered. Now select Color and Size to load items.");
                StateHasChanged();
                return;
            }

            await TryAutoSearchAsync();
        }

        // ─── Lookup Data ────────────────────────────────────────────────
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();
        protected IEnumerable<DesignModelDTO> Designs { get; set; } = new List<DesignModelDTO>();
        protected IEnumerable<ItemDTO> Items { get; set; } = new List<ItemDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();

        // ─── Brand / Origin / Features Lookups ─────────────────────────
        protected IEnumerable<ItemBrandDTO> Brands { get; set; } = new List<ItemBrandDTO>();
        protected IEnumerable<ItemFeatureDTO> Features { get; set; } = new List<ItemFeatureDTO>();
        protected IEnumerable<ItemOriginDTO> Origins { get; set; } = new List<ItemOriginDTO>();

        protected string BrandSearchText { get; set; } = string.Empty;
        protected List<ItemBrandDTO> BrandSuggestions { get; set; } = new List<ItemBrandDTO>();
        protected int? SelectedBrandId { get; set; }
        protected bool IsNewBrand { get; set; } = false;

        protected string OriginSearchText { get; set; } = string.Empty;
        protected IEnumerable<ItemOriginDTO> OriginSuggestions { get; set; } = new List<ItemOriginDTO>();
        protected int? SelectedOriginId { get; set; }
        protected bool IsNewOrigin { get; set; } = false;

        protected IEnumerable<int> SelectedFeatureIds { get; set; } = new List<int>();
        protected List<string> NewFeatureNames { get; set; } = new();
        protected string NewFeatureInput { get; set; } = string.Empty;

        // ─── UI State ───────────────────────────────────────────────────
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Entry" : IsDraftMode ? "Purchase Entry (From Draft)" : "New Purchase Entry";
        protected bool IsNewItemMode { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        // BarcodeSearchText kept for compatibility with Edit/Draft restore
        protected string BarcodeSearchText { get; set; } = string.Empty;

        protected RadzenDataGrid<PurchaseItemDTO> ItemsGrid = default!;

        protected List<string> ProductTypes = new()
        {
            "Sell Product", "Raw Material", "Both", "Consume", "Combo Package"
        };

        // ═══════════════════════════════════════════════════════════════
        // Lifecycle
        // ═══════════════════════════════════════════════════════════════
        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();

            if (IsDraftMode)
                await LoadDraft();
            else if (IsEditMode)
                await LoadPurchase();
            else
                InitializePurchase();
        }

        // ═══════════════════════════════════════════════════════════════
        // Initialization
        // ═══════════════════════════════════════════════════════════════
        private void InitializePurchase()
        {
            Purchase = _serviceUnitOfWork.PurchaseService.CreateNewPurchase();
            PurchaseItems = new List<PurchaseItemDTO>();
            PreviewItems = new List<PurchaseItemDTO>();
            CurrentItem = CreateNewItem();

            if (Stores != null)
            {
                var central = Stores.FirstOrDefault(s => s.Name.Contains("Central", StringComparison.OrdinalIgnoreCase));
                if (central != null) Purchase.StoreId = central.Id;
            }
        }

        private PurchaseItemDTO CreateNewItem() => new()
        {
            IsSaleable = true,
            ProductType = "Sell Product",
            Quantity = 1,
            IsActive = true,
            IsNewItem = false,
            CountStockByColor = false,
            CountStockBySize = false
        };

        // ═══════════════════════════════════════════════════════════════
        // Auto-search by barcode.
        // Called from:
        //   • OnBarcodeInputChanged — user typed a barcode
        //   • OnColorChanged / OnSizeChanged — after GenerateBarcode() has set
        //     CurrentItem.Barcode
        // ALL THREE (barcode text + Color + Size) must be present before an
        // item is searched and added to the preview grid.
        // ═══════════════════════════════════════════════════════════════
        private async Task TryAutoSearchAsync()
        {
            // All three inputs are required
            if (string.IsNullOrWhiteSpace(CurrentItem.Barcode)) return;
            if (!CurrentItem.ColorId.HasValue)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Color Required",
                    "Please select a Color before searching.");
                return;
            }
            if (!CurrentItem.SizeId.HasValue)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Size Required",
                    "Please select a Size before searching.");
                return;
            }

            var fullBarcode = CurrentItem.Barcode;

            // Avoid re-searching the same barcode already in preview
            if (PreviewItems.Any(p => p.Barcode == fullBarcode)) return;

            try
            {
                IsSearchingBarcode = true;
                StateHasChanged();

                var result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(fullBarcode);

                if (result.Found && result.ItemDetails != null)
                {
                    // Item exists — load all variants for the same base item
                    await AddVariantsToPreview(result.ItemDetails, result.itemWiseFeatures ?? new List<ItemWiseFeatureDTO>());
                    DisableItemFields = true;
                    IsNewItemMode = false;
                }
                else if (result.Found && result.Item != null)
                {
                    // Found in purchase history
                    var previewRow = BuildPreviewRowFromPurchaseItem(result.Item);
                    AddOrUpdatePreviewRow(previewRow);
                    DisableItemFields = false;
                }
                else
                {
                    // Not found — create a new item row in preview
                    var newRow = BuildPreviewRowFromCurrentItem();
                    newRow.IsNewItem = true;
                    AddOrUpdatePreviewRow(newRow);
                    DisableItemFields = false;
                    IsNewItemMode = true;
                    notificationService.Notify(NotificationSeverity.Info, "New Item",
                        "Barcode not found. New item row added to preview.");
                }

                ApplySharedPricesToAll();
                await PreviewGrid.Reload();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Search Error", ex.Message);
            }
            finally
            {
                IsSearchingBarcode = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Loads all color/size variants of the found item into the preview grid.
        /// Each row = one variant with its own barcode.
        /// </summary>
        private async Task AddVariantsToPreview(ItemDTO item, List<ItemWiseFeatureDTO> itemWiseFeatures)
        {
            // Get all items with the same design/sub-group so we can show all variants
            IEnumerable<ItemDTO> variants = new List<ItemDTO> { item };

            if (item.SubGroupId.HasValue)
            {
                var subGroupItems = await _serviceUnitOfWork.ItemService.LoadItemsBySubGroup(item.SubGroupId.Value)
                    ?? new List<ItemDTO>();

                // Filter to items sharing the same design (same product family)
                if (item.DesignId.HasValue)
                    variants = subGroupItems.Where(i => i.DesignId == item.DesignId).ToList();
                else
                    variants = new List<ItemDTO> { item };

                if (!variants.Any()) variants = new List<ItemDTO> { item };
            }

            foreach (var variant in variants)
            {
                // Skip if this barcode is already in the main PurchaseItems grid
                if (PurchaseItems.Any(p => p.Barcode == variant.Barcode)) continue;
                // Skip if already in preview
                if (PreviewItems.Any(p => p.Barcode == variant.Barcode)) continue;

                var colorName = Colors.FirstOrDefault(c => c.Id == variant.ColorId)?.Name ?? string.Empty;
                var sizeName = Sizes.FirstOrDefault(s => s.Id == variant.SizeId)?.Name ?? string.Empty;
                var unitName = Units.FirstOrDefault(u => u.Id == variant.MesurementUnitId)?.Name ?? string.Empty;
                var brandName = variant.BrandId.HasValue
                    ? (Brands.FirstOrDefault(b => b.BrandId == variant.BrandId)?.BrandName ?? variant.BrandColor ?? string.Empty)
                    : (variant.BrandColor ?? string.Empty);
                var originName = variant.OriginId.HasValue
                    ? (Origins.FirstOrDefault(o => o.OriginId == variant.OriginId)?.OriginName ?? variant.Origin ?? string.Empty)
                    : (variant.Origin ?? string.Empty);

                var featureIds = itemWiseFeatures
                    .Where(f => f.ItemId == variant.Id)
                    .Select(f => f.FeaturesId).ToList();
                var featuresDisplay = string.Join(", ", Features
                    .Where(f => featureIds.Contains(f.FeatureId))
                    .Select(f => f.FeatureName));

                var row = new PurchaseItemDTO
                {
                    ItemId = variant.Id,
                    ItemName = variant.Name,
                    GroupId = variant.GroupId,
                    SubGroupId = variant.SubGroupId,
                    DesignId = variant.DesignId,
                    ShadeNo = variant.ShadeNo,
                    ColorId = variant.ColorId,
                    ColorName = colorName,
                    SizeId = variant.SizeId,
                    SizeName = sizeName,
                    BrandId = variant.BrandId,
                    BrandName = brandName,
                    OriginId = variant.OriginId,
                    OriginName = originName,
                    FeatureIds = featureIds,
                    FeaturesDisplay = featuresDisplay,
                    Barcode = variant.Barcode ?? CurrentItem.Barcode,
                    MesurementUnitId = variant.MesurementUnitId,
                    MesurementUnitName = unitName,
                    CatalogueId = variant.CatalogueId,
                    CatalogueName = variant.Catalogue,
                    IsNewItem = false,
                    IsSaleable = CurrentItem.IsSaleable,
                    IsConsume = CurrentItem.IsConsume,
                    ProductType = variant.ProductType ?? CurrentItem.ProductType,
                    MaterialType = variant.MaterialType,
                    CountStockByColor = variant.CountStockByColor,
                    CountStockBySize = variant.CountStockBySize,
                    IsActive = true,
                    // Prices — start from existing item's prices; user can override
                    Quantity = SharedQty > 0 ? SharedQty : 1,
                    PurchasePrice = SharedPurchasePrice > 0 ? SharedPurchasePrice : (variant.PurchasePrice ?? 0),
                    OtherCost = SharedOtherCost,
                    CarryingCost = SharedCarryingCost,
                    VatPercentage = SharedVatPercentage ?? variant.VatPercentage,
                    SalePrice = SharedSalePrice.HasValue && SharedSalePrice > 0 ? SharedSalePrice : variant.SalePrice,
                    ImageBase64 = CurrentItemImageBase64,
                };

                row.TotalAmount = CalculateItemTotalFor(row);
                PreviewItems.Add(row);
            }
        }

        private PurchaseItemDTO BuildPreviewRowFromCurrentItem()
        {
            var colorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? string.Empty;
            var sizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? string.Empty;
            var unitName = Units.FirstOrDefault(u => u.Id == CurrentItem.MesurementUnitId)?.Name ?? string.Empty;

            return new PurchaseItemDTO
            {
                ItemId = CurrentItem.ItemId,
                ItemName = CurrentItem.ItemName,
                GroupId = CurrentItem.GroupId,
                SubGroupId = CurrentItem.SubGroupId,
                DesignId = CurrentItem.DesignId,
                ShadeNo = CurrentItem.ShadeNo,
                ColorId = CurrentItem.ColorId,
                ColorName = colorName,
                SizeId = CurrentItem.SizeId,
                SizeName = sizeName,
                BrandId = CurrentItem.BrandId,
                BrandName = BrandSearchText,
                OriginId = CurrentItem.OriginId,
                OriginName = OriginSearchText,
                FeatureIds = SelectedFeatureIds.ToList(),
                FeaturesDisplay = string.Join(", ", Features
                    .Where(f => SelectedFeatureIds.Contains(f.FeatureId)).Select(f => f.FeatureName)),
                Barcode = CurrentItem.Barcode,
                MesurementUnitId = CurrentItem.MesurementUnitId,
                MesurementUnitName = unitName,
                CatalogueId = CurrentItem.CatalogueId,
                CatalogueName = CatalogueSearchText,
                IsNewItem = IsNewItemMode,
                IsSaleable = CurrentItem.IsSaleable,
                IsConsume = CurrentItem.IsConsume,
                ProductType = CurrentItem.ProductType,
                CountStockByColor = CurrentItem.CountStockByColor,
                CountStockBySize = CurrentItem.CountStockBySize,
                IsActive = true,
                Quantity = SharedQty > 0 ? SharedQty : 1,
                PurchasePrice = SharedPurchasePrice,
                OtherCost = SharedOtherCost,
                CarryingCost = SharedCarryingCost,
                VatPercentage = SharedVatPercentage,
                SalePrice = SharedSalePrice,
                ImageBase64 = CurrentItemImageBase64,
            };
        }

        private PurchaseItemDTO BuildPreviewRowFromPurchaseItem(PurchaseItemDTO src)
        {
            var row = new PurchaseItemDTO
            {
                ItemId = src.ItemId,
                ItemName = src.ItemName,
                GroupId = src.GroupId,
                GroupName = src.GroupName,
                SubGroupId = src.SubGroupId,
                SubGroupName = src.SubGroupName,
                DesignId = src.DesignId,
                ShadeNo = src.ShadeNo,
                ColorId = src.ColorId,
                ColorName = src.ColorName,
                SizeId = src.SizeId,
                SizeName = src.SizeName,
                BrandId = src.BrandId,
                BrandName = src.BrandName,
                OriginId = src.OriginId,
                OriginName = src.OriginName,
                FeatureIds = src.FeatureIds ?? new List<int>(),
                FeaturesDisplay = src.FeaturesDisplay,
                Barcode = src.Barcode,
                MesurementUnitId = src.MesurementUnitId,
                MesurementUnitName = src.MesurementUnitName,
                CatalogueId = src.CatalogueId,
                CatalogueName = src.CatalogueName,
                IsNewItem = false,
                IsSaleable = src.IsSaleable,
                IsConsume = src.IsConsume,
                ProductType = src.ProductType,
                MaterialType = src.MaterialType,
                CountStockByColor = src.CountStockByColor,
                CountStockBySize = src.CountStockBySize,
                IsActive = true,
                Quantity = SharedQty > 0 ? SharedQty : 1,
                PurchasePrice = SharedPurchasePrice > 0 ? SharedPurchasePrice : src.PurchasePrice,
                OtherCost = SharedOtherCost ?? src.OtherCost,
                CarryingCost = SharedCarryingCost ?? src.CarryingCost,
                VatPercentage = SharedVatPercentage ?? src.VatPercentage,
                SalePrice = SharedSalePrice.HasValue && SharedSalePrice > 0 ? SharedSalePrice : src.SalePrice,
                ImageBase64 = CurrentItemImageBase64,
            };
            row.TotalAmount = CalculateItemTotalFor(row);
            return row;
        }

        private void AddOrUpdatePreviewRow(PurchaseItemDTO row)
        {
            var existing = PreviewItems.FirstOrDefault(p => p.Barcode == row.Barcode);
            if (existing != null)
            {
                var idx = PreviewItems.IndexOf(existing);
                PreviewItems[idx] = row;
            }
            else
            {
                PreviewItems.Add(row);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Shared price propagation handlers
        // ═══════════════════════════════════════════════════════════════
        protected void OnSharedQtyChanged(decimal value)
        {
            SharedQty = value;
            foreach (var item in PreviewItems) { item.Quantity = value; item.TotalAmount = CalculateItemTotalFor(item); }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnSharedPurchasePriceChanged(decimal value)
        {
            SharedPurchasePrice = value;
            foreach (var item in PreviewItems) { item.PurchasePrice = value; item.TotalAmount = CalculateItemTotalFor(item); }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnSharedOtherCostChanged(decimal? value)
        {
            SharedOtherCost = value;
            foreach (var item in PreviewItems) { item.OtherCost = value; item.TotalAmount = CalculateItemTotalFor(item); }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnSharedCarryingCostChanged(decimal? value)
        {
            SharedCarryingCost = value;
            foreach (var item in PreviewItems) { item.CarryingCost = value; item.TotalAmount = CalculateItemTotalFor(item); }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnSharedVatChanged(decimal? value)
        {
            SharedVatPercentage = value;
            foreach (var item in PreviewItems) { item.VatPercentage = value; item.TotalAmount = CalculateItemTotalFor(item); }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnSharedSalePriceChanged(decimal? value)
        {
            SharedSalePrice = value;
            foreach (var item in PreviewItems) { item.SalePrice = value; }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Per-row preview grid cell change handlers (individual edits)
        // ═══════════════════════════════════════════════════════════════
        protected void OnPreviewQtyChanged(PurchaseItemDTO item, decimal value)
        {
            item.Quantity = value;
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void OnPreviewPurchasePriceChanged(PurchaseItemDTO item, decimal value)
        {
            item.PurchasePrice = value;
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void OnPreviewOtherCostChanged(PurchaseItemDTO item, decimal? value)
        {
            item.OtherCost = value;
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void OnPreviewCarryingCostChanged(PurchaseItemDTO item, decimal? value)
        {
            item.CarryingCost = value;
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void OnPreviewVatChanged(PurchaseItemDTO item, decimal? value)
        {
            item.VatPercentage = value;
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void OnPreviewSalePriceChanged(PurchaseItemDTO item, decimal? value)
        {
            item.SalePrice = value;
            StateHasChanged();
        }

        protected void OnPreviewRowUpdate(PurchaseItemDTO item)
        {
            item.TotalAmount = CalculateItemTotalFor(item);
            StateHasChanged();
        }

        protected void RemoveFromPreview(PurchaseItemDTO item)
        {
            PreviewItems.Remove(item);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Helper: calculate total for a single item
        // ═══════════════════════════════════════════════════════════════
        private decimal CalculateItemTotalFor(PurchaseItemDTO item)
        {
            // Delegate to service if available, otherwise inline formula
            try { return _serviceUnitOfWork.PurchaseService.CalculateItemTotal(item); }
            catch
            {
                var base_ = item.Quantity * item.PurchasePrice;
                var other = item.OtherCost ?? 0;
                var carry = item.CarryingCost ?? 0;
                var vatAmt = (item.VatPercentage.HasValue)
                    ? (base_ + other + carry) * item.VatPercentage.Value / 100m
                    : 0;
                return base_ + other + carry + vatAmt;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Apply shared prices to all preview rows
        // ═══════════════════════════════════════════════════════════════
        private void ApplySharedPricesToAll()
        {
            foreach (var item in PreviewItems)
            {
                if (SharedQty > 0) item.Quantity = SharedQty;
                if (SharedPurchasePrice > 0) item.PurchasePrice = SharedPurchasePrice;
                if (SharedOtherCost.HasValue) item.OtherCost = SharedOtherCost;
                if (SharedCarryingCost.HasValue) item.CarryingCost = SharedCarryingCost;
                if (SharedVatPercentage.HasValue) item.VatPercentage = SharedVatPercentage;
                if (SharedSalePrice.HasValue) item.SalePrice = SharedSalePrice;
                item.TotalAmount = CalculateItemTotalFor(item);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Catalogue Handlers
        // ═══════════════════════════════════════════════════════════════
        protected void OnCatalogueTextChanged(object value)
        {
            var text = value?.ToString();
            CatalogueSearchText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                CatalogueSuggestions = new List<ItemCatalogueDTO>();
                SelectedCatalogueId = null;
                IsNewCatalogue = false;
                CurrentItem.CatalogueId = null;
                CurrentItem.CatalogueName = null;
                StateHasChanged();
                return;
            }

            CatalogueSuggestions = Catalogues
                .Where(c => c.CatalogueName.Contains(text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var exact = Catalogues.FirstOrDefault(c =>
                c.CatalogueName.Equals(text, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
            {
                SelectedCatalogueId = exact.CatalogueId;
                CurrentItem.CatalogueId = exact.CatalogueId;
                CurrentItem.CatalogueName = exact.CatalogueName;
                IsNewCatalogue = false;
            }
            else
            {
                SelectedCatalogueId = null;
                CurrentItem.CatalogueId = null;
                CurrentItem.CatalogueName = text;
                IsNewCatalogue = true;
            }

            GenerateProductName();
            StateHasChanged();
        }

        protected void OnCatalogueSelected(object value)
        {
            if (value is ItemCatalogueDTO cat)
            {
                SelectedCatalogueId = cat.CatalogueId;
                CurrentItem.CatalogueId = cat.CatalogueId;
                CurrentItem.CatalogueName = cat.CatalogueName;
                CatalogueSearchText = cat.CatalogueName;
                IsNewCatalogue = false;
                GenerateProductName();
            }
        }

        private void ResetItemFormSelections()
        {
            BrandSearchText = string.Empty;
            SelectedBrandId = null;
            IsNewBrand = false;
            OriginSearchText = string.Empty;
            SelectedOriginId = null;
            IsNewOrigin = false;
            SelectedFeatureIds = new List<int>();
            NewFeatureNames = new();
            NewFeatureInput = string.Empty;
            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CatalogueSearchText = string.Empty;
            SelectedCatalogueId = null;
            IsNewCatalogue = false;
        }

        // ═══════════════════════════════════════════════════════════════
        // Data Loading
        // ═══════════════════════════════════════════════════════════════
        private async Task LoadLookupData()
        {
            try
            {
                Suppliers = await LoadSuppliers();
                Stores = await LoadStores();
                Groups = await LoadGroups();
                Colors = await LoadColors();
                Sizes = await LoadSizes();
                Units = await LoadUnits();
                Brands = await LoadBrands();
                Features = await LoadFeatures();
                Origins = await LoadOrigins();
                AvailableItems = await LoadAllItems();
                Catalogues = await LoadCatalogues();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        private async Task<IEnumerable<ItemCatalogueDTO>> LoadCatalogues() =>
            await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues() ?? new List<ItemCatalogueDTO>();
        private async Task<IEnumerable<ItemDTO>> LoadAllItems() =>
            await _serviceUnitOfWork.ItemService.GetItems() ?? new List<ItemDTO>();
        private async Task<IEnumerable<SupplierModelDTO>> LoadSuppliers() =>
            await _serviceUnitOfWork.SupplierService.GetSuppliers() ?? new List<SupplierModelDTO>();
        private async Task<IEnumerable<StoreDTO>> LoadStores() =>
            await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
        private async Task<IEnumerable<GroupModelDTO>> LoadGroups() =>
            await _serviceUnitOfWork.GroupService.GetGroups() ?? new List<GroupModelDTO>();
        private async Task<IEnumerable<ColorsDTO>> LoadColors() =>
            await _serviceUnitOfWork.ColorsService.GetColorss() ?? new List<ColorsDTO>();
        private async Task<IEnumerable<SizesDTO>> LoadSizes() =>
            await _serviceUnitOfWork.SizesService.GetSizess() ?? new List<SizesDTO>();
        private async Task<IEnumerable<MesurementUnitModelDTO>> LoadUnits() =>
            await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits() ?? new List<MesurementUnitModelDTO>();
        private async Task<IEnumerable<ItemBrandDTO>> LoadBrands() =>
            await _serviceUnitOfWork.ItemBrandService.GetItemBrands() ?? new List<ItemBrandDTO>();
        private async Task<IEnumerable<ItemFeatureDTO>> LoadFeatures() =>
            await _serviceUnitOfWork.ItemFeatureService.GetItemFeatures() ?? new List<ItemFeatureDTO>();
        private async Task<IEnumerable<ItemOriginDTO>> LoadOrigins() =>
            await _serviceUnitOfWork.ItemOriginService.GetItemOrigins() ?? new List<ItemOriginDTO>();

        protected async Task LoadPurchase()
        {
            try
            {
                IsLoading = true;
                var purchase = await _serviceUnitOfWork.PurchaseService.GetPurchaseById(Id!.Value);
                if (purchase == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase not found.");
                    NavigationManager.NavigateTo("/PurchaseList");
                    return;
                }

                Purchase = purchase;
                PurchaseItems = purchase.PurchaseItems?.ToList() ?? new List<PurchaseItemDTO>();
                PreviewItems = new List<PurchaseItemDTO>();
                CurrentItem = CreateNewItem();
                CalculateTotals();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        protected async Task LoadDraft()
        {
            try
            {
                IsLoading = true;
                var draft = await _serviceUnitOfWork.PurchaseService.GetPurchaseDraftById(DraftId!.Value);
                if (draft == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Draft not found.");
                    NavigationManager.NavigateTo("/PurchaseDraftList");
                    return;
                }

                Purchase = new PurchaseDTO
                {
                    SupplierId = draft.SupplierId,
                    StoreId = draft.StoreId,
                    PurchaseDate = draft.PurchaseDate ?? DateTime.Now,
                    BillInvoiceNumber = draft.BillInvoiceNumber,
                    BillInvoiceName = draft.BillInvoiceName,
                    IsVatIncluded = draft.IsVatIncluded,
                    TotalAmount = draft.TotalAmount,
                    DiscountAmount = draft.DiscountAmount,
                    VatAmount = draft.VatAmount,
                    NetAmount = draft.NetAmount,
                    PaidAmount = draft.PaidAmount,
                    CreatedDate = draft.CreatedDate,
                    DueAmount = draft.DueAmount,
                    Remarks = draft.Remarks
                };

                PurchaseItems = draft.DraftItems.Select(di => new PurchaseItemDTO
                {
                    ItemId = di.ItemId ?? 0,
                    ItemName = di.ItemName,
                    GroupId = di.GroupId,
                    GroupName = di.GroupName,
                    SubGroupId = di.SubGroupId,
                    SubGroupName = di.SubGroupName,
                    ShadeNo = di.ShadeNo,
                    ColorId = di.ColorId,
                    ColorName = di.ColorName,
                    SizeId = di.SizeId,
                    SizeName = di.SizeName,
                    BrandId = di.BrandId,
                    BrandName = di.BrandName,
                    OriginId = di.OriginId,
                    OriginName = di.OriginName,
                    FeatureIds = di.FeatureIds ?? new List<int>(),
                    FeaturesDisplay = di.FeaturesDisplay ?? string.Empty,
                    Barcode = di.Barcode,
                    Quantity = di.Quantity,
                    PurchasePrice = di.PurchasePrice,
                    ProductPricePercentage = di.ProductPricePercentage,
                    OtherCost = di.OtherCost,
                    CarryingCost = di.CarryingCost,
                    TransportCost = di.TransportCost,
                    OperationalCost = di.OperationalCost,
                    VatPercentage = di.VatPercentage,
                    VatAmount = di.VatAmount,
                    TotalAmount = di.TotalAmount,
                    IsSaleable = di.IsSaleable,
                    IsConsume = di.IsConsume,
                    SalePrice = di.SalePrice,
                    ProductType = di.ProductType,
                    MaterialType = di.MaterialType,
                    MesurementUnitId = di.MesurementUnitId,
                    CountStockByColor = di.CountStockByColor,
                    CountStockBySize = di.CountStockBySize,
                    IsNewItem = di.IsNewItem,
                    IsActive = di.IsActive,
                    MesurementUnitName = di.MesurementUnitName,
                }).ToList();

                PreviewItems = new List<PurchaseItemDTO>();

                var firstItem = PurchaseItems.FirstOrDefault();
                if (firstItem?.GroupId.HasValue == true)
                    SubGroups = await LoadSubGroupsByGroup(firstItem.GroupId.Value);
                if (firstItem?.SubGroupId.HasValue == true)
                {
                    Items = await LoadItemsBySubGroup(firstItem.SubGroupId.Value);
                    Designs = await LoadDesignsBySubGroup(firstItem.SubGroupId.Value);
                }

                CurrentItem = CreateNewItem();
                CalculateTotals();

                notificationService.Notify(NotificationSeverity.Info, "Draft Loaded",
                    $"Draft '{draft.DraftName}' loaded successfully");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load draft: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseDraftList");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Image Upload
        // ═══════════════════════════════════════════════════════════════
        protected async Task OnItemImageSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;

            const long maxBytes = 2 * 1024 * 1024;
            if (file.Size > maxBytes)
            {
                notificationService.Notify(NotificationSeverity.Warning, "File Too Large", "Please select an image under 2 MB.");
                return;
            }

            try
            {
                using var stream = file.OpenReadStream(maxBytes);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                CurrentItemImageMimeType = file.ContentType;
                CurrentItemImageBase64 = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
                CurrentItem.ImageBase64 = CurrentItemImageBase64;

                // Also push image to existing preview rows
                foreach (var row in PreviewItems)
                    row.ImageBase64 = CurrentItemImageBase64;

                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Upload Error", $"Failed to read image: {ex.Message}");
            }
        }

        protected void ClearItemImage()
        {
            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CurrentItem.ImageBase64 = null;
            foreach (var row in PreviewItems) row.ImageBase64 = null;
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Brand Auto-Complete Handlers
        // ═══════════════════════════════════════════════════════════════
        protected void OnBrandTextChanged(object value)
        {
            var text = value?.ToString();
            BrandSearchText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                BrandSuggestions = new List<ItemBrandDTO>();
                SelectedBrandId = null;
                IsNewBrand = false;
                CurrentItem.BrandId = null;
                return;
            }

            BrandSuggestions = Brands
                .Where(b => b.BrandName.Contains(text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var exactMatch = Brands.FirstOrDefault(b =>
                b.BrandName.Equals(text, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                SelectedBrandId = exactMatch.BrandId;
                CurrentItem.BrandId = exactMatch.BrandId;
                IsNewBrand = false;
            }
            else
            {
                SelectedBrandId = null;
                CurrentItem.BrandId = null;
                IsNewBrand = true;
            }

            GenerateProductName();
        }

        protected void OnBrandSelected(object value)
        {
            if (value is ItemBrandDTO brand)
            {
                SelectedBrandId = brand.BrandId;
                CurrentItem.BrandId = brand.BrandId;
                BrandSearchText = brand.BrandName;
                IsNewBrand = false;
                GenerateProductName();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Origin Auto-Complete Handlers
        // ═══════════════════════════════════════════════════════════════
        protected void OnOriginTextChanged(object value)
        {
            var text = value?.ToString();
            OriginSearchText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                OriginSuggestions = new List<ItemOriginDTO>();
                SelectedOriginId = null;
                IsNewOrigin = false;
                CurrentItem.OriginName = null;
                CurrentItem.OriginId = null;
                return;
            }

            OriginSuggestions = Origins
                .Where(o => o.OriginName.Contains(text, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var exactMatch = Origins.FirstOrDefault(o =>
                o.OriginName.Equals(text, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                SelectedOriginId = exactMatch.OriginId;
                CurrentItem.OriginId = exactMatch.OriginId;
                CurrentItem.OriginName = exactMatch.OriginName;
                IsNewOrigin = false;
            }
            else
            {
                SelectedOriginId = null;
                CurrentItem.OriginId = null;
                CurrentItem.OriginName = text;
                IsNewOrigin = true;
            }

            GenerateProductName();
        }

        protected void OnOriginSelected(object value)
        {
            if (value is ItemOriginDTO origin)
            {
                SelectedOriginId = origin.OriginId;
                CurrentItem.OriginId = origin.OriginId;
                CurrentItem.OriginName = origin.OriginName;
                OriginSearchText = origin.OriginName;
                IsNewOrigin = false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Features Multi-Select Handlers
        // ═══════════════════════════════════════════════════════════════
        protected void AddNewFeature()
        {
            var trimmed = NewFeatureInput?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return;

            if (Features.Any(f => f.FeatureName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                var existing = Features.First(f => f.FeatureName.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
                var ids = SelectedFeatureIds.ToList();
                if (!ids.Contains(existing.FeatureId))
                {
                    ids.Add(existing.FeatureId);
                    SelectedFeatureIds = ids;
                }
                notificationService.Notify(NotificationSeverity.Info, "Feature exists",
                    $"'{trimmed}' already exists and was selected.");
            }
            else if (!NewFeatureNames.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                NewFeatureNames.Add(trimmed);
            }

            NewFeatureInput = string.Empty;
            StateHasChanged();
        }

        protected void RemoveNewFeature(string name)
        {
            NewFeatureNames.Remove(name);
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Save – Brand / Origin / Features pre-save resolution
        // ═══════════════════════════════════════════════════════════════
        private async Task<bool> ResolveNewLookupEntriesAsync(PurchaseItemDTO item)
        {
            try
            {
                if (IsNewBrand && !string.IsNullOrWhiteSpace(item.BrandName))
                {
                    var brandResult = await _serviceUnitOfWork.ItemBrandService.SaveItemBrand(
                        new ItemBrandDTO { BrandName = item.BrandName });
                    if (brandResult == null || !brandResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to create brand '{item.BrandName}'");
                        return false;
                    }
                    item.BrandId = Convert.ToInt32(brandResult.Id);
                    Brands = await LoadBrands();
                    IsNewBrand = false;
                }

                if (IsNewOrigin && !string.IsNullOrWhiteSpace(item.OriginName))
                {
                    var originResult = await _serviceUnitOfWork.ItemOriginService.SaveItemOrigin(
                        new ItemOriginDTO { OriginName = item.OriginName });
                    if (originResult == null || !originResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to create origin '{item.OriginName}'");
                        return false;
                    }
                    item.OriginId = Convert.ToInt32(originResult.Id);
                    Origins = await LoadOrigins();
                    IsNewOrigin = false;
                }

                if (IsNewCatalogue && !string.IsNullOrWhiteSpace(item.CatalogueName))
                {
                    var catResult = await _serviceUnitOfWork.ItemCatalogueService.SaveItemCatalogue(
                        new ItemCatalogueDTO { CatalogueName = item.CatalogueName });
                    if (catResult == null || !catResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to create catalogue '{item.CatalogueName}'");
                        return false;
                    }
                    item.CatalogueId = Convert.ToInt32(catResult.Id);
                    Catalogues = await LoadCatalogues();
                    IsNewCatalogue = false;
                }

                var allFeatureIds = SelectedFeatureIds.ToList();
                foreach (var fname in NewFeatureNames.ToList())
                {
                    var featureResult = await _serviceUnitOfWork.ItemFeatureService.SaveItemFeature(
                        new ItemFeatureDTO { FeatureName = fname });
                    if (featureResult == null || !featureResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to create feature '{fname}'");
                        return false;
                    }
                    if (featureResult.Id != null)
                        allFeatureIds.Add(Convert.ToInt32(featureResult.Id));
                }

                if (NewFeatureNames.Any())
                {
                    Features = await LoadFeatures();
                    NewFeatureNames.Clear();
                }

                item.FeatureIds = allFeatureIds;
                item.FeaturesDisplay = string.Join(", ", Features
                    .Where(f => allFeatureIds.Contains(f.FeatureId))
                    .Select(f => f.FeatureName));

                return true;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Lookup resolution failed: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Add ALL preview items to the main ITEMS GRID
        // ═══════════════════════════════════════════════════════════════
        protected async Task AddItemToGrid()
        {
            if (!PreviewItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "No Items", "Add items to the preview grid first.");
                return;
            }

            IsProcessing = true;
            try
            {
                var addedCount = 0;
                foreach (var previewItem in PreviewItems.ToList())
                {
                    // Validate
                    var validation = ValidatePreviewItem(previewItem);
                    if (!validation.IsValid)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Validation Error",
                            $"[{previewItem.Barcode}] {validation.Message}");
                        continue;
                    }

                    // Recalculate total
                    previewItem.TotalAmount = CalculateItemTotalFor(previewItem);

                    // Resolve brand/origin/catalogue for new-item rows
                    if (previewItem.IsNewItem)
                    {
                        bool resolved = await ResolveNewLookupEntriesAsync(previewItem);
                        if (!resolved) continue;
                    }

                    // Copy group/subgroup names if missing
                    if (string.IsNullOrWhiteSpace(previewItem.GroupName) && previewItem.GroupId.HasValue)
                        previewItem.GroupName = Groups.FirstOrDefault(g => g.Id == previewItem.GroupId)?.Name;
                    if (string.IsNullOrWhiteSpace(previewItem.SubGroupName) && previewItem.SubGroupId.HasValue)
                        previewItem.SubGroupName = SubGroups.FirstOrDefault(s => s.Id == previewItem.SubGroupId)?.Name;
                    if (string.IsNullOrWhiteSpace(previewItem.MesurementUnitName) && previewItem.MesurementUnitId.HasValue)
                        previewItem.MesurementUnitName = Units.FirstOrDefault(u => u.Id == previewItem.MesurementUnitId)?.Name;

                    previewItem.PurchaseId = Purchase.Id;
                    PurchaseItems.Add(previewItem);
                    addedCount++;
                }

                if (addedCount > 0)
                {
                    PreviewItems.Clear();
                    await PreviewGrid.Reload();
                    await ItemsGrid.Reload();
                    CalculateTotals();

                    // Reset shared prices and item form
                    SharedQty = 1;
                    SharedPurchasePrice = 0;
                    SharedOtherCost = null;
                    SharedCarryingCost = null;
                    SharedVatPercentage = null;
                    SharedSalePrice = null;
                    _barcodeInputText = string.Empty;
                    CurrentItem = CreateNewItem();
                    DisableItemFields = false;
                    IsNewItemMode = false;
                    ResetItemFormSelections();

                    notificationService.Notify(NotificationSeverity.Success, "Added",
                        $"{addedCount} item(s) added to purchase.");
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Edit Item from main grid — put it back in preview for editing
        // ═══════════════════════════════════════════════════════════════
        protected async Task EditItem(PurchaseItemDTO item)
        {
            _editingItem = item;

            CurrentItem = new PurchaseItemDTO
            {
                Id = item.Id,
                PurchaseId = item.PurchaseId,
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                GroupId = item.GroupId,
                GroupName = item.GroupName,
                SubGroupId = item.SubGroupId,
                SubGroupName = item.SubGroupName,
                DesignId = item.DesignId,
                DesignName = item.DesignName,
                ShadeNo = item.ShadeNo,
                ColorId = item.ColorId,
                ColorName = item.ColorName,
                SizeId = item.SizeId,
                SizeName = item.SizeName,
                BrandId = item.BrandId,
                BrandName = item.BrandName,
                OriginId = item.OriginId,
                OriginName = item.OriginName,
                FeatureIds = item.FeatureIds?.ToList() ?? new List<int>(),
                FeaturesDisplay = item.FeaturesDisplay,
                Barcode = item.Barcode,
                Quantity = item.Quantity,
                PurchasePrice = item.PurchasePrice,
                ProductPricePercentage = item.ProductPricePercentage,
                OtherCost = item.OtherCost,
                CarryingCost = item.CarryingCost,
                TransportCost = item.TransportCost,
                OperationalCost = item.OperationalCost,
                VatPercentage = item.VatPercentage,
                VatAmount = item.VatAmount,
                TotalAmount = item.TotalAmount,
                IsSaleable = item.IsSaleable,
                IsConsume = item.IsConsume,
                SalePrice = item.SalePrice,
                ProductType = item.ProductType,
                MaterialType = item.MaterialType,
                MesurementUnitId = item.MesurementUnitId,
                MesurementUnitName = item.MesurementUnitName,
                CountStockByColor = item.CountStockByColor,
                CountStockBySize = item.CountStockBySize,
                IsNewItem = item.IsNewItem,
                IsActive = item.IsActive,
            };

            // Restore UI state
            CatalogueSearchText = item.CatalogueName ?? string.Empty;
            SelectedCatalogueId = item.CatalogueId;
            IsNewCatalogue = false;
            CurrentItemImageBase64 = item.ImageBase64 ?? string.Empty;
            BrandSearchText = item.BrandName ?? string.Empty;
            OriginSearchText = item.OriginName ?? string.Empty;
            SelectedFeatureIds = item.FeatureIds?.ToList() ?? new List<int>();
            NewFeatureNames = new List<string>();
            NewFeatureInput = string.Empty;
            IsNewBrand = false;
            IsNewOrigin = false;
            _barcodeInputText = item.Barcode ?? string.Empty;
            DisableItemFields = false;
            IsNewItemMode = item.IsNewItem;

            // Restore shared prices from this item
            SharedQty = item.Quantity;
            SharedPurchasePrice = item.PurchasePrice;
            SharedOtherCost = item.OtherCost;
            SharedCarryingCost = item.CarryingCost;
            SharedVatPercentage = item.VatPercentage;
            SharedSalePrice = item.SalePrice;

            // Put item into preview grid for editing
            PreviewItems.Clear();
            PreviewItems.Add(CurrentItem);

            // Reload cascaded dropdowns
            if (item.GroupId.HasValue)
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);
            if (item.SubGroupId.HasValue)
            {
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);
            }

            PurchaseItems.Remove(item);
            CalculateTotals();
            await ItemsGrid.Reload();
            await PreviewGrid.Reload();

            StateHasChanged();
        }

        protected void Cancel()
        {
            if (_editingItem != null)
            {
                PurchaseItems.Add(_editingItem);
                _editingItem = null;
                PreviewItems.Clear();
                CalculateTotals();
            }
            NavigationManager.NavigateTo("/PurchaseList");
        }

        protected void DeleteItem(PurchaseItemDTO item)
        {
            PurchaseItems.Remove(item);
            CalculateTotals();
            ItemsGrid?.Reload();
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item removed from purchase");
        }

        private (bool IsValid, string Message) ValidatePreviewItem(PurchaseItemDTO item)
        {
            if (item.IsNewItem)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                    return (false, "Item name is required for new items");
                if (!item.SubGroupId.HasValue || item.SubGroupId.Value == 0)
                    return (false, "Sub-group is required for new items");
                if (!item.MesurementUnitId.HasValue || item.MesurementUnitId.Value == 0)
                    return (false, "Unit is required for new items");
            }
            else
            {
                if (item.ItemId == 0)
                    return (false, "Invalid item — no ItemId");
            }

            if (string.IsNullOrWhiteSpace(item.Barcode))
                return (false, "Barcode is required");
            if (item.Quantity <= 0)
                return (false, "Quantity must be greater than 0");
            if (item.PurchasePrice <= 0)
                return (false, "Purchase price must be greater than 0");

            if (item.IsSaleable)
            {
                if (!item.SalePrice.HasValue || item.SalePrice.Value <= 0)
                    return (false, "Sale price is required for saleable items");
                if (item.SalePrice.Value <= item.PurchasePrice)
                    return (false, "Sale price must be greater than purchase price");
            }

            if (PurchaseItems.Any(i => i.Barcode == item.Barcode))
                return (false, "Item with this barcode already added to purchase");

            return (true, string.Empty);
        }

        // ═══════════════════════════════════════════════════════════════
        // Save Purchase
        // ═══════════════════════════════════════════════════════════════
        protected async Task SavePurchase()
        {
            if (!ValidatePurchase()) return;

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.PurchaseService.SaveUpdatePurchase(Purchase, PurchaseItems);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        result.Message ?? "Purchase saved successfully");
                    NavigationManager.NavigateTo("/PurchaseList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error",
                        result.Message ?? "Failed to save purchase");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase: {ex.Message}");
            }
            finally { IsProcessing = false; }
        }

        private bool ValidatePurchase()
        {
            if (!Purchase.SupplierId.HasValue || Purchase.SupplierId.Value == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a supplier");
                return false;
            }
            if (!Purchase.StoreId.HasValue || Purchase.StoreId.Value == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a store");
                return false;
            }
            if (Purchase.PurchaseDate == default)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a purchase date");
                return false;
            }
            if (PurchaseItems.Count == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please add at least one item");
                return false;
            }
            return true;
        }

        protected async Task SaveAsDraft()
        {
            try
            {
                var result = await dialogService.OpenAsync<SaveDraftDialog>("Save as Draft",
                    new Dictionary<string, object>() { { "DraftName", "" } },
                    new DialogOptions() { Width = "400px" });

                if (result == null || string.IsNullOrWhiteSpace(result.ToString())) return;

                IsProcessing = true;

                var draftDTO = new PurchaseDraftDTO
                {
                    DraftName = result.ToString()!,
                    SupplierId = Purchase.SupplierId,
                    StoreId = Purchase.StoreId,
                    PurchaseDate = Purchase.PurchaseDate,
                    BillInvoiceNumber = Purchase.BillInvoiceNumber,
                    BillInvoiceName = Purchase.BillInvoiceName,
                    IsVatIncluded = Purchase.IsVatIncluded,
                    TotalAmount = Purchase.TotalAmount,
                    DiscountAmount = Purchase.DiscountAmount,
                    VatAmount = Purchase.VatAmount,
                    NetAmount = Purchase.NetAmount,
                    PaidAmount = Purchase.PaidAmount,
                    DueAmount = Purchase.DueAmount,
                    Remarks = Purchase.Remarks,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1,
                    IsActive = true
                };

                var draftItems = PurchaseItems.Select(pi => new PurchaseDraftItemDTO
                {
                    ItemId = pi.ItemId,
                    ItemName = pi.ItemName,
                    GroupId = pi.GroupId,
                    GroupName = pi.GroupName,
                    SubGroupId = pi.SubGroupId,
                    SubGroupName = pi.SubGroupName,
                    DesignId = pi.DesignId,
                    DesignName = pi.DesignName,
                    ShadeNo = pi.ShadeNo,
                    ColorId = pi.ColorId,
                    ColorName = pi.ColorName,
                    SizeId = pi.SizeId,
                    SizeName = pi.SizeName,
                    CatalogueId = pi.CatalogueId,
                    CatalogueName = pi.CatalogueName,
                    MaterialType = pi.MaterialType,
                    BrandId = pi.BrandId,
                    BrandName = pi.BrandName,
                    OriginId = pi.OriginId,
                    OriginName = pi.OriginName,
                    FeatureIds = pi.FeatureIds ?? new List<int>(),
                    FeaturesDisplay = pi.FeaturesDisplay ?? string.Empty,
                    Barcode = pi.Barcode,
                    MesurementUnitId = pi.MesurementUnitId,
                    Quantity = pi.Quantity,
                    PurchasePrice = pi.PurchasePrice,
                    ProductPricePercentage = pi.ProductPricePercentage,
                    OtherCost = pi.OtherCost,
                    CarryingCost = pi.CarryingCost,
                    TransportCost = pi.TransportCost,
                    OperationalCost = pi.OperationalCost,
                    VatPercentage = pi.VatPercentage,
                    VatAmount = pi.VatAmount,
                    TotalAmount = pi.TotalAmount,
                    SalePrice = pi.SalePrice,
                    IsSaleable = pi.IsSaleable,
                    IsConsume = pi.IsConsume,
                    ProductType = pi.ProductType,
                    CountStockByColor = pi.CountStockByColor,
                    CountStockBySize = pi.CountStockBySize,
                    IsNewItem = pi.IsNewItem,
                    IsActive = pi.IsActive
                }).ToList();

                var saveResult = await _serviceUnitOfWork.PurchaseService.SavePurchaseDraft(draftDTO, draftItems);

                if (saveResult.IsSuccessStatus)
                    notificationService.Notify(NotificationSeverity.Success, "Draft Saved",
                        saveResult.Message ?? $"Draft '{draftDTO.DraftName}' saved successfully");
                else
                    notificationService.Notify(NotificationSeverity.Error, "Error",
                        saveResult.Message ?? "Failed to save draft");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save draft: {ex.Message}");
            }
            finally { IsProcessing = false; }
        }

        // ═══════════════════════════════════════════════════════════════
        // Cascade / Product name / Barcode Events
        // ═══════════════════════════════════════════════════════════════
        protected async Task OnGroupChanged(int? groupId)
        {
            if (!groupId.HasValue) return;
            SubGroups = await LoadSubGroupsByGroup(groupId.Value);
            var sad = await _serviceUnitOfWork.GroupService.GetGroupById(groupId.Value);
            CurrentItem.VatPercentage = sad.VAT;
            Items = new List<ItemDTO>();
            CurrentItem.GroupId = groupId;
            CurrentItem.SubGroupId = null;
            CurrentItem.ItemId = 0;
            GenerateProductName();
            await GenerateBarcode();
        }

        protected async Task OnColorChanged(int? colorId)
        {
            if (!colorId.HasValue) return;
            CurrentItem.ColorId = colorId;
            GenerateProductName();
            await GenerateBarcode();           // updates CurrentItem.Barcode
            await TryAutoSearchAsync();        // searches with the generated barcode
        }

        protected async Task OnSizeChanged(int? sizeId)
        {
            if (!sizeId.HasValue) return;
            CurrentItem.SizeId = sizeId;
            GenerateProductName();
            await GenerateBarcode();           // updates CurrentItem.Barcode
            await TryAutoSearchAsync();        // searches with the generated barcode
        }

        protected async Task OnSubGroupChanged(int? subGroupId)
        {
            if (!subGroupId.HasValue) return;
            Items = await LoadItemsBySubGroup(subGroupId.Value);
            Designs = await LoadDesignsBySubGroup(subGroupId.Value);
            CurrentItem.SubGroupId = subGroupId;
            CurrentItem.DesignId = null;
            CurrentItem.ItemId = 0;
            await GenerateBarcode();
        }

        protected void OnDesignChanged(int? designId)
        {
            CurrentItem.DesignId = designId;
            GenerateProductName();
        }

        protected void OnBrandChanged(string? brand) { CurrentItem.BrandName = brand; GenerateProductName(); }
        protected void OnCatalogueChanged(string? catalogue) { CurrentItem.CatalogueName = catalogue; GenerateProductName(); }
        protected void OnProductNameChanged(string? productName)
        {
            IsProductNameFieldChange = true;
            GenerateProductName();
        }

        private void GenerateProductName()
        {
            if (CurrentItem.ItemId != 0 && !IsNewItemMode) return;

            string subProduct = Designs.FirstOrDefault(s => s.Id == CurrentItem.DesignId)?.Name ?? "";
            string brand = BrandSearchText ?? "";
            string color = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? "";
            string size = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? "";
            string catalogue = CatalogueSearchText ?? "";

            if (IsProductNameFieldChange)
            {
                var parts = (CurrentItem.ItemName ?? "")
                    .Split(" - ")
                    .TakeWhile(p => p != color && p != size)
                    .ToList();

                parts.AddRange(new[] { color, size }.Where(p => !string.IsNullOrWhiteSpace(p)));
                CurrentItem.ItemName = string.Join(" - ", parts);
            }
            else
            {
                List<string> parts;
                if (!string.IsNullOrWhiteSpace(catalogue))
                    parts = new List<string> { catalogue, color, size };
                else
                    parts = new List<string> { subProduct, brand, color, size };

                CurrentItem.ItemName = string.Join(" - ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            StateHasChanged();
        }

        protected async Task OnItemChanged(int itemId)
        {
            if (itemId > 0) await LoadItemDetails(itemId);
        }

        private async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId) =>
            await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId) ?? new List<SubGroupModelDTO>();
        private async Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId) =>
            await _serviceUnitOfWork.DesignService.LoadDesignsBySubGroup(subGroupId) ?? new List<DesignModelDTO>();
        private async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId) =>
            await _serviceUnitOfWork.ItemService.LoadItemsBySubGroup(subGroupId) ?? new List<ItemDTO>();
        private Task LoadItemDetails(int itemId) => Task.CompletedTask;

        // ═══════════════════════════════════════════════════════════════
        // Barcode Generation — unchanged original logic.
        // Called on Group/SubGroup/Color/Size change.
        // After generation, OnColorChanged/OnSizeChanged call TryAutoSearchAsync.
        // ═══════════════════════════════════════════════════════════════
        protected async Task GenerateBarcode()
        {
            if (!CurrentItem.GroupId.HasValue && CurrentItem.ItemId == 0) return;

            try
            {
                var request = new BarcodeGenerationRequestDTO
                {
                    ShadeNo = CurrentItem.ShadeNo,
                    ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.ColorCode,
                    SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name,
                    ItemId = CurrentItem.ItemId,
                    GroupId = CurrentItem.GroupId
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentItem.Barcode = barcode;
                BarcodeSearchText = barcode;
                BarcodeInputText = barcode;
                GenerateProductName();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to generate barcode: {ex.Message}");
            }
        }

        protected void ToggleCreateNewItem()
        {
            IsNewItemMode = !IsNewItemMode;
            CurrentItem.IsNewItem = IsNewItemMode;

            if (IsNewItemMode)
            {
                DisableItemFields = false;
                CurrentItem.ItemId = 0;
                GenerateProductName();
                notificationService.Notify(NotificationSeverity.Info, "Create Mode", "Fill in the details to create a new item");
            }
            else
            {
                CurrentItem = CreateNewItem();
                DisableItemFields = false;
                _barcodeInputText = string.Empty;
                PreviewItems.Clear();
                ResetItemFormSelections();
            }

            StateHasChanged();
        }

        protected void ClearBarcodeSearch()
        {
            _barcodeInputText = string.Empty;
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            IsNewItemMode = false;
            CurrentItem = CreateNewItem();
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetItemFormSelections();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Calculation
        // ═══════════════════════════════════════════════════════════════
        protected void CalculateItemTotal()
        {
            CurrentItem.TotalAmount = _serviceUnitOfWork.PurchaseService.CalculateItemTotal(CurrentItem);
        }

        protected void CalculateTotals()
        {
            Purchase.TotalAmount = _serviceUnitOfWork.PurchaseService.CalculatePurchaseTotal(PurchaseItems);
            Purchase.NetAmount = Purchase.TotalAmount - (Purchase.DiscountAmount ?? 0) + (Purchase.VatAmount ?? 0);
            Purchase.DueAmount = Purchase.NetAmount - (Purchase.PaidAmount ?? 0);
            StateHasChanged();
        }

        protected void OnDiscountChanged() => CalculateTotals();
        protected void OnVatChanged() => CalculateTotals();
        protected void OnPaidAmountChanged() => CalculateTotals();
        public void Dispose() => ItemsGrid?.Dispose();
    }
}