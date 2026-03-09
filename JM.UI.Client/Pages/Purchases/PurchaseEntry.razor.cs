using JM.UI.Client.Pages.Dialog;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.ItemFeatures;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Model.Items;
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

        // ─── Purchase Data ──────────────────────────────────────────────
        protected PurchaseDTO Purchase { get; set; } = new();
        protected List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
        protected PurchaseItemDTO CurrentItem { get; set; } = new();
        protected PurchaseItemDTO? _editingItem = null;

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

        // Brand auto-complete
        protected string BrandSearchText { get; set; } = string.Empty;
        protected List<ItemBrandDTO> BrandSuggestions { get; set; } = new List<ItemBrandDTO>();
        protected int? SelectedBrandId { get; set; }
        protected bool IsNewBrand { get; set; } = false;

        // Origin auto-complete
        protected string OriginSearchText { get; set; } = string.Empty;
        protected IEnumerable<ItemOriginDTO> OriginSuggestions { get; set; } = new List<ItemOriginDTO>();
        protected int? SelectedOriginId { get; set; }
        protected bool IsNewOrigin { get; set; } = false;

        // Features multi-select
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
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

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
                // Load items from the navigation property
                PurchaseItems = purchase.PurchaseItems?.ToList() ?? new List<PurchaseItemDTO>();

                CurrentItem = CreateNewItem();
                CalculateTotals();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        // ═══════════════════════════════════════════════════════════════
        // FIX: LoadDraft — restores ALL fields including Brand/Origin/Features
        // ═══════════════════════════════════════════════════════════════
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

                // ── Restore Purchase header ──────────────────────────
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

                // ── Restore line items ───────────────────────────────
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
                    // FIX: guard against null FeaturesDisplay
                    FeaturesDisplay = di.FeaturesDisplay ?? string.Empty,
                    Barcode = di.Barcode,
                    Quantity = di.Quantity,
                    PurchasePrice = di.PurchasePrice,
                    ProductPricePercentage = di.ProductPricePercentage,
                    OtherCost = di.OtherCost,
                    CarryingCost = di.CarryingCost,
                    // FIX: TransportCost was missing
                    TransportCost = di.TransportCost,
                    OperationalCost = di.OperationalCost,
                    VatPercentage = di.VatPercentage,
                    VatAmount = di.VatAmount,
                    TotalAmount = di.TotalAmount,
                    IsSaleable = di.IsSaleable,
                    // FIX: IsConsume was missing
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

                // ── Pre-load cascaded lookups for the last/first item so
                //    dropdowns are not empty when user edits from draft ─
                var firstItem = PurchaseItems.FirstOrDefault();
                if (firstItem?.GroupId.HasValue == true)
                {
                    SubGroups = await LoadSubGroupsByGroup(firstItem.GroupId.Value);
                }
                if (firstItem?.SubGroupId.HasValue == true)
                {
                    Items = await LoadItemsBySubGroup(firstItem.SubGroupId.Value);
                    Designs = await LoadDesignsBySubGroup(firstItem.SubGroupId.Value);
                }

                CurrentItem = CreateNewItem();

                // FIX: recalculate totals after loading items
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
                // 1. Brand
                if (IsNewBrand && !string.IsNullOrWhiteSpace(item.BrandName))
                {
                    var brandResult = await _serviceUnitOfWork.ItemBrandService.SaveItemBrand(
                        new ItemBrandDTO { BrandName = item.BrandName });

                    if (brandResult == null || !brandResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error",
                            $"Failed to create brand '{item.BrandName}'");
                        return false;
                    }

                    item.BrandId = Convert.ToInt32(brandResult.Id);
                    Brands = await LoadBrands();
                    IsNewBrand = false;
                }

                // 2. Origin
                if (IsNewOrigin && !string.IsNullOrWhiteSpace(item.OriginName))
                {
                    var originResult = await _serviceUnitOfWork.ItemOriginService.SaveItemOrigin(
                        new ItemOriginDTO { OriginName = item.OriginName });

                    if (originResult == null || !originResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error",
                            $"Failed to create origin '{item.OriginName}'");
                        return false;
                    }

                    item.OriginId = Convert.ToInt32(originResult.Id);
                    Origins = await LoadOrigins();
                    IsNewOrigin = false;
                }

                // 3. New Features
                var allFeatureIds = SelectedFeatureIds.ToList();

                foreach (var fname in NewFeatureNames.ToList())
                {
                    var featureResult = await _serviceUnitOfWork.ItemFeatureService.SaveItemFeature(
                        new ItemFeatureDTO { FeatureName = fname });

                    if (featureResult == null || !featureResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error",
                            $"Failed to create feature '{fname}'");
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
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Lookup resolution failed: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // Item Management
        // ═══════════════════════════════════════════════════════════════
        protected async Task AddItemToGrid()
        {
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            CalculateItemTotal();

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
                Catalogue = CurrentItem.Catalogue
            };

            PurchaseItems.Add(itemToAdd);
            await ItemsGrid.Reload();
            CalculateTotals();

            // Partial reset — keep header-level selections
            CurrentItem.Quantity = 1;
            CurrentItem.PurchasePrice = 0;
            CurrentItem.OtherCost = null;
            CurrentItem.CarryingCost = null;
            CurrentItem.VatPercentage = null;
            CurrentItem.TotalAmount = 0;
            ResetItemFormSelections();

            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added – ready for next entry");
        }

        // ═══════════════════════════════════════════════════════════════
        // Edit Item — restores ALL form state including cascaded dropdowns
        // ═══════════════════════════════════════════════════════════════
        protected async Task EditItem(PurchaseItemDTO item)
        {
            _editingItem = item;

            // ── 1. Copy all scalar fields into CurrentItem ───────────────
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
                Catalogue = item.Catalogue,
                IsNewItem = item.IsNewItem,
                IsActive = item.IsActive,
            };

            // ── 2. Restore Brand / Origin / Features UI state ────────────
            BrandSearchText = item.BrandName ?? string.Empty;
            OriginSearchText = item.OriginName ?? string.Empty;
            SelectedFeatureIds = item.FeatureIds?.ToList() ?? new List<int>();
            NewFeatureNames = new List<string>();
            NewFeatureInput = string.Empty;

            IsNewBrand = false;
            IsNewOrigin = false;

            // ── 3. Restore barcode search text ───────────────────────────
            BarcodeSearchText = item.Barcode ?? string.Empty;
            DisableItemFields = false;     // allow editing
            IsNewItemMode = item.IsNewItem;

            // ── 4. Reload cascaded dropdowns ─────────────────────────────
            if (item.GroupId.HasValue)
            {
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);
            }

            if (item.SubGroupId.HasValue)
            {
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);
            }

            // ── 5. Remove from grid and recalculate ──────────────────────
            PurchaseItems.Remove(item);
            CalculateTotals();
            await ItemsGrid.Reload();

            StateHasChanged();
        }

        protected void Cancel()
        {
            if (_editingItem != null)
            {
                PurchaseItems.Add(_editingItem);
                _editingItem = null;
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

            if (PurchaseItems.Any(i => i.Barcode == CurrentItem.Barcode))
                return (false, "Item with this barcode already added");

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
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to save purchase: {ex.Message}");
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

        // ═══════════════════════════════════════════════════════════════
        // FIX: SaveAsDraft — all fields correctly mapped
        // ═══════════════════════════════════════════════════════════════
        protected async Task SaveAsDraft()
        {
            try
            {
                // ── 1. Ask user for draft name ───────────────────────────────
                var result = await dialogService.OpenAsync<SaveDraftDialog>("Save as Draft",
                    new Dictionary<string, object>() { { "DraftName", "" } },
                    new DialogOptions() { Width = "400px" });

                if (result == null || string.IsNullOrWhiteSpace(result.ToString())) return;

                IsProcessing = true;

                // ── 2. Build draft header ────────────────────────────────────
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

                // ── 3. Build draft items ─────────────────────────────────────
                var draftItems = PurchaseItems.Select(pi => new PurchaseDraftItemDTO
                {
                    // Identity
                    ItemId = pi.ItemId,
                    ItemName = pi.ItemName,

                    // Group / SubGroup / Design
                    GroupId = pi.GroupId,
                    GroupName = pi.GroupName,
                    SubGroupId = pi.SubGroupId,
                    SubGroupName = pi.SubGroupName,
                    DesignId = pi.DesignId,
                    DesignName = pi.DesignName,

                    // Attributes
                    ShadeNo = pi.ShadeNo,
                    ColorId = pi.ColorId,
                    ColorName = pi.ColorName,
                    SizeId = pi.SizeId,
                    SizeName = pi.SizeName,
                    Catalogue = pi.Catalogue,
                    MaterialType = pi.MaterialType,

                    // Brand
                    BrandId = pi.BrandId,
                    BrandName = pi.BrandName,

                    // Origin
                    OriginId = pi.OriginId,
                    OriginName = pi.OriginName,

                    // Features
                    FeatureIds = pi.FeatureIds ?? new List<int>(),
                    FeaturesDisplay = pi.FeaturesDisplay ?? string.Empty,

                    // Barcode / UoM
                    Barcode = pi.Barcode,
                    MesurementUnitId = pi.MesurementUnitId,

                    // Pricing
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

                    // Flags
                    IsSaleable = pi.IsSaleable,
                    IsConsume = pi.IsConsume,
                    ProductType = pi.ProductType,
                    CountStockByColor = pi.CountStockByColor,
                    CountStockBySize = pi.CountStockBySize,
                    IsNewItem = pi.IsNewItem,
                    IsActive = pi.IsActive
                }).ToList();

                // ── 4. Save ──────────────────────────────────────────────────
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
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to save draft: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
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
            await GenerateBarcode();
        }

        protected async Task OnSizeChanged(int? sizeId)
        {
            if (!sizeId.HasValue) return;
            CurrentItem.SizeId = sizeId;
            GenerateProductName();
            await GenerateBarcode();
        }

        protected async Task OnSubGroupChanged(int? subGroupId)
        {
            if (!subGroupId.HasValue) return;
            Items = await LoadItemsBySubGroup(subGroupId.Value);
            Designs = await LoadDesignsBySubGroup(subGroupId.Value);
            CurrentItem.SubGroupId = subGroupId;
            CurrentItem.DesignId = null;
            CurrentItem.ItemId = 0;
            GenerateProductName();
            await GenerateBarcode();
        }

        protected void OnDesignChanged(int? designId) => CurrentItem.DesignId = designId;
        protected void OnBrandChanged(string? brand) { CurrentItem.BrandName = brand; GenerateProductName(); }
        protected void OnCatalogueChanged(string? catalogue) { CurrentItem.Catalogue = catalogue; GenerateProductName(); }

        private void GenerateProductName()
        {
            if (CurrentItem.ItemId != 0 && !IsNewItemMode) return;

            string subProduct = SubGroups.FirstOrDefault(s => s.Id == CurrentItem.SubGroupId)?.Name ?? "";
            string brand = CurrentItem.BrandName ?? "";
            string color = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name ?? "";
            string size = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name ?? "";
            string catalogue = CurrentItem.Catalogue ?? "";

            CurrentItem.ItemName = (!string.IsNullOrWhiteSpace(catalogue)
                ? $"{catalogue}{color}{size}"
                : $"{subProduct}{brand}{color}{size}")
                .Replace(" ", "").Trim();

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

        // ═══════════════════════════════════════════════════════════════
        // Barcode Search / Create New Item
        // ═══════════════════════════════════════════════════════════════
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
                        await PopulateFromExistingItem(result.ItemDetails);
                        DisableItemFields = true;
                        IsNewItemMode = false;
                        notificationService.Notify(NotificationSeverity.Success, "Success", "Item loaded!");
                    }
                    else if (result.Item != null)
                    {
                        PopulateFromPurchaseItem(result.Item);
                        DisableItemFields = false;
                        notificationService.Notify(NotificationSeverity.Info, "Info", "Item found in purchase history");
                    }
                }
                else
                {
                    CurrentItem.Barcode = barcode;
                    DisableItemFields = false;
                    IsNewItemMode = true;
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

        private async Task PopulateFromExistingItem(ItemDTO item)
        {
            CurrentItem.ItemId = item.Id;
            CurrentItem.ItemName = item.Name;
            CurrentItem.ShadeNo = item.ShadeNo;
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
            CurrentItem.Catalogue = item.Catalogue;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.OriginId = item.OriginId;
            CurrentItem.FeatureIds = item.FeatureIds;
            CurrentItem.DesignId = item.DesignId;

            // ─── Cascade: load SubGroups for this Group ──────────────────
            if (item.GroupId.HasValue)
            {
                CurrentItem.GroupId = item.GroupId;
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);

                // Also load the Group's VAT
                var group = await _serviceUnitOfWork.GroupService.GetGroupById(item.GroupId.Value);
                if (group != null) CurrentItem.VatPercentage = group.VAT;
            }

            // ─── Cascade: load Items + Designs for this SubGroup ─────────
            if (item.SubGroupId.HasValue)
            {
                CurrentItem.SubGroupId = item.SubGroupId;
                Items = await LoadItemsBySubGroup(item.SubGroupId.Value);
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);

                CurrentItem.DesignId = item.DesignId;
            }
            //CurrentItem.DesignId = item.DesignId;   

            // ─── Features ────────
            SelectedFeatureIds = item.FeatureIds ?? new List<int>();
            NewFeatureNames = new();

            // ─── Brand / Origin autocomplete text ──────
            BrandSearchText = item.BrandId.HasValue
                ? (Brands.FirstOrDefault(b => b.BrandId == item.BrandId)?.BrandName ?? item.BrandColor ?? "")
                : (item.BrandColor ?? "");

            OriginSearchText = item.OriginId.HasValue
                ? (Origins.FirstOrDefault(o => o.OriginId == item.OriginId)?.OriginName ?? item.Origin ?? "")
                : (item.Origin ?? "");

            SelectedFeatureIds = item.FeatureIds ?? new List<int>();
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
            CurrentItem.Catalogue = item.Catalogue;
            CurrentItem.BrandId = item.BrandId;
            CurrentItem.OriginId = item.OriginId;

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
                    GroupId = CurrentItem.GroupId
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentItem.Barcode = barcode;
                BarcodeSearchText = barcode;
                GenerateProductName();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to generate barcode: {ex.Message}");
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