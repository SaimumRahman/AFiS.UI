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
using JM.UI.Entities.Model.StockOpening;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.StockOpening
{
    public partial class StockOpeningEntryComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

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

        // ─── Stock Opening Data ──────────────────────────────────────
        protected StockOpeningEntryDTO StockOpening { get; set; } = new();
        protected List<StockOpeningItemDTO> StockOpeningItems { get; set; } = new();
        protected StockOpeningItemDTO CurrentItem { get; set; } = new();
        protected StockOpeningItemDTO? _editingItem = null;

        // ─── Preview Grid ────────────────────────────────────────────
        protected List<StockOpeningPreviewRow> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<StockOpeningPreviewRow> PreviewGrid = new();

        // ─── Shared price fields (S.Rate + QTY only for Stock Opening) ───
        protected decimal SharedSalePrice { get; set; } = 0;
        protected int? SharedQuantity { get; set; }

        // ─── Lookup Data ─────────────────────────────────────────────
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();
        protected IEnumerable<DesignModelDTO> Designs { get; set; } = new List<DesignModelDTO>();
        protected IEnumerable<ItemDTO> Items { get; set; } = new List<ItemDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();

        // ─── Brand / Origin / Features ───────────────────────────────
        protected IEnumerable<ItemBrandDTO> Brands { get; set; } = new List<ItemBrandDTO>();
        protected IEnumerable<ItemFeatureDTO> Features { get; set; } = new List<ItemFeatureDTO>();
        protected IEnumerable<ItemOriginDTO> Origins { get; set; } = new List<ItemOriginDTO>();

        protected string BrandSearchText { get; set; } = string.Empty;
        protected List<ItemBrandDTO> BrandSuggestions { get; set; } = new();
        protected int? SelectedBrandId { get; set; }
        protected bool IsNewBrand { get; set; } = false;

        protected string OriginSearchText { get; set; } = string.Empty;
        protected IEnumerable<ItemOriginDTO> OriginSuggestions { get; set; } = new List<ItemOriginDTO>();
        protected int? SelectedOriginId { get; set; }
        protected bool IsNewOrigin { get; set; } = false;

        protected IEnumerable<int> SelectedFeatureIds { get; set; } = new List<int>();
        protected List<string> NewFeatureNames { get; set; } = new();
        protected string NewFeatureInput { get; set; } = string.Empty;

        // ─── UI State ────────────────────────────────────────────────
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsNewItemMode { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;
        protected bool IsEditItemMode { get; set; } = false;
        protected string PageTitle => "Stock Opening Entry";

        protected RadzenDataGrid<StockOpeningItemDTO> ItemsGrid = default!;

        // ═══════════════════════════════════════════════════════════════
        // Lifecycle
        // ═══════════════════════════════════════════════════════════════
        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();
            await InitializeStockOpening();
        }

        // ═══════════════════════════════════════════════════════════════
        // Initialization
        // ═══════════════════════════════════════════════════════════════
        private async Task InitializeStockOpening()
        {
            StockOpening = new StockOpeningEntryDTO
            {
                TransectionDate = DateTime.Now
            };
            StockOpeningItems = new List<StockOpeningItemDTO>();
            CurrentItem = CreateNewItem();
            PreviewItems = new List<StockOpeningPreviewRow>();

            if (Stores != null)
            {
                var central = Stores.FirstOrDefault(s => s.Name.Contains("Central", StringComparison.OrdinalIgnoreCase));
                if (central != null) StockOpening.StoreId = central.Id;
            }
        }

        private StockOpeningItemDTO CreateNewItem() => new()
        {
            IsSaleable = true,
            Quantity = 1,
            IsActive = true,
            IsNewItem = false,
            CountStockByColor = false,
            CountStockBySize = false
        };

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
                Stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
                Groups = await _serviceUnitOfWork.GroupService.GetGroups() ?? new List<GroupModelDTO>();
                Colors = await _serviceUnitOfWork.ColorsService.GetColorss() ?? new List<ColorsDTO>();
                Sizes = await _serviceUnitOfWork.SizesService.GetSizess() ?? new List<SizesDTO>();
                Units = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits() ?? new List<MesurementUnitModelDTO>();
                Brands = await _serviceUnitOfWork.ItemBrandService.GetItemBrands() ?? new List<ItemBrandDTO>();
                Features = await _serviceUnitOfWork.ItemFeatureService.GetItemFeatures() ?? new List<ItemFeatureDTO>();
                Origins = await _serviceUnitOfWork.ItemOriginService.GetItemOrigins() ?? new List<ItemOriginDTO>();
                AvailableItems = await _serviceUnitOfWork.ItemService.GetItems() ?? new List<ItemDTO>();
                Catalogues = await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues() ?? new List<ItemCatalogueDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
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

                ApplyImageToMatchingPreviewRows(CurrentItemImageBase64);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Upload Error", $"Failed to read image: {ex.Message}");
            }
        }

        private void ApplyImageToMatchingPreviewRows(string imageBase64)
        {
            if (!PreviewItems.Any()) return;
            foreach (var row in PreviewItems)
            {
                if (row.ColorId == CurrentItem.ColorId)
                    row.ImageBase64 = imageBase64;
            }
            PreviewGrid?.Reload();
        }

        protected void ClearItemImage()
        {
            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CurrentItem.ImageBase64 = null;

            foreach (var row in PreviewItems.Where(r => r.ColorId == CurrentItem.ColorId))
                row.ImageBase64 = null;

            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Brand Handlers
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
        // Origin Handlers
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
        // Features Handlers
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
                notificationService.Notify(NotificationSeverity.Info, "Feature exists", $"'{trimmed}' already exists and was selected.");
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
        // Pre-save Lookup Resolution
        // ═══════════════════════════════════════════════════════════════
        private async Task<bool> ResolveNewLookupEntriesAsync(StockOpeningItemDTO item)
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
                    Brands = await _serviceUnitOfWork.ItemBrandService.GetItemBrands() ?? new List<ItemBrandDTO>();
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
                    Origins = await _serviceUnitOfWork.ItemOriginService.GetItemOrigins() ?? new List<ItemOriginDTO>();
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
                    Catalogues = await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues() ?? new List<ItemCatalogueDTO>();
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
                    Features = await _serviceUnitOfWork.ItemFeatureService.GetItemFeatures() ?? new List<ItemFeatureDTO>();
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
        // Preview Grid: Shared Price Change
        // ═══════════════════════════════════════════════════════════════
        protected void OnSharedPriceChanged()
        {
            foreach (var row in PreviewItems)
            {
                row.SalePrice = SharedSalePrice;
                row.Quantity = SharedQuantity ?? 0;
                RecalculatePreviewRow(row);
            }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnPreviewRowChanged(StockOpeningPreviewRow row)
        {
            RecalculatePreviewRow(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // Stock Opening total = SalePrice * Quantity only
        private void RecalculatePreviewRow(StockOpeningPreviewRow row)
        {
            row.TotalAmount = row.SalePrice * row.Quantity;
        }

        protected void RemovePreviewRow(StockOpeningPreviewRow row)
        {
            PreviewItems.Remove(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Build Preview Rows from Barcode Search Response
        // ═══════════════════════════════════════════════════════════════
        private List<StockOpeningPreviewRow> BuildPreviewRowsFromResponse(
            BarcodeSearchResponseDTO response, string barcode)
        {
            var rows = new List<StockOpeningPreviewRow>();

            if (response.Found && response.ItemDetails != null && response.ItemDetails.Any())
            {
                foreach (var item in response.ItemDetails.Where(x => x != null))
                {
                    var itemBarcode = !string.IsNullOrWhiteSpace(item!.Barcode) ? item.Barcode : barcode;
                    if (PreviewItems.Any(p => p.Barcode == itemBarcode)) continue;

                    var features = response.itemWiseFeatures?
                        .Where(f => f?.ItemId == item.Id)
                        .Select(f => f!.FeaturesId)
                        .ToList() ?? new List<int>();

                    rows.Add(new StockOpeningPreviewRow
                    {
                        ItemId = item.Id,
                        ItemName = item.Name ?? string.Empty,
                        Barcode = itemBarcode,
                        ColorId = item.ColorId,
                        ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty,
                        SizeId = item.SizeId,
                        SizeName = Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.Name ?? string.Empty,
                        GroupId = item.GroupId,
                        SubGroupId = item.SubGroupId,
                        BrandId = item.BrandId,
                        BrandName = Brands.FirstOrDefault(b => b.BrandId == item.BrandId)?.BrandName ?? item.BrandColor ?? string.Empty,
                        OriginId = item.OriginId,
                        OriginName = Origins.FirstOrDefault(o => o.OriginId == item.OriginId)?.OriginName ?? string.Empty,
                        FeatureIds = features,
                        FeaturesDisplay = string.Join(", ", Features.Where(f => features.Contains(f.FeatureId)).Select(f => f.FeatureName)),
                        MesurementUnitId = item.MesurementUnitId,
                        MesurementUnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                        CatalogueId = item.CatalogueId,
                        CatalogueName = item.Catalogue ?? string.Empty,
                        DesignId = item.DesignId,
                        IsNewItem = false,
                        IsSaleable = item.SalePrice.HasValue && item.SalePrice > 0,
                        CountStockByColor = item.CountStockByColor,
                        CountStockBySize = item.CountStockBySize,
                        Quantity = 0,
                        StockQuantity = response.Stock?.Quantity ?? 0,
                        SalePrice = SharedSalePrice > 0 ? SharedSalePrice : (item.SalePrice ?? 0),
                        TotalAmount = 0,
                        ImageBase64 = item.ColorId == CurrentItem.ColorId ? CurrentItemImageBase64 : null
                    });
                }
            }
            else
            {
                if (!PreviewItems.Any(p => p.Barcode == barcode))
                {
                    rows.Add(new StockOpeningPreviewRow
                    {
                        ItemId = 0,
                        ItemName = CurrentItem.ItemName ?? string.Empty,
                        Barcode = barcode,
                        ColorId = CurrentItem.ColorId,
                        ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? string.Empty,
                        SizeId = CurrentItem.SizeId,
                        SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? string.Empty,
                        GroupId = CurrentItem.GroupId,
                        SubGroupId = CurrentItem.SubGroupId,
                        BrandId = CurrentItem.BrandId,
                        BrandName = BrandSearchText,
                        OriginId = CurrentItem.OriginId,
                        OriginName = OriginSearchText,
                        MesurementUnitId = CurrentItem.MesurementUnitId,
                        CatalogueId = CurrentItem.CatalogueId,
                        CatalogueName = CatalogueSearchText,
                        IsNewItem = true,
                        IsSaleable = CurrentItem.IsSaleable,
                        CountStockByColor = CurrentItem.CountStockByColor,
                        CountStockBySize = CurrentItem.CountStockBySize,
                        Quantity = 0,
                        StockQuantity = 0,
                        SalePrice = SharedSalePrice,
                        TotalAmount = 0,
                        ImageBase64 = CurrentItemImageBase64
                    });
                }
            }

            return rows;
        }

        // ═══════════════════════════════════════════════════════════════
        // Add Items to Confirmed Grid
        // ═══════════════════════════════════════════════════════════════
        protected async Task AddItemToGrid()
        {
            if (IsEditItemMode)
            {
                await UpdateEditedItem();
                return;
            }

            if (PreviewItems.Any())
            {
                var validRows = PreviewItems.Where(r => r.Quantity > 0).ToList();
                if (!validRows.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "No Quantity",
                        "Please enter quantity for at least one item in the preview grid.");
                    return;
                }

                foreach (var row in validRows)
                {
                    if (StockOpeningItems.Any(i => i.Barcode == row.Barcode))
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Duplicate",
                            $"Barcode '{row.Barcode}' already added. Skipping.");
                        continue;
                    }

                    if (row.IsSaleable && row.SalePrice <= 0)
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Validation",
                            $"Sale price required for '{row.ItemName}'.");
                        continue;
                    }

                    StockOpeningItems.Add(new StockOpeningItemDTO
                    {
                        ItemId = row.ItemId,
                        ItemName = row.ItemName,
                        GroupId = row.GroupId,
                        GroupName = Groups.FirstOrDefault(g => g.Id == row.GroupId)?.Name,
                        SubGroupId = row.SubGroupId,
                        SubGroupName = SubGroups.FirstOrDefault(s => s.Id == row.SubGroupId)?.Name,
                        ColorId = row.ColorId,
                        ColorName = row.ColorName,
                        SizeId = row.SizeId,
                        SizeName = row.SizeName,
                        BrandId = row.BrandId,
                        BrandName = row.BrandName,
                        OriginId = row.OriginId,
                        OriginName = row.OriginName,
                        FeatureIds = row.FeatureIds,
                        FeaturesDisplay = row.FeaturesDisplay,
                        Barcode = row.Barcode,
                        Quantity = row.Quantity,
                        SalePrice = row.SalePrice,
                        TotalAmount = row.TotalAmount,
                        IsSaleable = row.IsSaleable,
                        IsConsume = row.IsConsume,
                        MesurementUnitId = row.MesurementUnitId,
                        MesurementUnitName = row.MesurementUnitName,
                        CountStockByColor = row.CountStockByColor,
                        CountStockBySize = row.CountStockBySize,
                        IsNewItem = row.IsNewItem,
                        DesignId = row.DesignId,
                        CatalogueId = row.CatalogueId,
                        CatalogueName = row.CatalogueName,
                        ImageBase64 = row.ImageBase64,
                        IsActive = true
                    });
                }

                PreviewItems.Clear();
                await PreviewGrid.Reload();
                await ItemsGrid.Reload();
                ResetSharedPricing();
                ResetItemFormSelections();
                BarcodeSearchText = string.Empty;
                DisableItemFields = false;
                IsNewItemMode = false;
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"{validRows.Count} item(s) added to stock opening");
                return;
            }

            // Fallback single-item add
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            CurrentItem.TotalAmount = (CurrentItem.SalePrice ?? 0) * CurrentItem.Quantity;

            if (IsNewBrand) CurrentItem.BrandName = BrandSearchText;
            bool resolved = await ResolveNewLookupEntriesAsync(CurrentItem);
            if (!resolved) return;

            StockOpeningItems.Add(new StockOpeningItemDTO
            {
                ItemId = CurrentItem.ItemId,
                ItemName = CurrentItem.ItemName,
                GroupId = CurrentItem.GroupId,
                GroupName = Groups.FirstOrDefault(g => g.Id == CurrentItem.GroupId)?.Name,
                SubGroupId = CurrentItem.SubGroupId,
                SubGroupName = SubGroups.FirstOrDefault(s => s.Id == CurrentItem.SubGroupId)?.Name,
                ShadeNo = CurrentItem.ShadeNo,
                ColorId = CurrentItem.ColorId,
                ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name,
                SizeId = CurrentItem.SizeId,
                SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name,
                BrandId = CurrentItem.BrandId,
                BrandName = CurrentItem.BrandId.HasValue
                    ? Brands.FirstOrDefault(b => b.BrandId == CurrentItem.BrandId)?.BrandName
                    : CurrentItem.BrandName,
                OriginId = CurrentItem.OriginId,
                OriginName = CurrentItem.OriginId.HasValue
                    ? Origins.FirstOrDefault(o => o.OriginId == CurrentItem.OriginId)?.OriginName
                    : CurrentItem.OriginName,
                FeatureIds = CurrentItem.FeatureIds,
                FeaturesDisplay = CurrentItem.FeaturesDisplay,
                Barcode = CurrentItem.Barcode,
                Quantity = CurrentItem.Quantity,
                SalePrice = CurrentItem.SalePrice,
                TotalAmount = CurrentItem.TotalAmount,
                IsSaleable = CurrentItem.IsSaleable,
                IsConsume = CurrentItem.IsConsume,
                MesurementUnitId = CurrentItem.MesurementUnitId,
                CountStockByColor = CurrentItem.CountStockByColor,
                CountStockBySize = CurrentItem.CountStockBySize,
                IsNewItem = CurrentItem.IsNewItem,
                DesignId = CurrentItem.DesignId,
                DesignName = Designs.FirstOrDefault(d => d.Id == CurrentItem.DesignId)?.Name,
                CatalogueId = CurrentItem.CatalogueId,
                CatalogueName = CurrentItem.CatalogueId.HasValue
                    ? Catalogues.FirstOrDefault(c => c.CatalogueId == CurrentItem.CatalogueId)?.CatalogueName
                    : CurrentItem.CatalogueName,
                ImageBase64 = CurrentItem.ImageBase64,
            });

            await ItemsGrid.Reload();
            ResetItemFormSelections();
            CurrentItem = CreateNewItem();
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added – ready for next entry");
        }

        private void ResetSharedPricing()
        {
            SharedSalePrice = 0;
            SharedQuantity = null;
        }

        // ═══════════════════════════════════════════════════════════════
        // Edit Item
        // ═══════════════════════════════════════════════════════════════
        protected async Task EditItem(StockOpeningItemDTO item)
        {
            _editingItem = item;
            IsEditItemMode = true;

            PreviewItems.Clear();
            ResetSharedPricing();

            PreviewItems.Add(new StockOpeningPreviewRow
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                Barcode = item.Barcode ?? string.Empty,
                ColorId = item.ColorId,
                ColorName = item.ColorName ?? string.Empty,
                SizeId = item.SizeId,
                SizeName = item.SizeName ?? string.Empty,
                GroupId = item.GroupId,
                SubGroupId = item.SubGroupId,
                BrandId = item.BrandId,
                BrandName = item.BrandName ?? string.Empty,
                OriginId = item.OriginId,
                OriginName = item.OriginName ?? string.Empty,
                FeatureIds = item.FeatureIds?.ToList() ?? new List<int>(),
                FeaturesDisplay = item.FeaturesDisplay ?? string.Empty,
                MesurementUnitId = item.MesurementUnitId,
                MesurementUnitName = item.MesurementUnitName ?? string.Empty,
                CatalogueId = item.CatalogueId,
                CatalogueName = item.CatalogueName ?? string.Empty,
                DesignId = item.DesignId,
                IsNewItem = item.IsNewItem,
                IsSaleable = item.IsSaleable,
                IsConsume = item.IsConsume,
                CountStockByColor = item.CountStockByColor,
                CountStockBySize = item.CountStockBySize,
                Quantity = item.Quantity,
                StockQuantity = 0,
                SalePrice = item.SalePrice ?? 0,
                TotalAmount = item.TotalAmount,
                ImageBase64 = item.ImageBase64,
            });

            PreviewGrid?.Reload();

            CurrentItem = new StockOpeningItemDTO
            {
                Id = item.Id,
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
                SalePrice = item.SalePrice,
                TotalAmount = item.TotalAmount,
                IsSaleable = item.IsSaleable,
                IsConsume = item.IsConsume,
                MesurementUnitId = item.MesurementUnitId,
                MesurementUnitName = item.MesurementUnitName,
                CountStockByColor = item.CountStockByColor,
                CountStockBySize = item.CountStockBySize,
                IsNewItem = item.IsNewItem,
                IsActive = item.IsActive,
                CatalogueId = item.CatalogueId,
                CatalogueName = item.CatalogueName,
                ImageBase64 = item.ImageBase64,
            };

            CatalogueSearchText = item.CatalogueName ?? string.Empty;
            SelectedCatalogueId = item.CatalogueId;
            IsNewCatalogue = false;
            CurrentItemImageBase64 = item.ImageBase64 ?? string.Empty;
            CurrentItemImageMimeType = "image/jpeg";
            BrandSearchText = item.BrandName ?? string.Empty;
            OriginSearchText = item.OriginName ?? string.Empty;
            SelectedFeatureIds = item.FeatureIds?.ToList() ?? new List<int>();
            NewFeatureNames = new List<string>();
            NewFeatureInput = string.Empty;
            IsNewBrand = false;
            IsNewOrigin = false;
            BarcodeSearchText = item.Barcode ?? string.Empty;
            DisableItemFields = false;
            IsNewItemMode = item.IsNewItem;
            IsProductNameFieldChange = false;

            SharedSalePrice = item.SalePrice ?? 0;

            if (item.GroupId.HasValue)
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);

            if (item.SubGroupId.HasValue)
            {
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);
            }

            notificationService.Notify(NotificationSeverity.Info, "Edit Mode",
                $"Editing '{item.ItemName}' — make changes then click Update.");

            StateHasChanged();
        }

        protected async Task UpdateEditedItem()
        {
            if (_editingItem == null) return;

            var row = PreviewItems.FirstOrDefault();
            if (row == null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Nothing to update", "The preview grid is empty.");
                return;
            }

            CurrentItem.ItemId = row.ItemId;
            CurrentItem.ItemName = row.ItemName;
            CurrentItem.Barcode = row.Barcode;
            CurrentItem.ColorId = row.ColorId;
            CurrentItem.SizeId = row.SizeId;
            CurrentItem.BrandId = row.BrandId;
            CurrentItem.BrandName = row.BrandName;
            CurrentItem.OriginId = row.OriginId;
            CurrentItem.OriginName = row.OriginName;
            CurrentItem.FeatureIds = row.FeatureIds;
            CurrentItem.FeaturesDisplay = row.FeaturesDisplay;
            CurrentItem.Quantity = row.Quantity;
            CurrentItem.SalePrice = row.SalePrice;
            CurrentItem.TotalAmount = row.TotalAmount;
            CurrentItem.ImageBase64 = row.ImageBase64;

            var validation = ValidateEditedItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            if (IsNewBrand) CurrentItem.BrandName = BrandSearchText;
            bool resolved = await ResolveNewLookupEntriesAsync(CurrentItem);
            if (!resolved) return;

            CurrentItem.TotalAmount = (CurrentItem.SalePrice ?? 0) * CurrentItem.Quantity;

            var idx = StockOpeningItems.IndexOf(_editingItem);
            if (idx < 0) StockOpeningItems.Add(BuildUpdatedItem());
            else StockOpeningItems[idx] = BuildUpdatedItem();

            await ItemsGrid.Reload();
            CancelEditItem();

            notificationService.Notify(NotificationSeverity.Success, "Updated",
                $"'{CurrentItem.ItemName}' updated successfully.");
        }

        private StockOpeningItemDTO BuildUpdatedItem() => new StockOpeningItemDTO
        {
            Id = _editingItem!.Id,
            ItemId = CurrentItem.ItemId,
            ItemName = CurrentItem.ItemName,
            GroupId = CurrentItem.GroupId,
            GroupName = Groups.FirstOrDefault(g => g.Id == CurrentItem.GroupId)?.Name ?? CurrentItem.GroupName,
            SubGroupId = CurrentItem.SubGroupId,
            SubGroupName = SubGroups.FirstOrDefault(s => s.Id == CurrentItem.SubGroupId)?.Name ?? CurrentItem.SubGroupName,
            ShadeNo = CurrentItem.ShadeNo,
            ColorId = CurrentItem.ColorId,
            ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? CurrentItem.ColorName,
            SizeId = CurrentItem.SizeId,
            SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? CurrentItem.SizeName,
            DesignId = CurrentItem.DesignId,
            DesignName = Designs.FirstOrDefault(d => d.Id == CurrentItem.DesignId)?.Name ?? CurrentItem.DesignName,
            BrandId = CurrentItem.BrandId,
            BrandName = CurrentItem.BrandId.HasValue
                ? Brands.FirstOrDefault(b => b.BrandId == CurrentItem.BrandId)?.BrandName ?? CurrentItem.BrandName
                : CurrentItem.BrandName,
            OriginId = CurrentItem.OriginId,
            OriginName = CurrentItem.OriginId.HasValue
                ? Origins.FirstOrDefault(o => o.OriginId == CurrentItem.OriginId)?.OriginName ?? CurrentItem.OriginName
                : CurrentItem.OriginName,
            FeatureIds = CurrentItem.FeatureIds,
            FeaturesDisplay = CurrentItem.FeaturesDisplay,
            Barcode = CurrentItem.Barcode,
            Quantity = CurrentItem.Quantity,
            SalePrice = CurrentItem.SalePrice,
            TotalAmount = CurrentItem.TotalAmount,
            IsSaleable = CurrentItem.IsSaleable,
            IsConsume = CurrentItem.IsConsume,
            MesurementUnitId = CurrentItem.MesurementUnitId,
            MesurementUnitName = Units.FirstOrDefault(u => u.Id == CurrentItem.MesurementUnitId)?.Name ?? CurrentItem.MesurementUnitName,
            CountStockByColor = CurrentItem.CountStockByColor,
            CountStockBySize = CurrentItem.CountStockBySize,
            IsNewItem = CurrentItem.IsNewItem,
            IsActive = CurrentItem.IsActive,
            CatalogueId = CurrentItem.CatalogueId,
            CatalogueName = CurrentItem.CatalogueId.HasValue
                ? Catalogues.FirstOrDefault(c => c.CatalogueId == CurrentItem.CatalogueId)?.CatalogueName ?? CurrentItem.CatalogueName
                : CurrentItem.CatalogueName,
            ImageBase64 = CurrentItem.ImageBase64,
        };

        private (bool IsValid, string Message) ValidateEditedItem()
        {
            if (IsNewItemMode)
            {
                if (string.IsNullOrWhiteSpace(CurrentItem.ItemName))
                    return (false, "Item name is required");
                if (!CurrentItem.SubGroupId.HasValue || CurrentItem.SubGroupId.Value == 0)
                    return (false, "Sub-group is required");
                if (!CurrentItem.MesurementUnitId.HasValue || CurrentItem.MesurementUnitId.Value == 0)
                    return (false, "Unit is required");
            }
            else
            {
                if (CurrentItem.ItemId == 0)
                    return (false, "Item must be selected");
            }

            if (string.IsNullOrWhiteSpace(CurrentItem.Barcode))
                return (false, "Barcode is required");
            if (CurrentItem.Quantity <= 0)
                return (false, "Quantity must be greater than 0");
            if (CurrentItem.IsSaleable && (!CurrentItem.SalePrice.HasValue || CurrentItem.SalePrice.Value <= 0))
                return (false, "Sale price is required for saleable items");

            if (StockOpeningItems.Any(i => i != _editingItem && i.Barcode == CurrentItem.Barcode))
                return (false, "Another item with this barcode already exists");

            return (true, string.Empty);
        }

        protected void CancelEditItem()
        {
            _editingItem = null;
            IsEditItemMode = false;
            IsNewItemMode = false;
            DisableItemFields = false;
            BarcodeSearchText = string.Empty;
            IsProductNameFieldChange = false;
            CurrentItem = CreateNewItem();
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedPricing();
            ResetItemFormSelections();
            StateHasChanged();
        }

        protected void Cancel()
        {
            if (IsEditItemMode)
            {
                CancelEditItem();
                notificationService.Notify(NotificationSeverity.Info, "Cancelled", "Edit cancelled. No changes were saved.");
            }
            else
            {
                NavigationManager.NavigateTo("/StockOpeningList");
            }
        }

        protected void DeleteItem(StockOpeningItemDTO item)
        {
            StockOpeningItems.Remove(item);
            ItemsGrid?.Reload();
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item removed from stock opening");
        }

        private (bool IsValid, string Message) ValidateCurrentItem()
        {
            if (IsNewItemMode)
            {
                if (string.IsNullOrWhiteSpace(CurrentItem.ItemName))
                    return (false, "Item name is required for new items");
                if (!CurrentItem.SubGroupId.HasValue || CurrentItem.SubGroupId.Value == 0)
                    return (false, "Sub-group is required for new items");
                if (!CurrentItem.MesurementUnitId.HasValue || CurrentItem.MesurementUnitId.Value == 0)
                    return (false, "Unit is required for new items");
            }
            else
            {
                if (CurrentItem.ItemId == 0)
                    return (false, "Please select an item or enable create new item mode");
            }

            if (string.IsNullOrWhiteSpace(CurrentItem.Barcode))
                return (false, "Barcode is required");
            if (CurrentItem.Quantity <= 0)
                return (false, "Quantity must be greater than 0");
            if (CurrentItem.IsSaleable && (!CurrentItem.SalePrice.HasValue || CurrentItem.SalePrice.Value <= 0))
                return (false, "Sale price is required for saleable items");

            if (StockOpeningItems.Any(i => i != _editingItem && i.Barcode == CurrentItem.Barcode))
                return (false, "Item with this barcode already added");

            return (true, string.Empty);
        }

        // ═══════════════════════════════════════════════════════════════
        // Save
        // ═══════════════════════════════════════════════════════════════
        protected async Task SaveStockOpening()
        {
            if (!ValidateStockOpening()) return;

            try
            {
                IsProcessing = true;
                StockOpening.Items = StockOpeningItems;
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

        private bool ValidateStockOpening()
        {
            if (StockOpening.StoreId == 0 || StockOpening.StoreId == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a store");
                return false;
            }
            if (StockOpening.TransectionDate == default)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please select a date");
                return false;
            }
            if (!StockOpeningItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", "Please add at least one item");
                return false;
            }
            return true;
        }

        // ═══════════════════════════════════════════════════════════════
        // Cascade / Product Name / Barcode Events
        // ═══════════════════════════════════════════════════════════════
        protected async Task OnGroupChanged(int? groupId)
        {
            if (!groupId.HasValue) return;
            SubGroups = await LoadSubGroupsByGroup(groupId.Value);
            CurrentItem.GroupId = groupId;
            CurrentItem.SubGroupId = null;
            CurrentItem.ItemId = 0;
            GenerateProductName();
            await GenerateBarcode();
        }

        protected async Task OnColorChanged(int? colorId)
        {
            if (!colorId.HasValue) return;
            if (!CurrentItem.SizeId.HasValue) return;
            CurrentItem.ColorId = colorId;

            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CurrentItem.ImageBase64 = null;

            GenerateProductName();
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentItem.Barcode))
                await SearchSingleBarcodeAndAddToPreview(CurrentItem.Barcode);

            StateHasChanged();
        }

        protected async Task OnSizeChanged(int? sizeId)
        {
            if (!sizeId.HasValue) return;
            CurrentItem.SizeId = sizeId;
            GenerateProductName();
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentItem.Barcode))
                await SearchSingleBarcodeAndAddToPreview(CurrentItem.Barcode);
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

        private async Task SearchSingleBarcodeAndAddToPreview(string barcode)
        {
            try
            {
                if (PreviewItems.Any(p => p.Barcode == barcode)) return;

                var response = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);
                StockOpeningPreviewRow newRow;

                if (response.Found && response.ItemDetails != null && response.ItemDetails.Any())
                {
                    var item = response.ItemDetails.FirstOrDefault(x => x.Barcode == barcode);

                    if (item == null)
                    {
                        newRow = BuildNewPreviewRow(barcode);
                    }
                    else
                    {
                        var features = response.itemWiseFeatures?
                            .Where(f => f?.ItemId == item.Id)
                            .Select(f => f!.FeaturesId)
                            .ToList() ?? new List<int>();

                        newRow = new StockOpeningPreviewRow
                        {
                            ItemId = item.Id,
                            ItemName = item.Name ?? string.Empty,
                            Barcode = barcode,
                            ColorId = item.ColorId,
                            ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty,
                            SizeId = item.SizeId,
                            SizeName = Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.Name ?? string.Empty,
                            GroupId = item.GroupId,
                            SubGroupId = item.SubGroupId,
                            BrandId = item.BrandId,
                            BrandName = Brands.FirstOrDefault(b => b.BrandId == item.BrandId)?.BrandName ?? string.Empty,
                            OriginId = item.OriginId,
                            OriginName = Origins.FirstOrDefault(o => o.OriginId == item.OriginId)?.OriginName ?? string.Empty,
                            FeatureIds = features,
                            FeaturesDisplay = string.Join(", ", Features.Where(f => features.Contains(f.FeatureId)).Select(f => f.FeatureName)),
                            MesurementUnitId = item.MesurementUnitId,
                            MesurementUnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                            CatalogueId = item.CatalogueId,
                            CatalogueName = item.Catalogue ?? string.Empty,
                            DesignId = item.DesignId,
                            IsNewItem = false,
                            IsSaleable = item.SalePrice.HasValue && item.SalePrice > 0,
                            CountStockByColor = item.CountStockByColor,
                            CountStockBySize = item.CountStockBySize,
                            Quantity = 0,
                            StockQuantity = response.Stock?.Quantity ?? 0,
                            SalePrice = SharedSalePrice > 0 ? SharedSalePrice : (item.SalePrice ?? 0),
                            TotalAmount = 0,
                            ImageBase64 = null
                        };
                    }
                }
                else
                {
                    newRow = BuildNewPreviewRow(barcode);
                }

                PreviewItems.Add(newRow);
                PreviewGrid?.Reload();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Preview load failed: {ex.Message}");
            }
        }

        private StockOpeningPreviewRow BuildNewPreviewRow(string barcode) => new()
        {
            ItemId = 0,
            ItemName = CurrentItem.ItemName ?? string.Empty,
            Barcode = barcode,
            ColorId = CurrentItem.ColorId,
            ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? string.Empty,
            SizeId = CurrentItem.SizeId,
            SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? string.Empty,
            GroupId = CurrentItem.GroupId,
            SubGroupId = CurrentItem.SubGroupId,
            BrandId = CurrentItem.BrandId,
            BrandName = BrandSearchText,
            OriginId = CurrentItem.OriginId,
            OriginName = OriginSearchText,
            MesurementUnitId = CurrentItem.MesurementUnitId,
            CatalogueId = CurrentItem.CatalogueId,
            CatalogueName = CatalogueSearchText,
            IsNewItem = true,
            IsSaleable = CurrentItem.IsSaleable,
            CountStockByColor = CurrentItem.CountStockByColor,
            CountStockBySize = CurrentItem.CountStockBySize,
            Quantity = 0,
            StockQuantity = 0,
            SalePrice = SharedSalePrice,
            TotalAmount = 0,
            ImageBase64 = null
        };

        protected async Task OnBarcodeDropdownChanged(object value)
        {
            var barcode = value?.ToString();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                IsSearchingBarcode = true;
                BarcodeSearchText = barcode;
                var result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                if (result.Found)
                {
                    if (result.ItemDetails != null)
                    {
                        await PopulateFromExistingItem(result.ItemDetails.FirstOrDefault()!, result.itemWiseFeatures);
                        DisableItemFields = true;
                        IsNewItemMode = false;

                        PreviewItems.Clear();
                        var newRows = BuildPreviewRowsFromResponse(result, barcode);
                        foreach (var row in newRows) PreviewItems.Add(row);

                        PreviewGrid?.Reload();
                        notificationService.Notify(NotificationSeverity.Success, "Success",
                            $"Item loaded! {PreviewItems.Count} variant(s) in preview.");

                        var itemData = AvailableItems.Where(x => x.Barcode == barcode).FirstOrDefault();
                        if (itemData != null)
                        {
                            CurrentItem.ColorId = itemData.ColorId;
                            CurrentItem.SizeId = itemData.SizeId;
                        }
                    }
                    else if (result.Item != null)
                    {
                        PopulateFromStockOpeningItem(result.Item);
                        DisableItemFields = false;

                        PreviewItems.Clear();
                        var newRows = BuildPreviewRowsFromResponse(result, barcode);
                        foreach (var row in newRows) PreviewItems.Add(row);

                        PreviewGrid?.Reload();
                    }
                }
                else
                {
                    CurrentItem.Barcode = barcode;
                    DisableItemFields = false;
                    IsNewItemMode = true;

                    PreviewItems.Clear();
                    var newRows = BuildPreviewRowsFromResponse(result, barcode);
                    foreach (var row in newRows) PreviewItems.Add(row);

                    await PreviewGrid?.Reload();
                    notificationService.Notify(NotificationSeverity.Info, "Create New", "No item found. You can create a new item.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Search failed: {ex.Message}");
            }
            finally
            {
                IsSearchingBarcode = false;
                StateHasChanged();
            }
        }

        private async Task PopulateFromExistingItem(ItemDTO item, List<ItemWiseFeatureDTO> itemWiseFeatures)
        {
            if (item == null) return;

            CurrentItem.ItemId = item.Id;
            CurrentItem.ItemName = item.Name;
            CurrentItem.ShadeNo = item.ShadeNo;
            CurrentItem.ColorId = item.ColorId;
            CurrentItem.SizeId = item.SizeId;
            CurrentItem.SalePrice = item.SalePrice;
            CurrentItem.Barcode = BarcodeSearchText;
            CurrentItem.IsNewItem = false;
            CurrentItem.MesurementUnitId = item.MesurementUnitId;
            CurrentItem.CountStockByColor = item.CountStockByColor;
            CurrentItem.CountStockBySize = item.CountStockBySize;
            CurrentItem.CatalogueId = item.CatalogueId;
            CurrentItem.CatalogueName = item.Catalogue;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.OriginId = item.OriginId;
            CurrentItem.FeatureIds = itemWiseFeatures?.Select(x => x.FeaturesId).ToList() ?? new List<int>();
            CurrentItem.DesignId = item.DesignId;

            SharedSalePrice = item.SalePrice ?? 0;

            if (item.GroupId.HasValue)
            {
                CurrentItem.GroupId = item.GroupId;
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);
            }

            if (item.SubGroupId.HasValue)
            {
                CurrentItem.SubGroupId = item.SubGroupId;
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);
                CurrentItem.DesignId = item.DesignId;
            }

            SelectedFeatureIds = itemWiseFeatures?.Select(x => x.FeaturesId).ToList() ?? new List<int>();
            NewFeatureNames = new();

            BrandSearchText = item.BrandId.HasValue
                ? (Brands.FirstOrDefault(b => b.BrandId == item.BrandId)?.BrandName ?? item.BrandColor ?? "")
                : (item.BrandColor ?? "");

            OriginSearchText = item.OriginId.HasValue
                ? (Origins.FirstOrDefault(o => o.OriginId == item.OriginId)?.OriginName ?? item.Origin ?? "")
                : (item.Origin ?? "");

            CatalogueSearchText = item.CatalogueId.HasValue
                ? (Catalogues.FirstOrDefault(c => c.CatalogueId == item.CatalogueId)?.CatalogueName ?? item.Catalogue ?? "")
                : (item.Catalogue ?? "");
        }

        private void PopulateFromStockOpeningItem(PurchaseItemDTO item)
        {
            CurrentItem.ItemId = item.ItemId;
            CurrentItem.ItemName = item.ItemName;
            CurrentItem.Quantity = item.Quantity;
            CurrentItem.ColorId = item.ColorId;
            CurrentItem.SizeId = item.SizeId;
            CurrentItem.SalePrice = item.SalePrice;
            CurrentItem.Barcode = BarcodeSearchText;
            CurrentItem.IsNewItem = false;
            CurrentItem.CatalogueName = item.CatalogueName;
            CurrentItem.CatalogueId = item.CatalogueId;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.OriginId = item.OriginId;

            SharedSalePrice = item.SalePrice ?? 0;

            BrandSearchText = item.BrandName ?? string.Empty;
            OriginSearchText = item.OriginName ?? string.Empty;
            SelectedFeatureIds = item.FeatureIds ?? new List<int>();
            NewFeatureNames = new();
        }

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
                    GroupId = CurrentItem.GroupId,
                    ExistingBarcode = CurrentItem.Barcode
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentItem.Barcode = barcode;
                BarcodeSearchText = barcode;
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
                BarcodeSearchText = string.Empty;
                PreviewItems.Clear();
                PreviewGrid?.Reload();
                ResetItemFormSelections();
            }

            StateHasChanged();
        }

        protected void ClearBarcodeSearch()
        {
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            IsNewItemMode = false;
            CurrentItem = CreateNewItem();
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedPricing();
            ResetItemFormSelections();
            StateHasChanged();
        }

        // ═══════════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════════
        private async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId) =>
            await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId) ?? new List<SubGroupModelDTO>();
        private async Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId) =>
            await _serviceUnitOfWork.DesignService.LoadDesignsBySubGroup(subGroupId) ?? new List<DesignModelDTO>();
        private async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId) =>
            await _serviceUnitOfWork.ItemService.LoadItemsBySubGroup(subGroupId) ?? new List<ItemDTO>();

        public void Dispose() => ItemsGrid?.Dispose();
    }
}