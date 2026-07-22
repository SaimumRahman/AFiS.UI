using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.InvRequisition;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfers
{
    public partial class ItemTransferEntryComponent : AddEditPageBase
    {
        [Inject]
        public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] 
        public ITransferService TransferService { get; set; } = default!;
        [Inject] 
        public DialogService dialogService { get; set; } = default!;
        [Inject] ProtectedLocalStorage _localStorage { get; set; }
        [Parameter] 
        public int? Id { get; set; }
        [Parameter]
        public int? RequisitionId { get; set; }
        protected bool IsFromRequisitionMode => RequisitionId.HasValue && RequisitionId.Value > 0;
        protected bool IsFromStoreReadOnly { get; set; } = false;
        protected bool IsDamagedMode { get; set; } = false;
        protected int CurrentUserId { get; set; } = 0;
        protected TransferMasterDTO Transfer { get; set; } = new();
        protected List<TransferDetailDTO> TransferDetails { get; set; } = new();
        protected TransferDetailDTO CurrentDetail { get; set; } = new();

        protected TransferDetailDTO? _editingItem = null;
        protected List<TransferPreviewRow> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<TransferPreviewRow> PreviewGrid = new();
        protected decimal? SharedIssueQty { get; set; }
        protected string SharedSerialNo { get; set; } = string.Empty;
        protected string SharedCreatedRemarks { get; set; } = string.Empty;
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<StoreDTO> ToStores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<LookupItemDTO> TransferTypes { get; set; } = new List<LookupItemDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();

        protected bool IsLoading { get; set; } = false;
        protected bool IsProcessing { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool IsScanningBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected bool IsEditItemMode { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;

        // â”€â”€ Scan textbox (direct-to-grid, qty = 1) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        protected string ScanBarcodeText { get; set; } = string.Empty;
        protected RadzenTextBox _barcodeInputRef = default!;
        private bool _shouldFocusBarcodeInput;
        private bool _isFirstRender = true;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Item Transfer" : IsFromRequisitionMode ? "Transfer from Requisition" : "New Item Transfer";

        protected RadzenDataGrid<TransferDetailDTO> ItemsGrid = default!;


        // â”€â”€ Lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected override async Task OnInitializedAsync()
        {
            NavigationGuard.IsGuardActive = true;
            await TokenService.InitializeTokenAsync();
            //await LoadLookupData();

            if (IsEditMode)
                await LoadTransfer();
            else
                await InitializeTransfer();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _isFirstRender)
            {
                _isFirstRender = false;
                await LoadLookupData();
                StateHasChanged();
            }

            if (_shouldFocusBarcodeInput)
            {
                _shouldFocusBarcodeInput = false;
                await _barcodeInputRef.Element.FocusAsync();
            }
        }
        // â”€â”€ Initialization â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private async Task InitializeTransfer()
        {
            Transfer = TransferService.CreateNewTransfer(companyId: 1, createdBy: 1);
            TransferDetails = new List<TransferDetailDTO>();
            CurrentDetail = TransferService.CreateNewDetailLine();
            PreviewItems = new List<TransferPreviewRow>();

            if (Stores.Any())
            {
                var central = Stores.FirstOrDefault(s =>
                    s.Name.Contains("Central", StringComparison.OrdinalIgnoreCase));
                if (central != null) Transfer.StoreId = central.Id;
            }
        }


        // â”€â”€ Data Loading â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task LoadLookupData()
        {
            try
            {
                // â”€â”€ Read from localStorage safely â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                int storeId = await GetLocalStorageInt("StoreId");
                CurrentUserId = await GetLocalStorageInt("UserId");

                Console.WriteLine($"[DEBUG] StoreId={storeId}, UserId={CurrentUserId}");

                // â”€â”€ Load Stores â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var stores = await _serviceUnitOfWork.StoreService.GetStores()
                             ?? new List<StoreDTO>();
                Stores = stores;
                ToStores = stores;

                // â”€â”€ Assign From Store based on User â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                if (CurrentUserId == 1)
                {
                    var headOfficeStore = stores.FirstOrDefault(s =>
                        s.Name.Contains("Head Office", StringComparison.OrdinalIgnoreCase));

                    Transfer.StoreId = headOfficeStore?.Id
                                          ?? stores.FirstOrDefault(s => s.Id == storeId)?.Id
                                          ?? 0;
                    IsFromStoreReadOnly = false;
                }
                else
                {
                    var matchedStore = stores.FirstOrDefault(s => s.Id == storeId);

                    if (matchedStore == null)
                    {
                        notificationService.Notify(
                            NotificationSeverity.Warning,
                            "Store Not Found",
                            $"Your assigned store (Id = {storeId}) was not found. Please contact admin.",
                            duration: 7000);
                        Transfer.StoreId = 0;
                    }
                    else
                    {
                        Transfer.StoreId = matchedStore.Id; // âœ… e.g. 1004
                    }

                    IsFromStoreReadOnly = true;
                }

                // â”€â”€ Exclude From Store from To Store list â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                ToStores = stores.Where(s => s.Id != Transfer.StoreId).ToList();

                // â”€â”€ Load Lookup Lists â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                Colors = await _serviceUnitOfWork.ColorsService.GetColorss()
                          ?? new List<ColorsDTO>();
                Sizes = await _serviceUnitOfWork.SizesService.GetSizess()
                          ?? new List<SizesDTO>();
                Units = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits()
                          ?? new List<MesurementUnitModelDTO>();

                // â”€â”€ Load Items based on store type â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                await LoadItemsForStore(Transfer.StoreId);

                // â”€â”€ Transfer Types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var transactionTypes = (await _serviceUnitOfWork.TransferTypeService.GetTransferTypes()).ToList();
                TransferTypes = (transactionTypes ?? new List<TransferTypeDTO>())
                    .Select(tt => new LookupItemDTO
                    {
                        Id = tt.TransferTypeId,
                        Name = tt.TransferTypeName
                    }).ToList();

                // If coming from requisition, load requisition data
                if (IsFromRequisitionMode)
                    await LoadFromRequisition();
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Failed to load lookup data: {ex.Message}");
            }
        }
        private async Task LoadFromRequisition()
        {
            try
            {
                IsLoading = true;

                var requisition = await _serviceUnitOfWork.InvRequisitionService.GetById(RequisitionId!.Value);
                if (requisition == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Not Found",
                        $"Requisition #{RequisitionId} could not be found.");
                    NavigationManager.NavigateTo("/InvRequisitionList");
                    return;
                }

                Transfer.RequisitionID = requisition.RequisitionID;
                Transfer.StoreId = requisition.FromStore;
                Transfer.ToStoreId = requisition.ToStore;
                Transfer.Comments = requisition.Remarks;
                Transfer.TransferDate = requisition.RequisitionDate;

                PreviewItems = new List<TransferPreviewRow>();

                var allItems = (await _serviceUnitOfWork.ItemService.GetItems())?.ToList() ?? new List<ItemDTO>();
                var itemLookup = new Dictionary<int, ItemDTO>();
                foreach (var i in allItems)
                {
                    if (!itemLookup.ContainsKey(i.Id))
                        itemLookup[i.Id] = i;
                }

                foreach (var detail in requisition.Details.Where(d => !d.IsDeleted))
                {
                    var item = itemLookup.GetValueOrDefault(detail.ItemID);
                    if (item == null) continue;

                    PreviewItems.Add(new TransferPreviewRow
                    {
                        ItemId = item.Id,
                        Barcode = item.Barcode ?? detail.Barcode ?? string.Empty,
                        ItemName = item.Name ?? detail.ItemName ?? string.Empty,
                        ColorId = item.ColorId,
                        ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty,
                        SizeId = item.SizeId,
                        SizeName = Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.Name ?? string.Empty,
                        GroupId = item.GroupId,
                        SubGroupId = item.SubGroupId,
                        DesignId = item.DesignId,
                        UnitId = item.MesurementUnitId,
                        UnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                        IssueQty = detail.Qty,
                        SalePrice = item.SalePrice ?? 0,
                        StockQuantity = item.CurrentStock,
                        IsNewItem = false,
                    });
                }

                // Auto-select Internal Transfer type and disable switching
                var internalTransfer = TransferTypes.FirstOrDefault(t =>
                    t.Name.Equals("Internal Transfer", StringComparison.OrdinalIgnoreCase));
                if (internalTransfer != null)
                {
                    Transfer.TransTypeID = internalTransfer.Id;
                    OnTransferTypeChanged(internalTransfer.Id);
                }

                PreviewGrid?.Reload();
                StateHasChanged();

                notificationService.Notify(NotificationSeverity.Info, "Requisition Loaded",
                    $"Requisition '{requisition.RequisitionNo}' loaded with {PreviewItems.Count} item(s).");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load requisition: {ex.Message}");
                NavigationManager.NavigateTo("/InvRequisitionList");
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected async Task LoadTransfer()
        {
            try
            {
                IsLoading = true;

                var master = await TransferService.GetTransferById(Id!.Value);

                if (master is null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Not Found",
                        $"Transfer #{Id} could not be found.");
                    NavigationManager.NavigateTo("/ItemsTransferList");
                    return;
                }

                Transfer = master;
                TransferDetails = master.Details?.ToList() ?? new List<TransferDetailDTO>();

                if (master.StoreId.HasValue)
                    ToStores = Stores.Where(s => s.Id != master.StoreId.Value).ToList();

                CurrentDetail = TransferService.CreateNewDetailLine();
                PreviewItems = new List<TransferPreviewRow>();

                notificationService.Notify(NotificationSeverity.Info, "Loaded",
                    $"Transfer {master.TransferNo} loaded for editing.");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load transfer: {ex.Message}");
                NavigationManager.NavigateTo("/TransferList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnFromStoreChanged(int? storeId)
        {
            // Branch users cannot change From Store
            if (IsFromStoreReadOnly) return;

            Transfer.StoreId = storeId;
            ToStores = Stores.Where(s => s.Id != storeId).ToList();

            if (Transfer.ToStoreId == storeId)
                Transfer.ToStoreId = null;

            _ = LoadItemsForStore(storeId);

            StateHasChanged();
        }

        protected void OnTransferTypeChanged(int? transTypeId)
        {
            if (transTypeId is null) return;

            var selectedType = TransferTypes.FirstOrDefault(t => t.Id == transTypeId.Value);
            if (selectedType is null) return;

            var isDamaged = selectedType.Name.Equals("Damage", StringComparison.OrdinalIgnoreCase);
            IsDamagedMode = isDamaged;

            if (isDamaged)
            {
                var headOffice = Stores.FirstOrDefault(s =>
                    s.Name.Contains("Head Office", StringComparison.OrdinalIgnoreCase));
                var damagedStore = Stores.FirstOrDefault(s =>
                    s.Code?.Equals("dm", StringComparison.OrdinalIgnoreCase) == true);

                if (headOffice != null)
                    Transfer.StoreId = headOffice.Id;

                if (damagedStore != null)
                    Transfer.ToStoreId = damagedStore.Id;

                ToStores = Stores.Where(s => s.Id != Transfer.StoreId).ToList();
                _ = LoadItemsForStore(Transfer.StoreId);
            }

            StateHasChanged();
        }


        // �"?�"? Shared Fields Changed �+' Propagate to all preview rows �"?�"?�"?�"?�"?

        protected void OnSharedFieldChanged()
        {
            foreach (var row in PreviewItems)
            {
                if (SharedIssueQty.HasValue)
                    row.IssueQty = SharedIssueQty.Value;
                row.SerialNo = SharedSerialNo;
                row.CreatedRemarks = SharedCreatedRemarks;
            }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnPreviewRowChanged(TransferPreviewRow row)
        {
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void RemovePreviewRow(TransferPreviewRow row)
        {
            PreviewItems.Remove(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }


        // â”€â”€ Barcode Dropdown Search â†’ loads preview grid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected async Task OnBarcodeDropdownChanged(object value)
        {
            var barcode = value?.ToString();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            if (!Transfer.StoreId.HasValue || Transfer.StoreId.Value == 0)
            {
                notificationService.Notify(
                    NotificationSeverity.Warning,
                    "Store Not Selected",
                    "Please select a 'From Store' before searching for items.",
                    duration: 5000);

                BarcodeSearchText = string.Empty;
                StateHasChanged();
                return;
            }

            try
            {
                IsSearchingBarcode = true;
                BarcodeSearchText = barcode;

                var storeName = Stores.FirstOrDefault(s => s.Id == Transfer.StoreId)?.Name
                                ?? $"Store #{Transfer.StoreId}";

                List<ItemDTO>? items = null;

                try
                {
                    items = (await _serviceUnitOfWork.TransferService
                                .SearchByBarcodeUptoColor(barcode, Transfer.StoreId.Value)).ToList();
                }
                catch (Exception innerEx) when (
                    innerEx.Message.Contains("does not contain any JSON") ||
                    innerEx.Message.Contains("isFinalBlock") ||
                    innerEx.Message.Contains("Unexpected error") ||
                    innerEx.Message.Contains("JSON"))   
                {
                    items = null;
                }

                //PreviewItems.Clear();
                //PreviewGrid?.Reload();

                if (items != null && items.Any())
                {
                    DisableItemFields = true;

                    foreach (var item in items)
                    {
                        var itemBarcode = item.Barcode ?? barcode;

                        // Skip if this exact barcode already exists in preview
                        if (PreviewItems.Any(r => r.Barcode == itemBarcode))
                        {
                            notificationService.Notify(
                                NotificationSeverity.Info,
                                "Already in Preview",
                                $"'{item.Name}' ({itemBarcode}) is already in the preview list.");
                            continue;
                        }

                        PreviewItems.Add(new TransferPreviewRow
                        {
                            ItemId = item.Id,
                            Barcode = itemBarcode,
                            ItemName = item.Name ?? string.Empty,
                            ColorId = item.ColorId,
                            ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty,
                            SizeId = item.SizeId,
                            SizeName = Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.Name ?? string.Empty,
                            GroupId = item.GroupId,
                            SubGroupId = item.SubGroupId,
                            DesignId = item.DesignId,
                            UnitId = item.MesurementUnitId,
                            UnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                            IsNewItem = false,
                            IssueQty = 0,
                            SerialNo = SharedSerialNo,
                            CreatedRemarks = SharedCreatedRemarks,
                            StockQuantity = item.CurrentStock,
                            SalePrice = item.SalePrice.Value
                        });
                    }
                    PreviewGrid?.Reload();

                    notificationService.Notify(
                        NotificationSeverity.Success,
                        "Items Found",
                        $"{items.Count} item(s) loaded from '{storeName}'.");
                }
                else
                {
                    CurrentDetail.Barcode = barcode;
                    DisableItemFields = false;

                    notificationService.Notify(
                        NotificationSeverity.Warning,
                        "Item Not Found",
                        $"Barcode '{barcode}' was not found in '{storeName}'. " +
                        $"Please verify the barcode or select the correct From Store.",
                        duration: 6000);
                }
            }
            catch (Exception ex)
            {
                PreviewItems.Clear();
                PreviewGrid?.Reload();
                DisableItemFields = false;

                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Search Failed",
                    $"An unexpected error occurred while searching: {ex.Message}",
                    duration: 7000);
            }
            finally
            {
                IsSearchingBarcode = false;
                StateHasChanged();
            }
        }


        // â”€â”€ Scan Barcode Textbox â†’ direct add to final list (qty = 1) â”€

        protected async Task OnScanBarcodeKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key != "Enter") return;
            await ProcessScannedBarcode();
        }

        protected async Task OnScanBarcodeChange(string value)
        {
            ScanBarcodeText = value;
        }

        protected async Task ProcessScannedBarcode()
        {
            var barcode = ScanBarcodeText?.Trim();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            if (!Transfer.StoreId.HasValue || Transfer.StoreId.Value == 0)
            {
                notificationService.Notify(
                    NotificationSeverity.Warning,
                    "Store Not Selected",
                    "Please select a 'From Store' before scanning items.",
                    duration: 5000);
                ScanBarcodeText = string.Empty;
                _shouldFocusBarcodeInput = true;
                StateHasChanged();
                return;
            }

            try
            {
                IsScanningBarcode = true;
                StateHasChanged(); // no refocus needed — scanning just started

                var storeName = Stores.FirstOrDefault(s => s.Id == Transfer.StoreId)?.Name
                                ?? $"Store #{Transfer.StoreId}";

                ItemDTO? item = null;

                try
                {
                    item = await _serviceUnitOfWork.TransferService
                               .SearchByBarcodeExact(barcode, Transfer.StoreId.Value);
                }
                catch (Exception innerEx) when (
                    innerEx.Message.Contains("does not contain any JSON") ||
                    innerEx.Message.Contains("isFinalBlock") ||
                    innerEx.Message.Contains("Unexpected error") ||
                    innerEx.Message.Contains("JSON"))
                {
                    item = null;
                }

                if (item == null)
                {
                    notificationService.Notify(
                        NotificationSeverity.Warning,
                        "Item Not Found",
                        $"Barcode '{barcode}' was not found in '{storeName}'.",
                        duration: 5000);
                    ScanBarcodeText = string.Empty;
                    _shouldFocusBarcodeInput = true;
                    StateHasChanged();
                    return;
                }

                // Check duplicate in confirmed list
                var existingLine = TransferDetails.FirstOrDefault(d => d.Barcode == item.Barcode);

                if (existingLine != null)
                {
                    // Ask user
                    bool confirmed = await dialogService.Confirm(
                        $"'{item.Name}' ({item.Barcode}) is already in the transfer list " +
                        $"with qty {existingLine.IssueQty:N2}. " +
                        $"Do you want to add 1 more?",
                        "Item Already Added",
                        new ConfirmOptions
                        {
                            OkButtonText = "Yes, Add More",
                            CancelButtonText = "No"
                        }) ?? false;

                    if (!confirmed)
                    {
                        ScanBarcodeText = string.Empty;
                        _shouldFocusBarcodeInput = true;
                        StateHasChanged();
                        return;
                    }

                    var totalQty = existingLine.IssueQty + 1;

                    // Stock check against combined qty
                    if (item.CurrentStock > 0 && totalQty > item.CurrentStock)
                    {
                        notificationService.Notify(
                            NotificationSeverity.Error,
                            "Insufficient Stock",
                            $"'{item.Name}' ({item.Barcode}): Combined qty ({totalQty:N2}) " +
                            $"exceeds available stock ({item.CurrentStock:N0}). Cannot add.");
                        ScanBarcodeText = string.Empty;
                        _shouldFocusBarcodeInput = true;
                        StateHasChanged();
                        return;
                    }

                    // Update qty in confirmed list
                    existingLine.IssueQty = totalQty;
                    existingLine.UpdatedAt = DateTime.Now;
                    await ItemsGrid.Reload();

                    notificationService.Notify(
                        NotificationSeverity.Success,
                        "Quantity Updated",
                        $"'{item.Name}' qty updated to {totalQty:N2}.");

                    ScanBarcodeText = string.Empty;
                    IsScanningBarcode = false;
                    _shouldFocusBarcodeInput = true;
                    StateHasChanged();
                    return;
                }
                // Stock check
                if (item.CurrentStock > 0 && 1 > item.CurrentStock)
                {
                    notificationService.Notify(
                        NotificationSeverity.Error,
                        "Insufficient Stock",
                        $"'{item.Name}': no stock available in '{storeName}'.");
                    ScanBarcodeText = string.Empty;
                    _shouldFocusBarcodeInput = true;
                    StateHasChanged();
                    return;
                }

                var newLine = TransferService.CreateNewDetailLine();
                newLine.TransferID = Transfer.TransferId;
                newLine.ItemID = item.Id;
                newLine.Barcode = item.Barcode ?? barcode;
                newLine.ItemName = item.Name ?? string.Empty;
                newLine.ColorId = item.ColorId;
                newLine.ColorName = Colors.FirstOrDefault(c => c.Id == item.ColorId)?.Name ?? string.Empty;
                newLine.SizeId = item.SizeId;
                newLine.SizeName = Sizes.FirstOrDefault(s => s.Id == item.SizeId)?.Name ?? string.Empty;
                newLine.GroupId = item.GroupId;
                newLine.SubGroupId = item.SubGroupId;
                newLine.DesignId = item.DesignId;
                newLine.UnitID = item.MesurementUnitId ?? 0;
                newLine.UnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name;
                newLine.IssueQty = 1;
                newLine.SerialNo = string.Empty;
                newLine.CreatedRemarks = string.Empty;
                newLine.IsNewItem = false;
                newLine.CreatedAt = DateTime.Now;
                newLine.SalePrice = item.SalePrice ?? 0;

                TransferDetails.Add(newLine);
                await ItemsGrid.Reload();

                notificationService.Notify(
                    NotificationSeverity.Success,
                    "Item Added",
                    $"'{item.Name}' scanned and added with qty = 1.");
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Scan Failed",
                    $"An unexpected error occurred: {ex.Message}",
                    duration: 7000);
            }
            finally
            {
                ScanBarcodeText = string.Empty;
                IsScanningBarcode = false;
                _shouldFocusBarcodeInput = true;
                StateHasChanged();
            }
        }


        // â”€â”€ Add Items to Confirmed Grid (from preview) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected async Task AddItemToGrid()
        {
            if (IsEditItemMode)
            {
                await UpdateEditedItem();
                return;
            }

            var validRows = PreviewItems.Where(r => r.IssueQty > 0).ToList();
            if (!validRows.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "No Quantity",
                    "Please enter issue quantity for at least one item.");
                return;
            }

            int addedCount = 0;
            int updatedCount = 0;

            foreach (var row in validRows)
            {
                // â”€â”€ Stock check (preview qty vs available stock) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                if (row.StockQuantity > 0 && row.IssueQty > row.StockQuantity)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Insufficient Stock",
                        $"'{row.ItemName}' ({row.Barcode}): Issue qty ({row.IssueQty:N2}) " +
                        $"exceeds available stock ({row.StockQuantity:N0}). Skipping.");
                    continue;
                }

                // â”€â”€ Duplicate check â€” already in confirmed list â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var existingLine = TransferDetails.FirstOrDefault(i => i.Barcode == row.Barcode);

                if (existingLine != null)
                {
                    // Ask user
                    bool confirmed = await dialogService.Confirm(
                        $"'{row.ItemName}' ({row.Barcode}) is already in the transfer list " +
                        $"with qty {existingLine.IssueQty:N2}. " +
                        $"Do you want to add {row.IssueQty:N2} more?",
                        "Item Already Added",
                        new ConfirmOptions
                        {
                            OkButtonText = "Yes, Add More",
                            CancelButtonText = "No"
                        }) ?? false;

                    if (!confirmed) continue;   // user said No â€” skip silently

                    var totalQty = existingLine.IssueQty + row.IssueQty;

                    // Stock check against combined qty
                    if (row.StockQuantity > 0 && totalQty > row.StockQuantity)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Insufficient Stock",
                            $"'{row.ItemName}' ({row.Barcode}): Combined qty ({totalQty:N2}) " +
                            $"exceeds available stock ({row.StockQuantity:N0}). Cannot add.");
                        continue;
                    }

                    // Update qty in confirmed list
                    existingLine.IssueQty = totalQty;
                    existingLine.UpdatedAt = DateTime.Now;
                    updatedCount++;
                    continue;
                }

                // â”€â”€ Fresh add â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                var newLine = TransferService.CreateNewDetailLine();

                newLine.TransferID = Transfer.TransferId;
                newLine.ItemID = row.ItemId;
                newLine.Barcode = row.Barcode;
                newLine.ItemName = row.ItemName;
                newLine.ColorId = row.ColorId;
                newLine.ColorName = row.ColorName;
                newLine.SizeId = row.SizeId;
                newLine.SizeName = row.SizeName;
                newLine.GroupId = row.GroupId;
                newLine.SubGroupId = row.SubGroupId;
                newLine.DesignId = row.DesignId;
                newLine.UnitID = row.UnitId ?? 0;
                newLine.UnitName = !string.IsNullOrWhiteSpace(row.UnitName)
                                              ? row.UnitName
                                              : Units.FirstOrDefault(u => u.Id == row.UnitId)?.Name;
                newLine.IssueQty = row.IssueQty;
                newLine.SerialNo = row.SerialNo;
                newLine.CreatedRemarks = row.CreatedRemarks;
                newLine.IsNewItem = row.IsNewItem;
                newLine.CreatedAt = DateTime.Now;
                newLine.SalePrice = row.SalePrice;

                TransferDetails.Add(newLine);
                addedCount++;
            }

            // â”€â”€ Reload confirmed grid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            await ItemsGrid.Reload();

            // â”€â”€ Clear preview â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            PreviewItems.Clear();
            await PreviewGrid.Reload();
            ResetSharedFields();
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            CurrentDetail.ColorId = null;
            CurrentDetail.SizeId = null;
            CurrentDetail.Barcode = null;

            // â”€â”€ Notification â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (addedCount == 0 && updatedCount == 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Nothing Added",
                    "No items were added. Please fix the validation errors and try again.");
                return;
            }

            var parts = new List<string>();
            if (addedCount > 0) parts.Add($"{addedCount} new item(s) added");
            if (updatedCount > 0) parts.Add($"{updatedCount} existing item(s) updated");

            notificationService.Notify(NotificationSeverity.Success, "Done",
                string.Join(", ", parts) + ".");
        }

        // â”€â”€ Edit Item in Confirmed Grid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected async Task EditItem(TransferDetailDTO item)
        {
            _editingItem = item;
            IsEditItemMode = true;

            PreviewItems.Clear();
            ResetSharedFields();

            PreviewItems.Add(new TransferPreviewRow
            {
                ItemId = item.ItemID,
                ItemName = item.ItemName ?? string.Empty,
                Barcode = item.Barcode ?? string.Empty,
                ColorId = item.ColorId,
                ColorName = item.ColorName ?? string.Empty,
                SizeId = item.SizeId,
                SizeName = item.SizeName ?? string.Empty,
                GroupId = item.GroupId,
                SubGroupId = item.SubGroupId,
                DesignId = item.DesignId,
                UnitId = item.UnitID,
                UnitName = item.UnitName ?? string.Empty,
                IsNewItem = item.IsNewItem,
                IssueQty = item.IssueQty,
                SerialNo = item.SerialNo,
                CreatedRemarks = item.CreatedRemarks,
                StockQuantity = 0,
                SalePrice = item.SalePrice
            });

            PreviewGrid?.Reload();

            SharedIssueQty = item.IssueQty;
            SharedSerialNo = item.SerialNo ?? string.Empty;
            SharedCreatedRemarks = item.CreatedRemarks ?? string.Empty;

            CurrentDetail = new TransferDetailDTO
            {
                TransferDetailID = item.TransferDetailID,
                TransferID = item.TransferID,
                ItemID = item.ItemID,
                Barcode = item.Barcode,
                ItemName = item.ItemName,
                GroupId = item.GroupId,
                SubGroupId = item.SubGroupId,
                DesignId = item.DesignId,
                ColorId = item.ColorId,
                ColorName = item.ColorName,
                SizeId = item.SizeId,
                SizeName = item.SizeName,
                UnitID = item.UnitID,
                IssueQty = item.IssueQty,
                SerialNo = item.SerialNo,
                CreatedRemarks = item.CreatedRemarks,
                SalePrice = item.SalePrice
            };

            BarcodeSearchText = item.Barcode ?? string.Empty;
            DisableItemFields = true;

            notificationService.Notify(NotificationSeverity.Info, "Edit Mode",
                $"Editing '{item.ItemName}' â€” make changes then click Update.");

            StateHasChanged();
        }

        protected async Task UpdateEditedItem()
        {
            if (_editingItem == null) return;

            var row = PreviewItems.FirstOrDefault();
            if (row == null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Nothing to update",
                    "The preview grid is empty.");
                return;
            }

            if (row.IssueQty <= 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Issue quantity must be greater than 0.");
                return;
            }

            var idx = TransferDetails.IndexOf(_editingItem);
            var updated = new TransferDetailDTO
            {
                TransferDetailID = _editingItem.TransferDetailID,
                TransferID = _editingItem.TransferID,
                ItemID = row.ItemId,
                Barcode = row.Barcode,
                ItemName = row.ItemName,
                ColorId = row.ColorId,
                ColorName = row.ColorName,
                SizeId = row.SizeId,
                SizeName = row.SizeName,
                GroupId = row.GroupId,
                SubGroupId = row.SubGroupId,
                DesignId = row.DesignId,
                UnitID = row.UnitId ?? _editingItem.UnitID,
                UnitName = !string.IsNullOrWhiteSpace(row.UnitName)
                                       ? row.UnitName
                                       : Units.FirstOrDefault(u => u.Id == row.UnitId)?.Name,
                IssueQty = row.IssueQty,
                SerialNo = row.SerialNo,
                CreatedRemarks = row.CreatedRemarks,
                IsNewItem = row.IsNewItem,
                SalePrice = row.SalePrice,
                UpdatedAt = DateTime.Now
            };

            if (idx >= 0)
                TransferDetails[idx] = updated;
            else
                TransferDetails.Add(updated);

            await ItemsGrid.Reload();
            CancelEditItem();

            notificationService.Notify(NotificationSeverity.Success, "Updated",
                $"'{row.ItemName}' updated successfully.");
        }

        protected void CancelEditItem()
        {
            _editingItem = null;
            IsEditItemMode = false;
            DisableItemFields = false;
            BarcodeSearchText = string.Empty;
            CurrentDetail = TransferService.CreateNewDetailLine();
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedFields();
            StateHasChanged();
        }

        protected async Task DeleteItem(TransferDetailDTO item)
        {
            if (item.TransferDetailID > 0)
            {
                var result = await TransferService.DeleteTransferDetail(item.TransferDetailID, 1);

                if (!result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Delete Failed",
                        result.Message ?? "Could not delete the transfer detail.");
                    return;
                }
            }

            TransferDetails.Remove(item);
            ItemsGrid?.Reload();
            notificationService.Notify(NotificationSeverity.Success, "Removed",
                "Item removed from transfer.");
        }


        // â”€â”€ Save Transfer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected async Task SaveTransfer()
        {
            if (!ValidateTransfer()) return;

            Transfer.Details = TransferDetails;

            var (isValid, errorMessage) = await TransferService.ValidateTransfer(Transfer);
            if (!isValid)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation", errorMessage);
                return;
            }

            try
            {
                IsProcessing = true;

                var result = await TransferService.SaveUpdateTransfer(Transfer);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        result.Message ?? "Transfer saved successfully.");
                    NavigationManager.NavigateTo("/ItemsTransferList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Save Failed",
                        result.Message ?? "An error occurred while saving the transfer.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to save transfer: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool ValidateTransfer()
        {
            if (!Transfer.StoreId.HasValue || Transfer.StoreId.Value == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Please select the From Store.");
                return false;
            }
            if (!Transfer.ToStoreId.HasValue || Transfer.ToStoreId.Value == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Please select the To Store.");
                return false;
            }
            if (Transfer.StoreId == Transfer.ToStoreId)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "From Store and To Store cannot be the same.");
                return false;
            }
            if (Transfer.TransferDate == default)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Please select a transfer date.");
                return false;
            }
            if (TransferDetails.Count == 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Please add at least one item to transfer.");
                return false;
            }
            return true;
        }


        // â”€â”€ Cancel / Navigation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected void Cancel()
        {
            if (IsEditItemMode)
            {
                CancelEditItem();
                notificationService.Notify(NotificationSeverity.Info, "Cancelled",
                    "Edit cancelled. No changes were saved.");
            }
            else if (IsFromRequisitionMode)
            {
                NavigationManager.NavigateTo("/InvRequisitionList");
            }
            else
            {
                NavigationManager.NavigateTo("/TransferList");
            }
        }


        // â”€â”€ Clear / Reset â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        protected void ClearBarcodeSearch()
        {
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            CurrentDetail = TransferService.CreateNewDetailLine();
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedFields();
            StateHasChanged();
        }

        protected void ResetLeftPanel()
        {
            if (IsFromRequisitionMode)
            {
                NavigationManager.NavigateTo("/InvRequisitionList");
                return;
            }
            Transfer.StoreId = null;
            Transfer.ToStoreId = null;
            Transfer.TransTypeID = 0;
            Transfer.DeliveryTypeId = 0;
            Transfer.DeliveryAddress = null;
            Transfer.RequisitionID = null;
            Transfer.Comments = null;
            ToStores = Stores.ToList();

            CurrentDetail = TransferService.CreateNewDetailLine();
            BarcodeSearchText = string.Empty;
            ScanBarcodeText = string.Empty;
            DisableItemFields = false;
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedFields();
            StateHasChanged();
        }


        // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private bool HasUnsavedData()
        {
            return PreviewItems.Any()
                || TransferDetails.Any()
                || Transfer.ToStoreId.HasValue
                || Transfer.TransTypeID != 0
                || Transfer.DeliveryTypeId != 0
                || !string.IsNullOrWhiteSpace(Transfer.DeliveryAddress)
                || !string.IsNullOrWhiteSpace(Transfer.Comments)
                || Transfer.RequisitionID.HasValue
                || !string.IsNullOrWhiteSpace(SharedSerialNo)
                || !string.IsNullOrWhiteSpace(SharedCreatedRemarks)
                || SharedIssueQty.HasValue
                || !string.IsNullOrWhiteSpace(BarcodeSearchText)
                || !string.IsNullOrWhiteSpace(ScanBarcodeText);
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
        private void ResetSharedFields()
        {
            SharedIssueQty = null;
            SharedSerialNo = string.Empty;
            SharedCreatedRemarks = string.Empty;
        }
        // â”€â”€ Helper: check if a store is Head Office â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // â”€â”€ Helper: check if a store is Head Office â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private bool IsHeadOfficeStore(int? storeId)
        {
            if (!storeId.HasValue) return false;
            var store = Stores.FirstOrDefault(s => s.Id == storeId.Value);
            return store != null &&
                   store.Name.Contains("Head Office", StringComparison.OrdinalIgnoreCase);
        }

        // â”€â”€ Load items â€” all for Head Office, filtered for branch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task LoadItemsForStore(int? storeId)
        {
            if (!storeId.HasValue || storeId.Value == 0)
            {
                AvailableItems = new List<ItemDTO>();
                StateHasChanged();
                return;
            }

            try
            {
                if (IsHeadOfficeStore(storeId))
                {
                    // Head Office â†’ load ALL items
                    AvailableItems = await _serviceUnitOfWork.ItemService.GetItems()
                                     ?? new List<ItemDTO>();
                }
                else
                {
                    // Branch store â†’ load only their stock
                    AvailableItems = await _serviceUnitOfWork.ItemService.GetItemsByStoreId(storeId.Value)
                                     ?? new List<ItemDTO>();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Failed to load items: {ex.Message}");
                AvailableItems = new List<ItemDTO>();
            }

            StateHasChanged();
        }
        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && !string.IsNullOrEmpty(result.Value))
                {
                    if (int.TryParse(result.Value, out int parsed) && parsed > 0)
                        return parsed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
            }

            return 0;
        }
        public void Dispose()
        {
            ItemsGrid?.Dispose();
            NavigationGuard.IsGuardActive = false;
        }
    }
}