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
    public partial class PurchaseEntryComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }
        [Parameter] public int? DraftId { get; set; }
        protected bool IsDraftMode => DraftId.HasValue && DraftId.Value > 0;
        protected bool IsProductNameFieldChange { get; set; } = false;

        // â”€â”€â”€ Image Upload â”€â”€
        protected string CurrentItemImageBase64 { get; set; } = string.Empty;
        protected string CurrentItemImageMimeType { get; set; } = "image/jpeg";

        // â”€â”€ Add to lookup fields (alongside Brand/Origin) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected IEnumerable<ItemCatalogueDTO> Catalogues { get; set; } = new List<ItemCatalogueDTO>();

        // Catalogue auto-complete state
        protected string CatalogueSearchText { get; set; } = string.Empty;
        protected List<ItemCatalogueDTO> CatalogueSuggestions { get; set; } = new();
        protected int? SelectedCatalogueId { get; set; }
        protected bool IsNewCatalogue { get; set; } = false;

        // â”€â”€â”€ Purchase Data â”€â”€â”€â”€
        protected PurchaseDTO Purchase { get; set; } = new();
        protected List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
        protected PurchaseItemDTO CurrentItem { get; set; } = new();
        protected PurchaseItemDTO? _editingItem = null;

        // â”€â”€â”€ Preview Grid Items (editable, loaded from barcode/color/size search) â”€â”€â”€
        protected List<PreviewItemRow> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<PreviewItemRow> PreviewGrid = new();

        // â”€â”€â”€ Shared price fields for preview grid (applied to all rows) â”€â”€â”€
        protected decimal SharedPurchasePrice { get; set; } = 0;
        protected decimal SharedSalePrice { get; set; } = 0;
        protected decimal? SharedOtherCost { get; set; }
        protected decimal? SharedCarryingCost { get; set; }
        protected decimal? SharedVatPercentage { get; set; }
        protected decimal? SharedTransportCost { get; set; }
        protected decimal? SharedOperationalCost { get; set; }
        protected int? SharedQuantity { get; set; }

        // â”€â”€â”€ Lookup Data â”€â”€â”€â”€â”€â”€
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

        // â”€â”€â”€ Brand / Origin / Features Lookups â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected IEnumerable<ItemBrandDTO> Brands { get; set; } = new List<ItemBrandDTO>();
        protected IEnumerable<ItemFeatureDTO> Features { get; set; } = new List<ItemFeatureDTO>();
        protected IEnumerable<ItemOriginDTO> Origins { get; set; } = new List<ItemOriginDTO>();

        // Brand auto-complete
        protected string BrandSearchText { get; set; } = string.Empty;
        protected List<ItemBrandDTO> BrandSuggestions { get; set; } = new List<ItemBrandDTO>();
        protected int? SelectedBrandId { get; set; }
        protected bool IsNewBrand { get; set; } = false;

        // Origin auto-complete
        protected string OriginSearchText { get; set; } = string.Empty;
        protected List<ItemOriginDTO> OriginSuggestions { get; set; } = new List<ItemOriginDTO>();
        protected int? SelectedOriginId { get; set; }
        protected bool IsNewOrigin { get; set; } = false;

        // Features multi-select
        protected IEnumerable<int> SelectedFeatureIds { get; set; } = new List<int>();
        protected List<string> NewFeatureNames { get; set; } = new();
        protected string NewFeatureInput { get; set; } = string.Empty;

        // â”€â”€â”€ UI State â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Entry" : IsDraftMode ? "Purchase Entry (From Draft)" : "New Purchase Entry";
        protected bool IsNewItemMode { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;

        protected bool IsEditItemMode { get; set; } = false;

        protected RadzenDataGrid<PurchaseItemDTO> ItemsGrid = default!;

        protected List<string> ProductTypes = new()
        {
            "Sell Product", "Raw Material", "Both", "Consume", "Combo Package"
        };
        protected override async Task OnInitializedAsync()
        {
            NavigationGuard.IsGuardActive = true;
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();
            Purchase.StoreId = Stores.FirstOrDefault()?.Id;
            if (IsDraftMode)
                await LoadDraft();
            else if (IsEditMode)
                await LoadPurchase();
            else
                await InitializePurchase();
        }
        private async Task InitializePurchase()
        {
            Purchase = await _serviceUnitOfWork.PurchaseService.CreateNewPurchase();
            PurchaseItems = new List<PurchaseItemDTO>();
            CurrentItem = CreateNewItem();
            PreviewItems = new List<PreviewItemRow>();

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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Data Loading
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        private async Task LoadLookupData()
        {
            try
            {
                Suppliers = await LoadSuppliers();
                Stores = (await LoadStores()).Where(x => x.Name.Equals("Head Office", StringComparison.OrdinalIgnoreCase));
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
            await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues()
            ?? new List<ItemCatalogueDTO>();
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
                PreviewItems = new List<PreviewItemRow>();
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

                PreviewItems = new List<PreviewItemRow>();

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

                // Apply this image to all preview rows that share the current item's color
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
                {
                    row.ImageBase64 = imageBase64;
                }
            }

            PreviewGrid?.Reload();
        }

        protected void ClearItemImage()
        {
            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CurrentItem.ImageBase64 = null;

            // Also clear image from preview rows that share the current color
            foreach (var row in PreviewItems.Where(r => r.ColorId == CurrentItem.ColorId))
                row.ImageBase64 = null;

            PreviewGrid?.Reload();
            StateHasChanged();
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Brand Auto-Complete Handlers
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        protected void OnBrandTextChanged(object value)
        {
            var text = value?.ToString();
            BrandSearchText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                SelectedBrandId = null;
                IsNewBrand = false;
                CurrentItem.BrandId = null;
                return;
            }

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
            StateHasChanged();
        }
        protected void OnOriginLoadData(LoadDataArgs args)
        {
            var text = args.Filter;

            if (string.IsNullOrWhiteSpace(text))
            {
                OriginSuggestions = new List<ItemOriginDTO>();
            }
            else
            {
                OriginSuggestions = Origins
                    .Where(o => o.OriginName.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            StateHasChanged();
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
                StateHasChanged();
            }
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Origin Auto-Complete Handlers
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        protected void OnOriginTextChanged(object value)
        {
            var text = value?.ToString();
            OriginSearchText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                SelectedOriginId = null;
                IsNewOrigin = false;
                CurrentItem.OriginName = null;
                CurrentItem.OriginId = null;
                return;
            }

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
            StateHasChanged();
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Features Multi-Select Handlers
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Save â€“ Brand / Origin / Features pre-save resolution
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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

        protected void OnSharedPriceChanged()
        {
            foreach (var row in PreviewItems)
            {
                row.BasePurchasePrice = SharedPurchasePrice;  // raw price
                row.SalePrice = SharedSalePrice;
                row.OtherCost = SharedOtherCost;
                row.CarryingCost = SharedCarryingCost;
                row.Quantity = SharedQuantity ?? 0;
                row.TransportCost = SharedTransportCost;
                row.OperationalCost = SharedOperationalCost;
                RecalculatePreviewRow(row);  // this sets row.PurchasePrice
            }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        private bool _userEditedPurchasePrice = false;

        protected void OnPreviewRowPriceChanged(PreviewItemRow row)
        {
            _userEditedPurchasePrice = true;
            RecalculatePreviewRow(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        private void RecalculatePreviewRow(PreviewItemRow row)
        {
            if (_userEditedPurchasePrice)
            {
                var baseComponents = row.BasePurchasePrice
                                    + (row.OtherCost ?? 0)
                                    + (row.CarryingCost ?? 0)
                                    + (row.TransportCost ?? 0)
                                    + (row.OperationalCost ?? 0);

                var calculated = Math.Max(0, baseComponents * row.Quantity);
                row.TotalAmount = calculated;

                _userEditedPurchasePrice = false;
                return;
            }

            // Recalculate P.Rate from components
            row.PurchasePrice = row.BasePurchasePrice
                              + (row.OtherCost ?? 0)
                              + (row.CarryingCost ?? 0)
                              + (row.TransportCost ?? 0)
                              + (row.OperationalCost ?? 0);

            var baseAmount = row.PurchasePrice * row.Quantity;

            decimal vatAmt = 0;
            if (Purchase.IsVatIncluded)
            {
                var actualVat = Groups.FirstOrDefault(x => x.Id == row.GroupId)?.VAT ?? 0;
                vatAmt = baseAmount * actualVat / 100;
            }

            row.TotalAmount = baseAmount + vatAmt;
        }

        protected async Task OnSharedPriceChanged(EditContext context)
        {
            _userEditedPurchasePrice = false;
            await InvokeAsync(StateHasChanged);
        }

        protected async Task RemovePreviewRow(PreviewItemRow row)
        {
            await _serviceUnitOfWork.ItemService.DeleteItem(row.ItemId);
            PreviewItems.Remove(row);

            PreviewGrid?.Reload();
            StateHasChanged();
        }

        private List<PreviewItemRow> BuildPreviewRowsFromResponse(BarcodeSearchResponseDTO response, string barcode)
        {
            var rows = new List<PreviewItemRow>();

            if (response.Found && response.ItemDetails != null && response.ItemDetails.Any())
            {
                foreach (var item in response.ItemDetails.Where(x => x != null))
                {
                    var itemBarcode = !string.IsNullOrWhiteSpace(item!.Barcode)
                        ? item.Barcode
                        : barcode;
                    if (PreviewItems.Any(p => p.Barcode == itemBarcode)) continue;
                    var features = response.itemWiseFeatures?
                        .Where(f => f?.ItemId == item.Id)
                        .Select(f => f!.FeaturesId)
                        .ToList() ?? new List<int>();

                    var row = new PreviewItemRow
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
                        ProductType = item.ProductType ?? "Sell Product",
                        MaterialType = item.MaterialType,
                        CountStockByColor = item.CountStockByColor,
                        CountStockBySize = item.CountStockBySize,
                        Quantity = 0,
                        StockQuantity = response.Stock?.Quantity ?? 0,
                        SalePrice = SharedSalePrice > 0 ? SharedSalePrice : (item.SalePrice ?? 0),
                        OtherCost = SharedOtherCost,
                        CarryingCost = SharedCarryingCost,
                        VatPercentage = SharedVatPercentage,
                        TransportCost = SharedTransportCost,
                        OperationalCost = SharedOperationalCost,
                        TotalAmount = 0,
                        // Carry current image if this row's color matches the currently selected color
                        ImageBase64 = item.ColorId == CurrentItem.ColorId ? CurrentItemImageBase64 : null,
                        BasePurchasePrice = SharedPurchasePrice > 0 ? SharedPurchasePrice : (item.PurchasePrice ?? 0),
                        PurchasePrice = SharedPurchasePrice > 0 ? SharedPurchasePrice : (item.PurchasePrice ?? 0),
                    };

                    rows.Add(row);
                }
            }
            else
            {
                if (!PreviewItems.Any(p => p.Barcode == barcode))
                {
                    var newRow = new PreviewItemRow
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
                        DesignId = CurrentItem.DesignId,          // â† ADD THIS
                        IsNewItem = true,
                        IsSaleable = CurrentItem.IsSaleable,
                        ProductType = CurrentItem.ProductType ?? "Sell Product",
                        CountStockByColor = CurrentItem.CountStockByColor,
                        CountStockBySize = CurrentItem.CountStockBySize,
                        Quantity = 0,
                        StockQuantity = 0,
                        PurchasePrice = SharedPurchasePrice,
                        SalePrice = SharedSalePrice,
                        OtherCost = SharedOtherCost,
                        CarryingCost = SharedCarryingCost,
                        VatPercentage = SharedVatPercentage,
                        TransportCost = SharedTransportCost,
                        OperationalCost = SharedOperationalCost,
                        TotalAmount = 0,
                        ImageBase64 = CurrentItemImageBase64
                    };
                    rows.Add(newRow);
                }
            }

            return rows;
        }
        protected async Task AddItemToGrid()
        {
            // If we're in edit-item mode, delegate to the update path
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

                // Resolve new brand/origin/catalogue/features before adding items
                if (IsNewBrand)
                    CurrentItem.BrandName = BrandSearchText;
                if (IsNewCatalogue)
                    CurrentItem.CatalogueName = CatalogueSearchText;

                bool lookupsResolved = await ResolveNewLookupEntriesAsync(CurrentItem);
                if (!lookupsResolved) return;

                SelectedFeatureIds = CurrentItem.FeatureIds?.ToList() ?? new List<int>();

                // Update each preview row with resolved IDs
                foreach (var row in validRows)
                {
                    if (!row.BrandId.HasValue && CurrentItem.BrandId.HasValue)
                    {
                        row.BrandId = CurrentItem.BrandId;
                        row.BrandName = Brands.FirstOrDefault(b => b.BrandId == CurrentItem.BrandId)?.BrandName ?? row.BrandName;
                    }
                    if (!row.OriginId.HasValue && CurrentItem.OriginId.HasValue)
                    {
                        row.OriginId = CurrentItem.OriginId;
                        row.OriginName = Origins.FirstOrDefault(o => o.OriginId == CurrentItem.OriginId)?.OriginName ?? row.OriginName;
                    }
                    if (!row.CatalogueId.HasValue && CurrentItem.CatalogueId.HasValue)
                    {
                        row.CatalogueId = CurrentItem.CatalogueId;
                        row.CatalogueName = Catalogues.FirstOrDefault(c => c.CatalogueId == CurrentItem.CatalogueId)?.CatalogueName ?? row.CatalogueName;
                    }
                    row.FeatureIds = CurrentItem.FeatureIds?.ToList() ?? new List<int>();
                    row.FeaturesDisplay = string.Join(", ", Features
                        .Where(f => (CurrentItem.FeatureIds ?? new List<int>()).Contains(f.FeatureId))
                        .Select(f => f.FeatureName));
                }

                int addedCount = 0;

                foreach (var row in validRows)
                {
                    // â”€â”€ P.Rate validation (calculated value) â”€â”€
                    if (row.PurchasePrice <= 0)
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Validation",
                            $"Purchase price must be greater than 0 for '{row.ItemName}'. " +
                            $"(Base: {row.BasePurchasePrice:N2} + costs = {row.PurchasePrice:N2})");
                        continue;
                    }

                    // â”€â”€ Duplicate barcode check â”€â”€
                    if (PurchaseItems.Any(i => i.Barcode == row.Barcode))
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Duplicate",
                            $"Barcode '{row.Barcode}' already added. Skipping.");
                        continue;
                    }

                    // â”€â”€ S.Rate validation (only if Saleable) â”€â”€
                    if (row.IsSaleable)
                    {
                        var t = CurrentItem.IsSaleable;
                        if (row.SalePrice <= 0)
                        {
                            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                                $"Sale price required for saleable item '{row.ItemName}'.");
                            continue;
                        }
                        if (row.SalePrice <= row.PurchasePrice)
                        {
                            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                                $"Sale price ({row.SalePrice:N2}) must be greater than " +
                                $"calculated purchase price ({row.PurchasePrice:N2}) for '{row.ItemName}'.");
                            continue;
                        }
                    }

                    PurchaseItems.Add(new PurchaseItemDTO
                    {
                        PurchaseId = Purchase.Id,
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
                        PurchasePrice = row.PurchasePrice,
                        OtherCost = row.OtherCost,
                        CarryingCost = row.CarryingCost,
                        TransportCost = row.TransportCost,
                        OperationalCost = row.OperationalCost,
                        VatPercentage = row.VatPercentage,
                        TotalAmount = row.TotalAmount,
                        IsSaleable = row.IsSaleable,
                        IsConsume = row.IsConsume,
                        SalePrice = row.SalePrice,
                        ProductType = row.ProductType,
                        MaterialType = row.MaterialType,
                        MesurementUnitId = row.MesurementUnitId,
                        MesurementUnitName = !string.IsNullOrWhiteSpace(row.MesurementUnitName)
                            ? row.MesurementUnitName
                            : Units.FirstOrDefault(u => u.Id == row.MesurementUnitId)?.Name,
                        CountStockByColor = row.CountStockByColor,
                        CountStockBySize = row.CountStockBySize,
                        IsNewItem = row.IsNewItem,
                        DesignId = row.DesignId,
                        DesignName = Designs.FirstOrDefault(d => d.Id == row.DesignId)?.Name,
                        CatalogueId = row.CatalogueId,
                        CatalogueName = row.CatalogueName,
                        ImageBase64 = row.ImageBase64,
                        IsActive = true
                    });

                    addedCount++;
                }

                // â”€â”€ Nothing passed validation â€” keep preview grid intact â”€â”€
                if (addedCount == 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Nothing Added",
                        "No items were added. Please fix the validation errors and try again.");
                    return;
                }

                // â”€â”€ Partial success â€” notify how many were skipped â”€â”€
                if (addedCount < validRows.Count)
                {
                    notificationService.Notify(NotificationSeverity.Info, "Partial Add",
                        $"{addedCount} of {validRows.Count} item(s) added. " +
                        $"{validRows.Count - addedCount} item(s) skipped due to validation errors.");
                }

                PreviewItems.Clear();
                await PreviewGrid.Reload();
                await ItemsGrid.Reload();
                CalculateTotals();
                ResetSharedPricing();
                BarcodeSearchText = string.Empty;
                DisableItemFields = false;
                IsNewItemMode = false;

                // Clear only Color, Size, ShadeNo â€” leave rest of left panel intact
                CurrentItem.ColorId = null;
                CurrentItem.SizeId = null;
                CurrentItem.ShadeNo = null;
                CurrentItem.Barcode = null;
                CurrentItemImageBase64 = string.Empty;
                CurrentItemImageMimeType = string.Empty;
                CurrentItem.ImageBase64 = null;

                if (addedCount == validRows.Count)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        $"{addedCount} item(s) added to purchase.");
                }

                return;
            }

            // â”€â”€ Fallback: original single-item add logic â”€â”€
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            CalculateItemTotal();
            if (IsNewBrand)
                CurrentItem.BrandName = BrandSearchText;

            bool resolved = await ResolveNewLookupEntriesAsync(CurrentItem);
            if (!resolved) return;

            var itemToAdd = new PurchaseItemDTO
            {
                PurchaseId = Purchase.Id,
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
                PurchasePrice = CurrentItem.PurchasePrice,
                OtherCost = CurrentItem.OtherCost,
                CarryingCost = CurrentItem.CarryingCost,
                TransportCost = CurrentItem.TransportCost,
                VatPercentage = CurrentItem.VatPercentage,
                TotalAmount = CurrentItem.TotalAmount,
                IsSaleable = CurrentItem.IsSaleable,
                IsConsume = CurrentItem.IsConsume,
                SalePrice = CurrentItem.SalePrice,
                ProductType = CurrentItem.ProductType,
                MaterialType = CurrentItem.MaterialType,
                CountStockByColor = CurrentItem.CountStockByColor,
                CountStockBySize = CurrentItem.CountStockBySize,
                IsNewItem = CurrentItem.IsNewItem,
                MesurementUnitId = CurrentItem.MesurementUnitId,
                DesignId = CurrentItem.DesignId,
                DesignName = Designs.FirstOrDefault(d => d.Id == CurrentItem.DesignId)?.Name,
                CatalogueId = CurrentItem.CatalogueId,
                CatalogueName = CurrentItem.CatalogueId.HasValue
                    ? Catalogues.FirstOrDefault(c => c.CatalogueId == CurrentItem.CatalogueId)?.CatalogueName
                    : CurrentItem.CatalogueName,
                ImageBase64 = CurrentItem.ImageBase64,
            };

            PurchaseItems.Add(itemToAdd);
            await ItemsGrid.Reload();
            CalculateTotals();

            CurrentItem.Quantity = 1;
            CurrentItem.PurchasePrice = 0;
            CurrentItem.OtherCost = null;
            CurrentItem.CarryingCost = null;
            CurrentItem.VatPercentage = null;
            CurrentItem.TotalAmount = 0;
            ResetItemFormSelections();

            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added â€“ ready for next entry");
        }
        private void ResetSharedPricing()
        {
            SharedPurchasePrice = 0;
            SharedSalePrice = 0;
            SharedOtherCost = null;
            SharedCarryingCost = null;
            SharedVatPercentage = null;
            SharedTransportCost = null;
            SharedOperationalCost = null;
        }
        protected async Task EditItem(PurchaseItemDTO item)
        {
            // If we were already editing something, restore it first
            if (IsEditItemMode && _editingItem != null && _editingItem != item)
            {
                // Discard unsaved changes to the previous item â€” nothing to do
                // because the item was never removed from the list
            }

            _editingItem = item;
            IsEditItemMode = true;

            // Clear the preview grid â€” irrelevant while editing a confirmed item
            // Load the item being edited into the preview grid so the user can edit it there
            PreviewItems.Clear();
            ResetSharedPricing();

            PreviewItems.Add(new PreviewItemRow
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
                ProductType = item.ProductType ?? "Sell Product",
                MaterialType = item.MaterialType,
                CountStockByColor = item.CountStockByColor,
                CountStockBySize = item.CountStockBySize,
                // Keep quantity from the saved item â€” the user is editing it
                Quantity = item.Quantity,
                StockQuantity = 0,   // not available from PurchaseItemDTO
                PurchasePrice = item.PurchasePrice,
                SalePrice = item.SalePrice ?? 0,
                OtherCost = item.OtherCost,
                CarryingCost = item.CarryingCost,
                TransportCost = item.TransportCost,
                OperationalCost = item.OperationalCost,
                VatPercentage = item.VatPercentage,
                TotalAmount = item.TotalAmount,
                ImageBase64 = item.ImageBase64,
            });

            PreviewGrid?.Reload();

            // Populate form from the item being edited
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
                CatalogueId = item.CatalogueId,
                CatalogueName = item.CatalogueName,
                ImageBase64 = item.ImageBase64,
            };

            // Sync all auxiliary UI state
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

            // In edit mode the item already exists, so fields should be editable
            // but item identity (barcode) must not be changed
            DisableItemFields = false;
            IsNewItemMode = item.IsNewItem;
            IsProductNameFieldChange = false;

            // Load cascaded dropdowns
            if (item.GroupId.HasValue)
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);

            if (item.SubGroupId.HasValue)
            {
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);
            }

            // Sync shared pricing fields so the bar shows the item's current prices
            SharedPurchasePrice = item.PurchasePrice;
            SharedSalePrice = item.SalePrice ?? 0;
            SharedVatPercentage = item.VatPercentage;
            SharedOtherCost = item.OtherCost;
            SharedCarryingCost = item.CarryingCost;
            SharedTransportCost = item.TransportCost;
            SharedOperationalCost = item.OperationalCost;

            // Scroll / notify
            notificationService.Notify(NotificationSeverity.Info, "Edit Mode",
                $"Editing '{item.ItemName}' â€” make changes then click Update.");

            StateHasChanged();
        }
        protected async Task UpdateEditedItem()
        {
            if (_editingItem == null) return;

            // Pull values from the preview row the user edited
            var row = PreviewItems.FirstOrDefault();
            if (row == null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Nothing to update",
                    "The preview grid is empty.");
                return;
            }

            // Sync preview row values back to CurrentItem so existing validation/build logic works
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
            CurrentItem.PurchasePrice = row.PurchasePrice;
            CurrentItem.SalePrice = row.SalePrice;
            CurrentItem.OtherCost = row.OtherCost;
            CurrentItem.CarryingCost = row.CarryingCost;
            CurrentItem.TransportCost = row.TransportCost;
            CurrentItem.OperationalCost = row.OperationalCost;
            CurrentItem.VatPercentage = row.VatPercentage;
            CurrentItem.TotalAmount = row.TotalAmount;
            CurrentItem.ImageBase64 = row.ImageBase64;

            // --- rest of your existing UpdateEditedItem logic unchanged ---
            var validation = ValidateEditedItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            if (IsNewBrand) CurrentItem.BrandName = BrandSearchText;
            bool resolved = await ResolveNewLookupEntriesAsync(CurrentItem);
            if (!resolved) return;

            CalculateItemTotal();

            var idx = PurchaseItems.IndexOf(_editingItem);
            if (idx < 0)
                PurchaseItems.Add(BuildUpdatedItem());
            else
                PurchaseItems[idx] = BuildUpdatedItem();

            await ItemsGrid.Reload();
            CalculateTotals();
            CancelEditItem();

            notificationService.Notify(NotificationSeverity.Success, "Updated",
                $"'{CurrentItem.ItemName}' updated successfully.");
        }
        private PurchaseItemDTO BuildUpdatedItem() => new PurchaseItemDTO
        {
            Id = _editingItem!.Id,
            PurchaseId = _editingItem.PurchaseId,
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
            PurchasePrice = CurrentItem.PurchasePrice,
            ProductPricePercentage = CurrentItem.ProductPricePercentage,
            OtherCost = CurrentItem.OtherCost,
            CarryingCost = CurrentItem.CarryingCost,
            TransportCost = CurrentItem.TransportCost,
            OperationalCost = CurrentItem.OperationalCost,
            VatPercentage = CurrentItem.VatPercentage,
            VatAmount = CurrentItem.VatAmount,
            TotalAmount = CurrentItem.TotalAmount,
            IsSaleable = CurrentItem.IsSaleable,
            IsConsume = CurrentItem.IsConsume,
            SalePrice = CurrentItem.SalePrice,
            ProductType = CurrentItem.ProductType,
            MaterialType = CurrentItem.MaterialType,
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

        /// <summary>
        /// Validates CurrentItem in edit mode â€” identical to ValidateCurrentItem
        /// except barcode-duplicate check is scoped to other items only.
        /// </summary>
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
            if (CurrentItem.PurchasePrice <= 0)
                return (false, "Purchase price must be greater than 0");

            if (CurrentItem.IsSaleable)
            {
                if (!CurrentItem.SalePrice.HasValue || CurrentItem.SalePrice.Value <= 0)
                    return (false, "Sale price is required for saleable items");
                if (CurrentItem.SalePrice.Value <= CurrentItem.PurchasePrice)
                    return (false, "Sale price must be greater than purchase price");
            }

            // Duplicate barcode check â€” exclude the item being edited
            if (PurchaseItems.Any(i => i != _editingItem && i.Barcode == CurrentItem.Barcode))
                return (false, "Another item with this barcode already exists");

            return (true, string.Empty);
        }

        /// <summary>
        /// Exits edit mode, restores all form state, without navigating away.
        /// </summary>
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

        /// <summary>
        /// Main Cancel button â€” exits edit mode if editing, otherwise navigates to list.
        /// </summary>
        protected void Cancel()
        {
            if (IsEditItemMode)
            {
                CancelEditItem();
                notificationService.Notify(NotificationSeverity.Info, "Cancelled", "Edit cancelled. No changes were saved.");
            }
            else
            {
                NavigationManager.NavigateTo("/PurchaseList");
            }
        }

        protected void DeleteItem(PurchaseItemDTO item)
        {
            PurchaseItems.Remove(item);
            CalculateTotals();
            ItemsGrid?.Reload();
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item removed from purchase");
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
            if (CurrentItem.PurchasePrice <= 0)
                return (false, "Purchase price must be greater than 0");

            if (CurrentItem.IsSaleable)
            {
                if (!CurrentItem.SalePrice.HasValue || CurrentItem.SalePrice.Value <= 0)
                    return (false, "Sale price is required for saleable items");
                if (CurrentItem.SalePrice.Value <= CurrentItem.PurchasePrice)
                    return (false, "Sale price must be greater than purchase price");
            }

            // Exclude the item currently being edited from the duplicate check
            if (PurchaseItems.Any(i => i != _editingItem && i.Barcode == CurrentItem.Barcode))
                return (false, "Item with this barcode already added");

            return (true, string.Empty);
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Save Purchase
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        protected async Task SavePurchase()
        {
            Purchase.StoreId = (await _serviceUnitOfWork.StoreService.GetStores()).Where(x => x.Name.Equals("Head Office", StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Id;
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
            finally
            {
                IsProcessing = false;
            }
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Save As Draft
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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
                    MesurementUnitName = pi.MesurementUnitName,  // â† ADD THIS
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
            finally
            {
                IsProcessing = false;
            }
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Cascade / Product name / Barcode Events
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        protected async Task OnGroupChanged(int? groupId)
        {
            if (!groupId.HasValue) return;
            SubGroups = await LoadSubGroupsByGroup(groupId.Value);
            var sad = await _serviceUnitOfWork.GroupService.GetGroupById(groupId.Value);
            CurrentItem.VatPercentage = sad.VAT;
            SharedVatPercentage = sad.VAT;
            Items = new List<ItemDTO>();
            CurrentItem.GroupId = groupId;
            CurrentItem.SubGroupId = null;
            CurrentItem.ItemId = 0;
            GenerateProductName();
           // await GenerateBarcode();
        }

        // â”€â”€â”€ Color Change: clear image, then load barcode preview â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected async Task OnColorChanged(int? colorId)
        {
            if (!colorId.HasValue) return;
            if (!CurrentItem.SizeId.HasValue) return;
            CurrentItem.ColorId = colorId;

            // Clear the current image so the user uploads a new one for this color
            CurrentItemImageBase64 = string.Empty;
            CurrentItemImageMimeType = string.Empty;
            CurrentItem.ImageBase64 = null;

            GenerateProductName();
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentItem.Barcode))
            {
                await SearchSingleBarcodeAndAddToPreview(CurrentItem.Barcode);
            }

            StateHasChanged();
        }

        // â”€â”€â”€ Size Change: call same barcode search API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected async Task OnSizeChanged(int? sizeId)
        {
            if (!sizeId.HasValue) return;
            CurrentItem.SizeId = sizeId;
            GenerateProductName();
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentItem.Barcode))
            {
                await SearchSingleBarcodeAndAddToPreview(CurrentItem.Barcode);
            }
        }

        private async Task SearchSingleBarcodeAndAddToPreview(string barcode)
        {
            try
            {
                if (PreviewItems.Any(p => p.Barcode == barcode)) return;

                var response = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                PreviewItemRow newRow;

                if (response.Found && response.ItemDetails != null && response.ItemDetails.Any())
                {
                    var item = response.ItemDetails.FirstOrDefault(x => x.Barcode == barcode);

                    if (item == null)
                    {
                        newRow = new PreviewItemRow
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
                            DesignId = CurrentItem.DesignId,   // â† ADD THIS LINE to each block
                            IsNewItem = true,
                            IsSaleable = CurrentItem.IsSaleable,
                            ProductType = CurrentItem.ProductType ?? "Sell Product",
                            CountStockByColor = CurrentItem.CountStockByColor,
                            CountStockBySize = CurrentItem.CountStockBySize,
                            Quantity = 0,
                            StockQuantity = 0,
                            PurchasePrice = SharedPurchasePrice,
                            SalePrice = SharedSalePrice,
                            OtherCost = SharedOtherCost,
                            CarryingCost = SharedCarryingCost,
                            VatPercentage = SharedVatPercentage,
                            TransportCost = SharedTransportCost,
                            OperationalCost = SharedOperationalCost,
                            TotalAmount = 0,
                           
                            // Image is null â€” user must upload for this new color
                            ImageBase64 = null
                        };
                    }
                    else
                    {
                        var features = response.itemWiseFeatures?
                            .Where(f => f?.ItemId == item.Id)
                            .Select(f => f!.FeaturesId)
                            .ToList() ?? new List<int>();

                        newRow = new PreviewItemRow
                        {
                            ItemId = item.Id,
                            ItemName = item.Name ?? string.Empty,
                            Barcode = barcode,
                            ColorId = item.ColorId,
                            ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty,
                            SizeId = CurrentItem.SizeId,
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
                            ProductType = item.ProductType ?? "Sell Product",
                            MaterialType = item.MaterialType,
                            CountStockByColor = item.CountStockByColor,
                            CountStockBySize = item.CountStockBySize,
                            Quantity = 0,
                            StockQuantity = response.Stock?.Quantity ?? 0,
                            PurchasePrice = SharedPurchasePrice > 0 ? SharedPurchasePrice : (item.PurchasePrice ?? 0),
                            SalePrice = SharedSalePrice > 0 ? SharedSalePrice : (item.SalePrice ?? 0),
                            OtherCost = SharedOtherCost,
                            CarryingCost = SharedCarryingCost,
                            VatPercentage = SharedVatPercentage,
                            TransportCost = SharedTransportCost,
                            OperationalCost = SharedOperationalCost,
                            TotalAmount = 0,
                            // Image is null â€” user must upload for this new color
                            ImageBase64 = null
                        };
                    }
                }
                else
                {
                    newRow = new PreviewItemRow
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
                        ProductType = CurrentItem.ProductType ?? "Sell Product",
                        CountStockByColor = CurrentItem.CountStockByColor,
                        CountStockBySize = CurrentItem.CountStockBySize,
                        Quantity = 0,
                        StockQuantity = 0,
                        PurchasePrice = SharedPurchasePrice,
                        SalePrice = SharedSalePrice,
                        OtherCost = SharedOtherCost,
                        CarryingCost = SharedCarryingCost,
                        VatPercentage = SharedVatPercentage,
                        TransportCost = SharedTransportCost,
                        OperationalCost = SharedOperationalCost,
                        TotalAmount = 0,
                        DesignId = CurrentItem.DesignId,  
                        // Image is null â€” user must upload for this new color
                        ImageBase64 = null,
                        FeatureIds = SelectedFeatureIds.ToList(),
                    };
                }

                // Resolve new brand/origin/catalogue/features before creating the item
                if (IsNewBrand)
                    CurrentItem.BrandName = BrandSearchText;
                if (IsNewCatalogue)
                    CurrentItem.CatalogueName = CatalogueSearchText;

                bool lookupsResolved = await ResolveNewLookupEntriesAsync(CurrentItem);
                if (lookupsResolved)
                {
                    SelectedFeatureIds = CurrentItem.FeatureIds?.ToList() ?? new List<int>();
                    if (!newRow.BrandId.HasValue && CurrentItem.BrandId.HasValue)
                        newRow.BrandId = CurrentItem.BrandId;
                    if (!newRow.OriginId.HasValue && CurrentItem.OriginId.HasValue)
                        newRow.OriginId = CurrentItem.OriginId;
                    if (!newRow.CatalogueId.HasValue && CurrentItem.CatalogueId.HasValue)
                        newRow.CatalogueId = CurrentItem.CatalogueId;
                    newRow.FeatureIds = CurrentItem.FeatureIds?.ToList() ?? new List<int>();
                }

                var newItemId = await _serviceUnitOfWork.ItemService.CreateItem(newRow);
                if (newItemId > 0)
                {
                    newRow.ItemId = newItemId;
                    PreviewItems.Add(newRow);
                    PreviewGrid?.Reload();
                    StateHasChanged();
                }
                
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Preview load failed: {ex.Message}");
            }
        }

        private async Task SearchAndUpdatePreviewGrid(string barcode)
        {
            try
            {
                var response = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);
                var newRows = BuildPreviewRowsFromResponse(response, barcode);

                foreach (var row in newRows)
                    PreviewItems.Add(row);

                if (newRows.Any())
                {
                    PreviewGrid?.Reload();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Preview load failed: {ex.Message}");
            }
        }

        protected async Task OnSubGroupChanged(int? subGroupId)
        {
            if (!subGroupId.HasValue) return;
            Items = await LoadItemsBySubGroup(subGroupId.Value);
            Designs = await LoadDesignsBySubGroup(subGroupId.Value);
            CurrentItem.SubGroupId = subGroupId;
            CurrentItem.DesignId = null;
            CurrentItem.ItemId = 0;
           // await GenerateBarcode();
        }

        protected async Task OnDesignChanged(int? designId)
        {
            CurrentItem.DesignId = designId;
            GenerateProductName();
            await GenerateBarcode();
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
            //string size = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? "";
            string catalogue = CatalogueSearchText ?? "";

            if (IsProductNameFieldChange)
            {
                var parts = (CurrentItem.ItemName ?? "")
                    .Split(" - ")
                    .TakeWhile(p => p != color /*&& p != size*/)
                    .ToList();

                parts.AddRange(new[] { color/*, size */}
                    .Where(p => !string.IsNullOrWhiteSpace(p)));

                CurrentItem.ItemName = string.Join(" - ", parts);
            }
            else
            {
                List<string> parts;

                if (!string.IsNullOrWhiteSpace(catalogue))
                    parts = new List<string> { catalogue, color/*, size*/ };
                else
                    parts = new List<string> { subProduct, brand, color/*, size*/ };

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
        private Task LoadItemDetails(int itemId) => Task.CompletedTask; // TODO

        protected async Task OnBarcodeDropdownChanged(object value)
        {
            var barcode = value?.ToString();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                IsSearchingBarcode = true;
                BarcodeSearchText = barcode;
                var result = new BarcodeSearchResponseDTO();
                result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                if (result.Found)
                {
                    if (result.ItemDetails != null)
                    {
                        await PopulateFromExistingItem(result.ItemDetails.FirstOrDefault()!, result.itemWiseFeatures);
                        DisableItemFields = true;
                        IsNewItemMode = false;

                        PreviewItems.Clear();
                        var newRows = BuildPreviewRowsFromResponse(result, barcode);
                        foreach (var row in newRows)
                            PreviewItems.Add(row);

                        PreviewGrid?.Reload();
                        notificationService.Notify(NotificationSeverity.Success, "Success",
                            $"Item loaded! {PreviewItems.Count} variant(s) in preview.");
                        var itemData = AvailableItems.Where(x => x.Barcode == barcode).FirstOrDefault();
                        CurrentItem.ColorId = itemData.ColorId;
                        CurrentItem.SizeId = itemData.SizeId;
                    }
                    else if (result.Item != null)
                    {
                        PopulateFromPurchaseItem(result.Item);
                        DisableItemFields = false;

                        PreviewItems.Clear();
                        var newRows = BuildPreviewRowsFromResponse(result, barcode);
                        foreach (var row in newRows)
                            PreviewItems.Add(row);

                        PreviewGrid?.Reload();
                        notificationService.Notify(NotificationSeverity.Info, "Info", "Item found in purchase history");
                    }
                }
                else
                {
                    CurrentItem.Barcode = barcode;
                    DisableItemFields = false;
                    IsNewItemMode = true;

                    PreviewItems.Clear();
                    var newRows = BuildPreviewRowsFromResponse(result, barcode);
                    foreach (var row in newRows)
                        PreviewItems.Add(row);

                    await PreviewGrid?.Reload();
                    notificationService.Notify(NotificationSeverity.Info, "Create New",
                        "No item found. You can create a new item.");
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
            CurrentItem.MaterialType = item.MaterialType;
            CurrentItem.ProductPricePercentage = item.ProductPricePercentage;
            CurrentItem.SalePrice = item.SalePrice;
            CurrentItem.PurchasePrice = item.PurchasePrice ?? 0;
            CurrentItem.Barcode = BarcodeSearchText;
            CurrentItem.IsNewItem = false;
            CurrentItem.MesurementUnitId = item.MesurementUnitId;
            CurrentItem.CountStockByColor = item.CountStockByColor;
            CurrentItem.CountStockBySize = item.CountStockBySize;
            CurrentItem.ProductType = item.ProductType;
            CurrentItem.CatalogueId = item.CatalogueId;
            CurrentItem.CatalogueName = item.Catalogue;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.ColorName = item.BrandColor;
            CurrentItem.OriginId = item.OriginId;
            CurrentItem.FeatureIds = itemWiseFeatures?.Select(x => x.FeaturesId).ToList() ?? new List<int>();
            CurrentItem.DesignId = item.DesignId;

            SharedPurchasePrice = item.PurchasePrice ?? 0;
            SharedSalePrice = item.SalePrice ?? 0;

            if (item.GroupId.HasValue)
            {
                CurrentItem.GroupId = item.GroupId;
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);
                var group = await _serviceUnitOfWork.GroupService.GetGroupById(item.GroupId.Value);
                if (group != null)
                {
                    CurrentItem.VatPercentage = group.VAT;
                    SharedVatPercentage = group.VAT;
                }
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

            NewFeatureNames = new();
        }

        private void PopulateFromPurchaseItem(PurchaseItemDTO item)
        {
            CurrentItem.ItemId = item.ItemId;
            CurrentItem.ItemName = item.ItemName;
            CurrentItem.Quantity = item.Quantity;
            CurrentItem.TotalAmount = item.TotalAmount;
            CurrentItem.GroupName = item.GroupName;
            CurrentItem.SubGroupName = item.SubGroupName;
            CurrentItem.ShadeNo = item.ShadeNo;
            CurrentItem.ColorId = item.ColorId;
            CurrentItem.SizeId = item.SizeId;
            CurrentItem.PurchasePrice = item.PurchasePrice;
            CurrentItem.SalePrice = item.SalePrice;
            CurrentItem.ProductType = item.ProductType;
            CurrentItem.Barcode = BarcodeSearchText;
            CurrentItem.IsNewItem = false;
            CurrentItem.CatalogueName = item.CatalogueName;
            CurrentItem.CatalogueId = item.CatalogueId;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.OriginId = item.OriginId;

            SharedPurchasePrice = item.PurchasePrice;
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
                    ExistingBarcode = CurrentItem.Barcode,
                    SubGroupId = CurrentItem.SubGroupId,
                    DesignId = CurrentItem.DesignId
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
                notificationService.Notify(NotificationSeverity.Info, "Create Mode",
                    "Fill in the details to create a new item");
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

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Calculation
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
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
        protected void ResetLeftPanel()
        {
            // â”€â”€ Header fields 
            Purchase.SupplierId = null;
            //Purchase.SystemInvoiceNo = null;
            Purchase.BillInvoiceNumber = null;
            //Purchase.PurchaseDate = default;
            Purchase.StoreId = null;
            Purchase.IsVatIncluded = false;

            // â”€â”€ Item-type checkboxes 
            CurrentItem.IsSaleable = false;
            CurrentItem.IsConsume = false;
            CurrentItem.IsRawMaterial = false;

            // â”€â”€ Cascade dropdowns â”€â”€â”€
            CurrentItem.GroupId = null;
            CurrentItem.SubGroupId = null;
            CurrentItem.DesignId = null;
            SubGroups = new List<SubGroupModelDTO>();
            Designs = new List<DesignModelDTO>();

            // â”€â”€ UoM â”€â”€â”€â”€â”€â”€â”€â”€â”€
            CurrentItem.MesurementUnitId = null;

            // â”€â”€ ShadeNo â”€â”€â”€â”€â”€
            CurrentItem.ShadeNo = null;

            // â”€â”€ Brand â”€â”€â”€â”€â”€â”€â”€
            BrandSearchText = string.Empty;
            BrandSuggestions = new List<ItemBrandDTO>();
            SelectedBrandId = null;
            IsNewBrand = false;
            CurrentItem.BrandId = null;
            CurrentItem.BrandName = null;

            // â”€â”€ Catalogue â”€â”€â”€
            CatalogueSearchText = string.Empty;
            CatalogueSuggestions = new List<ItemCatalogueDTO>();
            SelectedCatalogueId = null;
            IsNewCatalogue = false;
            CurrentItem.CatalogueId = null;
            CurrentItem.CatalogueName = null;

            // â”€â”€ Origin â”€â”€â”€â”€â”€â”€
            OriginSearchText = string.Empty;
            OriginSuggestions = new List<ItemOriginDTO>();
            SelectedOriginId = null;
            IsNewOrigin = false;
            CurrentItem.OriginId = null;
            CurrentItem.OriginName = null;

            // â”€â”€ Features â”€â”€â”€â”€
            SelectedFeatureIds = new List<int>();
            NewFeatureNames = new List<string>();
            NewFeatureInput = string.Empty;
            DisableItemFields = false;
            IsEditItemMode = false;
            IsProcessing = false;
            StateHasChanged();
        }
        private bool HasUnsavedData()
        {
            return Purchase.SupplierId.HasValue
                || !string.IsNullOrWhiteSpace(Purchase.BillInvoiceNumber)
                || Purchase.StoreId.HasValue
                || Purchase.IsVatIncluded
                || CurrentItem.IsSaleable
                || CurrentItem.IsConsume
                || CurrentItem.IsRawMaterial
                || CurrentItem.GroupId.HasValue
                || CurrentItem.SubGroupId.HasValue
                || CurrentItem.DesignId.HasValue
                || CurrentItem.MesurementUnitId.HasValue
                || !string.IsNullOrWhiteSpace(CurrentItem.ShadeNo)
                || !string.IsNullOrWhiteSpace(BrandSearchText)
                || SelectedBrandId.HasValue
                || !string.IsNullOrWhiteSpace(CatalogueSearchText)
                || SelectedCatalogueId.HasValue
                || !string.IsNullOrWhiteSpace(OriginSearchText)
                || SelectedOriginId.HasValue
                || SelectedFeatureIds.Any()
                || NewFeatureNames.Any()
                || !string.IsNullOrWhiteSpace(NewFeatureInput);
        }
        protected async Task TryResetLeftPanel()
        {
            if (!HasUnsavedData())
            {
                ResetLeftPanel();
                return;
            }

            var confirmed = await dialogService.Confirm(
                "All unsaved data will be lost. Are you sure you want to reset the form?",
                "Reset Form?",
                new ConfirmOptions
                {
                    OkButtonText = "Yes, Reset",
                    CancelButtonText = "No, Keep Data",
                    CloseDialogOnOverlayClick = true
                }) ?? false;

            if (confirmed)
                ResetLeftPanel();
        }
        protected void OnBrandLoadData(LoadDataArgs args)
        {
            var text = args.Filter;

            if (string.IsNullOrWhiteSpace(text))
            {
                BrandSuggestions = new List<ItemBrandDTO>();
            }
            else
            {
                BrandSuggestions = Brands
                    .Where(b => b.BrandName.Contains(text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            StateHasChanged();
        }
        protected void OnDiscountChanged() => CalculateTotals();
        protected void OnVatChanged() => CalculateTotals();
        protected void OnPaidAmountChanged() => CalculateTotals();
        public void Dispose() 
        {
            ItemsGrid?.Dispose();
            NavigationGuard.IsGuardActive = false;
        } 
    }
}