using JM.UI.Client.Pages.Dialog;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Groups;
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

        // Purchase Data
        protected PurchaseDTO Purchase { get; set; } = new();
        protected List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
        protected PurchaseItemDTO CurrentItem { get; set; } = new();

        // Lookup Data
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();
        protected IEnumerable<ItemDTO> Items { get; set; } = new List<ItemDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();

        // UI State
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Entry" : "New Purchase Entry";

        // NEW: Create Item on the Go state
        protected bool IsNewItemMode { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;


        // Grid Reference
        protected RadzenDataGrid<PurchaseItemDTO> ItemsGrid = default!;

        // Product Type Options
        protected List<string> ProductTypes = new()
        {
            "Sell Product",
            "Raw Material",
            "Both",
            "Consume",
            "Combo Package"
        };

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();

            if (IsDraftMode)
            {
                await LoadDraft();
            }
            else if (IsEditMode)
            {
                await LoadPurchase();
            }
            else
            {
                InitializePurchase();
            }
        }

        // =============================================
        // Initialization Methods
        // =============================================
        private void InitializePurchase()
        {
            Purchase = _serviceUnitOfWork.PurchaseService.CreateNewPurchase();
            PurchaseItems = new List<PurchaseItemDTO>();
            CurrentItem = CreateNewItem();
        }

        // Add new methods
        // Update LoadDraft method (already provided in previous response)
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

                // Map draft to Purchase
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
                    DueAmount = draft.DueAmount,
                    Remarks = draft.Remarks
                };

                // Map draft items to purchase items
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
                    Barcode = di.Barcode,
                    Quantity = di.Quantity,
                    PurchasePrice = di.PurchasePrice,
                    ProductPricePercentage = di.ProductPricePercentage,
                    OtherCost = di.OtherCost,
                    CarryingCost = di.CarryingCost,
                    OperationalCost = di.OperationalCost,
                    VatPercentage = di.VatPercentage,
                    VatAmount = di.VatAmount,
                    TotalAmount = di.TotalAmount,
                    IsSaleable = di.IsSaleable,
                    SalePrice = di.SalePrice,
                    ProductType = di.ProductType,
                    MaterialType = di.MaterialType,
                    Origin = di.Origin,
                    Features = di.Features,
                    BrandColor = di.BrandColor,
                    MesurementUnitId = di.MesurementUnitId,
                    CountStockByColor = di.CountStockByColor,
                    CountStockBySize = di.CountStockBySize,
                    IsNewItem = di.IsNewItem,
                    IsActive = di.IsActive
                }).ToList();

                CurrentItem = CreateNewItem();

                notificationService.Notify(NotificationSeverity.Info, "Draft Loaded", $"Draft '{draft.DraftName}' loaded successfully");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load draft: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseDraftList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task SaveAsDraft()
        {
            try
            {
                // Show dialog to get draft name
                var result = await dialogService.OpenAsync<SaveDraftDialog>("Save as Draft",
                    new Dictionary<string, object>() { { "DraftName", "" } },
                    new DialogOptions() { Width = "400px" });

                if (result == null || string.IsNullOrWhiteSpace(result.ToString()))
                    return;

                IsProcessing = true;

                var draftDTO = new PurchaseDraftDTO
                {
                    DraftName = result.ToString(),
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
                    CreatedBy = 1 // TODO: Get from current user
                };

                var draftItems = PurchaseItems.Select(pi => new PurchaseDraftItemDTO
                {
                    ItemId = pi.ItemId,
                    ItemName = pi.ItemName,
                    GroupId = pi.GroupId,
                    GroupName = pi.GroupName,
                    SubGroupId = pi.SubGroupId,
                    SubGroupName = pi.SubGroupName,
                    ShadeNo = pi.ShadeNo,
                    ColorId = pi.ColorId,
                    ColorName = pi.ColorName,
                    SizeId = pi.SizeId,
                    SizeName = pi.SizeName,
                    Barcode = pi.Barcode,
                    Quantity = pi.Quantity,
                    PurchasePrice = pi.PurchasePrice,
                    ProductPricePercentage = pi.ProductPricePercentage,
                    OtherCost = pi.OtherCost,
                    CarryingCost = pi.CarryingCost,
                    OperationalCost = pi.OperationalCost,
                    VatPercentage = pi.VatPercentage,
                    VatAmount = pi.VatAmount,
                    TotalAmount = pi.TotalAmount,
                    IsSaleable = pi.IsSaleable,
                    SalePrice = pi.SalePrice,
                    ProductType = pi.ProductType,
                    MaterialType = pi.MaterialType,
                    Origin = pi.Origin,
                    Features = pi.Features,
                    BrandColor = pi.BrandColor,
                    MesurementUnitId = pi.MesurementUnitId,
                    CountStockByColor = pi.CountStockByColor,
                    CountStockBySize = pi.CountStockBySize,
                    IsNewItem = pi.IsNewItem,
                    IsActive = pi.IsActive
                }).ToList();

                var saveResult = await _serviceUnitOfWork.PurchaseService.SavePurchaseDraft(draftDTO, draftItems);

                if (saveResult.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", saveResult.Message ?? "Draft saved successfully");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", saveResult.Message ?? "Failed to save draft");
                }
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

        private PurchaseItemDTO CreateNewItem()
        {
            return new PurchaseItemDTO
            {
                IsSaleable = true,
                ProductType = "Sell Product",
                Quantity = 1,
                IsActive = true,
                IsNewItem = false,
                CountStockByColor = false,
                CountStockBySize = false
            };
        }

        // =============================================
        // Data Loading Methods
        // =============================================
        private async Task LoadLookupData()
        {
            try
            {
                // Load all lookup data (implement these methods in your services)
                Suppliers = await LoadSuppliers();
                Stores = await LoadStores();
                Groups = await LoadGroups();
                Colors = await LoadColors();
                Sizes = await LoadSizes();
                Units = await LoadUnits();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        private async Task LoadPurchase()
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
                PurchaseItems = purchase.PurchaseItems;
                CurrentItem = CreateNewItem();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // =============================================
        // Lookup Load Methods (Implement based on your services)
        // =============================================
        private async Task<IEnumerable<SupplierModelDTO>> LoadSuppliers()
        {
            // TODO: Implement supplier loading from your service
            return await _serviceUnitOfWork.SupplierService.GetSuppliers() ?? new List<SupplierModelDTO>();
        }

        private async Task<IEnumerable<StoreDTO>> LoadStores()
        {
            // TODO: Implement store loading from your service
            return await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
        }

        private async Task<IEnumerable<GroupModelDTO>> LoadGroups()
        {
            // TODO: Implement group loading from your service
            return await _serviceUnitOfWork.GroupService.GetGroups() ?? new List<GroupModelDTO>();
        }

        private async Task<IEnumerable<ColorsDTO>> LoadColors()
        {
            // TODO: Implement color loading from your service
            return await _serviceUnitOfWork.ColorsService.GetColorss() ?? new List<ColorsDTO>();
        }

        private async Task<IEnumerable<SizesDTO>> LoadSizes()
        {
            // TODO: Implement size loading from your service
            return await _serviceUnitOfWork.SizesService.GetSizess() ?? new List<SizesDTO>();
        }

        private async Task<IEnumerable<MesurementUnitModelDTO>> LoadUnits()
        {
            // TODO: Implement unit loading from your service
            return await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits() ?? new List<MesurementUnitModelDTO>();
        }

        protected async Task OnGroupChanged(int? groupId)
        {
            if (groupId.HasValue)
            {
                SubGroups = await LoadSubGroupsByGroup(groupId.Value);
                Items = new List<ItemDTO>();
                CurrentItem.GroupId = groupId;
                CurrentItem.SubGroupId = null;
                CurrentItem.ItemId = 0;
            }
        }
        protected void OnColorChanged(int? colorId)
        {
            if (!colorId.HasValue) return;

            var col = Colors.FirstOrDefault(x => x.Id == colorId);

            if (col != null && !string.IsNullOrWhiteSpace(col.ColorCode))
            {
                CurrentItem.Barcode = $"{CurrentItem.Barcode}{col.ColorCode}";
            }
        }

        protected async Task OnSizeChanged(int? sizeId)
        {
            if (sizeId.HasValue)
            {
                var col = Sizes.Where(x=>x.Id== sizeId).FirstOrDefault().Name;
                CurrentItem.Barcode = CurrentItem.Barcode  + col;
            }
        }

        protected async Task OnSubGroupChanged(int? subGroupId)
        {
            if (subGroupId.HasValue)
            {
                Items = await LoadItemsBySubGroup(subGroupId.Value);
                CurrentItem.SubGroupId = subGroupId;
                CurrentItem.ItemId = 0;
            }
        }

        protected async Task OnItemChanged(int itemId)
        {
            if (itemId > 0)
            {
                await LoadItemDetails(itemId);
            }
        }

        private async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId)
        {
            // TODO: Implement
            return await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId) ?? new List<SubGroupModelDTO>();
        }

        private async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId)
        {
            // TODO: Implement
            return await _serviceUnitOfWork.ItemService.LoadItemsBySubGroup(subGroupId) ?? new List<ItemDTO>();
        }

        private async Task LoadItemDetails(int itemId)
        {
            // TODO: Implement to load item details and populate current item
        }

        // =============================================
        // NEW: Barcode Search and Create Item on the Go
        // =============================================
        protected async Task SearchByBarcode()
        {
            if (string.IsNullOrWhiteSpace(BarcodeSearchText))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "Please enter a barcode to search");
                return;
            }

            try
            {
                IsSearchingBarcode = true;
                var result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(BarcodeSearchText);

                if (result.Found)
                {
                    if (result.ItemDetails != null)
                    {
                        // Existing item found - populate and disable fields
                        PopulateFromExistingItem(result.ItemDetails);
                        DisableItemFields = true;
                        IsNewItemMode = false;
                        notificationService.Notify(NotificationSeverity.Success, "Success", "Existing item found!");
                    }
                    else if (result.Item != null)
                    {
                        // Item from purchase history found
                        PopulateFromPurchaseItem(result.Item);
                        DisableItemFields = false;
                        notificationService.Notify(NotificationSeverity.Info, "Info", "Item found in purchase history");
                    }
                }
                else
                {
                    // No item found - enable create new item mode
                    CurrentItem.Barcode = BarcodeSearchText;
                    DisableItemFields = false;
                    IsNewItemMode = true;
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

        private void PopulateFromExistingItem(ItemDTO item)
        {
            CurrentItem.ItemId = item.Id;
            CurrentItem.ItemName = item.Name;
            CurrentItem.GroupId = item.GroupId;
            CurrentItem.SubGroupId = item.SubGroupId;
            CurrentItem.ShadeNo = item.ShadeNo;
            CurrentItem.MaterialType = item.MaterialType;
            CurrentItem.Origin = item.Origin;
            CurrentItem.Features = item.Features;
            CurrentItem.BrandColor = item.BrandColor;
            CurrentItem.ProductPricePercentage = item.ProductPricePercentage;
            CurrentItem.SalePrice = item.SalePrice;
            CurrentItem.PurchasePrice = item.PurchasePrice ?? 0;
            CurrentItem.Barcode = BarcodeSearchText;
            CurrentItem.IsNewItem = false;
            CurrentItem.MesurementUnitId = item.MesurementUnitId;
            CurrentItem.CountStockByColor = item.CountStockByColor;
            CurrentItem.CountStockBySize = item.CountStockBySize;
            CurrentItem.ProductType = item.ProductType;
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
        }

        protected async Task GenerateBarcode()
        {
            try
            {
                var request = new BarcodeGenerationRequestDTO
                {
                    ShadeNo = CurrentItem.ShadeNo,
                    ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.ColorCode,
                    SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name,
                    ItemId = CurrentItem.ItemId > 0 ? CurrentItem.ItemId : CurrentItem.GroupId,
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentItem.Barcode = barcode;
                BarcodeSearchText = barcode;
                notificationService.Notify(NotificationSeverity.Success, "Success", "Barcode generated successfully");
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
                // Clear item selection but keep entered data
                CurrentItem.ItemId = 0;
                notificationService.Notify(NotificationSeverity.Info, "Create Mode", "Fill in the details to create a new item");
            }
            else
            {
                // Back to selection mode
                CurrentItem = CreateNewItem();
                DisableItemFields = false;
                BarcodeSearchText = string.Empty;
            }

            StateHasChanged();
        }

        protected void ClearBarcodeSearch()
        {
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            IsNewItemMode = false;
            CurrentItem = CreateNewItem();
            StateHasChanged();
        }

        // =============================================
        // Item Management Methods
        // =============================================
        protected void AddItemToGrid()
        {
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
                return;
            }

            CalculateItemTotal();

            var itemToAdd = new PurchaseItemDTO
            {
                ItemId = CurrentItem.ItemId,
                ItemName = CurrentItem.ItemName,
                Barcode = CurrentItem.Barcode,
                GroupId = CurrentItem.GroupId,
                GroupName = CurrentItem.GroupName,
                SubGroupId = CurrentItem.SubGroupId,
                SubGroupName = CurrentItem.SubGroupName,
                ShadeNo = CurrentItem.ShadeNo,
                ColorId = CurrentItem.ColorId,
                ColorName = CurrentItem.ColorName,
                SizeId = CurrentItem.SizeId,
                SizeName = CurrentItem.SizeName,
                MesurementUnitId = CurrentItem.MesurementUnitId,
                Quantity = CurrentItem.Quantity,
                PurchasePrice = CurrentItem.PurchasePrice,
                OtherCost = CurrentItem.OtherCost,
                CarryingCost = CurrentItem.CarryingCost,
                VatPercentage = CurrentItem.VatPercentage,
                TotalAmount = CurrentItem.TotalAmount,
                IsSaleable = CurrentItem.IsSaleable,
                SalePrice = CurrentItem.SalePrice,
                ProductType = CurrentItem.ProductType,
                MaterialType = CurrentItem.MaterialType,
                Origin = CurrentItem.Origin,
                BrandColor = CurrentItem.BrandColor,
                CountStockByColor = CurrentItem.CountStockByColor,
                CountStockBySize = CurrentItem.CountStockBySize,
                IsNewItem = CurrentItem.IsNewItem,
            };

            PurchaseItems.Add(itemToAdd);
            CalculateTotals();
            ItemsGrid?.Reload();

            CurrentItem.Quantity = 1;                   
            CurrentItem.PurchasePrice = 0;
            CurrentItem.OtherCost = null;
            CurrentItem.CarryingCost = null;
            CurrentItem.VatPercentage = null;
            CurrentItem.TotalAmount = 0;

            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added – ready for next entry");

        }

        //protected void AddItemToGrid()
        //{
        //    var validation = ValidateCurrentItem();
        //    if (!validation.IsValid)
        //    {
        //        notificationService.Notify(NotificationSeverity.Error, "Validation Error", validation.Message);
        //        return;
        //    }

        //    // Calculate totals
        //    CalculateItemTotal();

        //    // Add to grid
        //    PurchaseItems.Add(CurrentItem);
        //    CalculateTotals();

        //    // Reset current item
        //    CurrentItem = CreateNewItem();
        //    BarcodeSearchText = string.Empty;
        //    DisableItemFields = false;
        //    IsNewItemMode = false;

        //    ItemsGrid?.Reload();
        //    notificationService.Notify(NotificationSeverity.Success, "Success", "Item added to purchase");
        //}

        protected void EditItem(PurchaseItemDTO item)
        {
            CurrentItem = item;
            BarcodeSearchText = item.Barcode ?? string.Empty;
            PurchaseItems.Remove(item);
            CalculateTotals();
            ItemsGrid?.Reload();
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

            // Check for duplicate barcode in current items
            if (PurchaseItems.Any(i => i.Barcode == CurrentItem.Barcode))
                return (false, "Item with this barcode already added");

            return (true, string.Empty);
        }

        // =============================================
        // Calculation Methods
        // =============================================
        protected void CalculateItemTotal()
        {
            var itemTotal = _serviceUnitOfWork.PurchaseService.CalculateItemTotal(CurrentItem);
            CurrentItem.TotalAmount = itemTotal;
        }

        protected void CalculateTotals()
        {
            Purchase.TotalAmount = _serviceUnitOfWork.PurchaseService.CalculatePurchaseTotal(PurchaseItems);
            Purchase.NetAmount = Purchase.TotalAmount - (Purchase.DiscountAmount ?? 0) + (Purchase.VatAmount ?? 0);
            Purchase.DueAmount = Purchase.NetAmount - (Purchase.PaidAmount ?? 0);
            StateHasChanged();
        }

        protected void OnDiscountChanged()
        {
            CalculateTotals();
        }

        protected void OnVatChanged()
        {
            CalculateTotals();
        }

        protected void OnPaidAmountChanged()
        {
            CalculateTotals();
        }

        // =============================================
        // Save Methods
        // =============================================
        protected async Task SavePurchase()
        {
            if (!ValidatePurchase())
                return;

            try
            {
                IsProcessing = true;

                var result = await _serviceUnitOfWork.PurchaseService.SaveUpdatePurchase(Purchase, PurchaseItems);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Purchase saved successfully");
                    NavigationManager.NavigateTo("/PurchaseList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to save purchase");
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

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseList");
        }

        public void Dispose()
        {
            ItemsGrid?.Dispose();
        }
    }
}
