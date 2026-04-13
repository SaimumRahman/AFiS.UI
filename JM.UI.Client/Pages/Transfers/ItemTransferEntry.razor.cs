using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfers
{
    public partial class ItemTransferEntryComponent : PosComponentBase
    {

        // ── Injections ────────────────────────────────────────────────

        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        /// <summary>
        /// Dedicated transfer service — handles master/detail CRUD,
        /// validation, and factory helpers.
        /// </summary>
        [Inject] public ITransferService TransferService { get; set; } = default!;


        // ── Route Parameters ──────────────────────────────────────────

        [Parameter] public int? Id { get; set; }


        // ── Transfer Data ─────────────────────────────────────────────

        protected TransferMasterDTO Transfer { get; set; } = new();
        protected List<TransferDetailDTO> TransferDetails { get; set; } = new();

        /// <summary>Current detail row being built / edited in the entry panel.</summary>
        protected TransferDetailDTO CurrentDetail { get; set; } = new();

        /// <summary>Reference to the item currently being edited in the confirmed grid.</summary>
        protected TransferDetailDTO? _editingItem = null;


        // ── Preview Grid ──────────────────────────────────────────────

        protected List<TransferPreviewRow> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<TransferPreviewRow> PreviewGrid = new();


        // ── Shared fields (propagated to all preview rows) ────────────

        protected decimal SharedUnitPrice { get; set; }
        protected decimal? SharedIssueQty { get; set; }
        protected string SharedSerialNo { get; set; } = string.Empty;
        protected string SharedCreatedRemarks { get; set; } = string.Empty;


        // ── Lookup Data ───────────────────────────────────────────────

        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<StoreDTO> ToStores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<LookupItemDTO> TransferTypes { get; set; } = new List<LookupItemDTO>();
        protected IEnumerable<LookupItemDTO> DeliveryTypes { get; set; } = new List<LookupItemDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();
        protected IEnumerable<DesignModelDTO> Designs { get; set; } = new List<DesignModelDTO>();
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
        protected IEnumerable<MesurementUnitModelDTO> Units { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();


        // ── UI State ──────────────────────────────────────────────────

        protected bool IsLoading { get; set; } = false;
        protected bool IsProcessing { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected bool IsEditItemMode { get; set; } = false;
        protected string BarcodeSearchText { get; set; } = string.Empty;

        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Item Transfer" : "New Item Transfer";

        protected RadzenDataGrid<TransferDetailDTO> ItemsGrid = default!;


        // ── Lifecycle ─────────────────────────────────────────────────

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();

            if (IsEditMode)
                await LoadTransfer();
            else
                await InitializeTransfer();
        }


        // ── Initialization ────────────────────────────────────────────

        private async Task InitializeTransfer()
        {
            // Use the service factory method so defaults (CompanyId, CreatedBy,
            // auto-generated TransferNo, today's date, etc.) are applied consistently.
            Transfer = TransferService.CreateNewTransfer(
                companyId: 1,
                createdBy:1);

            TransferDetails = new List<TransferDetailDTO>();
            CurrentDetail = TransferService.CreateNewDetailLine();
            PreviewItems = new List<TransferPreviewRow>();

            // Auto-select default "Central" store if available
            if (Stores.Any())
            {
                var central = Stores.FirstOrDefault(s =>
                    s.Name.Contains("Central", StringComparison.OrdinalIgnoreCase));
                if (central != null) Transfer.StoreId = central.Id;
            }
        }

        private TransferDetailDTO CreateNewDetail() =>
            TransferService.CreateNewDetailLine();


        // ── Data Loading ──────────────────────────────────────────────

        private async Task LoadLookupData()
        {
            try
            {
                var stores = await _serviceUnitOfWork.StoreService.GetStores()
                             ?? new List<StoreDTO>();
                Stores = stores;
                ToStores = stores;

                Groups = await _serviceUnitOfWork.GroupService.GetGroups()
                          ?? new List<GroupModelDTO>();
                Colors = await _serviceUnitOfWork.ColorsService.GetColorss()
                          ?? new List<ColorsDTO>();
                Sizes = await _serviceUnitOfWork.SizesService.GetSizess()
                          ?? new List<SizesDTO>();
                Units = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits()
                          ?? new List<MesurementUnitModelDTO>();
                AvailableItems = await _serviceUnitOfWork.ItemService.GetItems()
                                 ?? new List<ItemDTO>();

                // Static lookup lists — replace with service calls if you have a DB table
                TransferTypes = new List<LookupItemDTO>
                {
                    new() { Id = 1, Name = "Internal Transfer" },
                    new() { Id = 2, Name = "Requisition" },
                    new() { Id = 3, Name = "Return" }
                };

                DeliveryTypes = new List<LookupItemDTO>
                {
                    new() { Id = 1, Name = "Own Vehicle" },
                    new() { Id = 2, Name = "Courier" },
                    new() { Id = 3, Name = "Hand Carry" }
                };
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load lookup data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load an existing transfer for editing.
        /// Uses <see cref="ITransferService.GetTransferById"/> so the component
        /// no longer depends on a stub.
        /// </summary>
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
                    NavigationManager.NavigateTo("/TransferList");
                    return;
                }

                Transfer = master;

                // Details are carried inside the master DTO — flatten to the local list.
                TransferDetails = master.Details?.ToList() ?? new List<TransferDetailDTO>();

                // Restore cascaded dropdowns for the first detail (if any)
                var firstDetail = TransferDetails.FirstOrDefault();
                if (firstDetail is not null)
                {
                    if (firstDetail.GroupId.HasValue)
                        SubGroups = await LoadSubGroupsByGroup(firstDetail.GroupId.Value);
                    if (firstDetail.SubGroupId.HasValue)
                        Designs = await LoadDesignsBySubGroup(firstDetail.SubGroupId.Value);
                }

                // Keep ToStores consistent with the loaded From Store
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


        // ── Store Changed — exclude the From Store from the To Store list ──

        protected void OnFromStoreChanged(int? storeId)
        {
            Transfer.StoreId = storeId;
            ToStores = Stores.Where(s => s.Id != storeId).ToList();

            if (Transfer.ToStoreId == storeId)
                Transfer.ToStoreId = null;

            StateHasChanged();
        }


        // ── Shared Fields Changed → Propagate to all preview rows ─────

        protected void OnSharedFieldChanged()
        {
            foreach (var row in PreviewItems)
            {
                row.UnitPrice = SharedUnitPrice;
                if (SharedIssueQty.HasValue)
                    row.IssueQty = SharedIssueQty.Value;
                row.SerialNo = SharedSerialNo;
                row.CreatedRemarks = SharedCreatedRemarks;
                RecalculatePreviewRow(row);
            }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnPreviewRowChanged(TransferPreviewRow row)
        {
            RecalculatePreviewRow(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        private static void RecalculatePreviewRow(TransferPreviewRow row) =>
            row.TotalAmount = row.IssueQty * row.UnitPrice;

        protected void RemovePreviewRow(TransferPreviewRow row)
        {
            PreviewItems.Remove(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }


        // ── Cascade Dropdown Events ───────────────────────────────────

        protected async Task OnGroupChanged(int? groupId)
        {
            if (!groupId.HasValue) return;
            CurrentDetail.GroupId = groupId;
            CurrentDetail.SubGroupId = null;
            CurrentDetail.DesignId = null;
            SubGroups = await LoadSubGroupsByGroup(groupId.Value);
            Designs = new List<DesignModelDTO>();
            StateHasChanged();
        }

        protected async Task OnSubGroupChanged(int? subGroupId)
        {
            if (!subGroupId.HasValue) return;
            CurrentDetail.SubGroupId = subGroupId;
            CurrentDetail.DesignId = null;
            Designs = await LoadDesignsBySubGroup(subGroupId.Value);
            await GenerateBarcode();
        }

        protected async Task OnColorChanged(int? colorId)
        {
            if (!colorId.HasValue) return;
            CurrentDetail.ColorId = colorId;
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentDetail.Barcode))
                await SearchSingleBarcodeAndAddToPreview(CurrentDetail.Barcode);

            StateHasChanged();
        }

        protected async Task OnSizeChanged(int? sizeId)
        {
            if (!sizeId.HasValue) return;
            CurrentDetail.SizeId = sizeId;
            await GenerateBarcode();

            if (!string.IsNullOrWhiteSpace(CurrentDetail.Barcode))
                await SearchSingleBarcodeAndAddToPreview(CurrentDetail.Barcode);
        }


        // ── Barcode Search ────────────────────────────────────────────

        protected async Task OnBarcodeDropdownChanged(object value)
        {
            var barcode = value?.ToString();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                IsSearchingBarcode = true;
                BarcodeSearchText = barcode;

                var result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                PreviewItems.Clear();

                if (result.Found && result.ItemDetails != null && result.ItemDetails.Any())
                {
                    DisableItemFields = true;

                    var first = result.ItemDetails.First();
                    CurrentDetail.ItemID = first.Id;
                    CurrentDetail.Barcode = barcode;
                    CurrentDetail.ColorId = first.ColorId;
                    CurrentDetail.SizeId = first.SizeId;
                    CurrentDetail.GroupId = first.GroupId;
                    CurrentDetail.SubGroupId = first.SubGroupId;
                    CurrentDetail.DesignId = first.DesignId;
                    CurrentDetail.UnitID = first.MesurementUnitId ?? 0;

                    if (first.GroupId.HasValue)
                        SubGroups = await LoadSubGroupsByGroup(first.GroupId.Value);
                    if (first.SubGroupId.HasValue)
                        Designs = await LoadDesignsBySubGroup(first.SubGroupId.Value);

                    foreach (var item in result.ItemDetails.Where(x => x != null))
                    {
                        var itemBarcode = !string.IsNullOrWhiteSpace(item!.Barcode)
                            ? item.Barcode
                            : barcode;

                        if (PreviewItems.Any(p => p.Barcode == itemBarcode)) continue;

                        PreviewItems.Add(new TransferPreviewRow
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
                            DesignId = item.DesignId,
                            UnitId = item.MesurementUnitId,
                            UnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                            StockQuantity = result.Stock?.Quantity ?? 0,
                            IsNewItem = false,
                            ProductType = item.ProductType ?? string.Empty,
                            CountStockByColor = item.CountStockByColor,
                            CountStockBySize = item.CountStockBySize,
                            IssueQty = 0,
                            UnitPrice = SharedUnitPrice > 0 ? SharedUnitPrice : (item.PurchasePrice ?? 0),
                            SerialNo = SharedSerialNo,
                            CreatedRemarks = SharedCreatedRemarks,
                            TotalAmount = 0
                        });
                    }

                    PreviewGrid?.Reload();
                    notificationService.Notify(NotificationSeverity.Success, "Loaded",
                        $"Item found! {PreviewItems.Count} variant(s) in preview.");
                }
                else
                {
                    CurrentDetail.Barcode = barcode;
                    DisableItemFields = false;

                    PreviewItems.Add(new TransferPreviewRow
                    {
                        ItemId = 0,
                        Barcode = barcode,
                        ItemName = string.Empty,
                        IsNewItem = true,
                        IssueQty = 0,
                        UnitPrice = SharedUnitPrice,
                        SerialNo = SharedSerialNo,
                        CreatedRemarks = SharedCreatedRemarks,
                        TotalAmount = 0
                    });

                    PreviewGrid?.Reload();
                    notificationService.Notify(NotificationSeverity.Info, "Not Found",
                        $"Barcode '{barcode}' not found. Please verify.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Search failed: {ex.Message}");
            }
            finally
            {
                IsSearchingBarcode = false;
                StateHasChanged();
            }
        }

        private async Task SearchSingleBarcodeAndAddToPreview(string barcode)
        {
            try
            {
                if (PreviewItems.Any(p => p.Barcode == barcode)) return;

                var response = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                TransferPreviewRow newRow;

                if (response.Found && response.ItemDetails != null && response.ItemDetails.Any())
                {
                    var item = response.ItemDetails.FirstOrDefault(x => x.Barcode == barcode)
                               ?? response.ItemDetails.First();

                    newRow = new TransferPreviewRow
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
                        DesignId = item.DesignId,
                        UnitId = item.MesurementUnitId,
                        UnitName = Units.FirstOrDefault(u => u.Id == item.MesurementUnitId)?.Name ?? string.Empty,
                        StockQuantity = response.Stock?.Quantity ?? 0,
                        IsNewItem = false,
                        ProductType = item.ProductType ?? string.Empty,
                        CountStockByColor = item.CountStockByColor,
                        CountStockBySize = item.CountStockBySize,
                        IssueQty = 0,
                        UnitPrice = SharedUnitPrice > 0 ? SharedUnitPrice : (item.PurchasePrice ?? 0),
                        SerialNo = SharedSerialNo,
                        CreatedRemarks = SharedCreatedRemarks,
                        TotalAmount = 0
                    };
                }
                else
                {
                    newRow = new TransferPreviewRow
                    {
                        ItemId = 0,
                        Barcode = barcode,
                        ItemName = string.Empty,
                        ColorId = CurrentDetail.ColorId,
                        ColorName = Colors.FirstOrDefault(c => c.Id == CurrentDetail.ColorId)?.Name ?? string.Empty,
                        SizeId = CurrentDetail.SizeId,
                        SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentDetail.SizeId)?.Name ?? string.Empty,
                        GroupId = CurrentDetail.GroupId,
                        SubGroupId = CurrentDetail.SubGroupId,
                        IsNewItem = true,
                        IssueQty = 0,
                        UnitPrice = SharedUnitPrice,
                        SerialNo = SharedSerialNo,
                        CreatedRemarks = SharedCreatedRemarks,
                        TotalAmount = 0
                    };
                }

                PreviewItems.Add(newRow);
                PreviewGrid?.Reload();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Preview load failed: {ex.Message}");
            }
        }


        // ── Barcode Generation ────────────────────────────────────────

        protected async Task GenerateBarcode()
        {
            if (!CurrentDetail.GroupId.HasValue && CurrentDetail.ItemID == 0) return;

            try
            {
                var request = new BarcodeGenerationRequestDTO
                {
                    ColorName = Colors.FirstOrDefault(c => c.Id == CurrentDetail.ColorId)?.ColorCode,
                    SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentDetail.SizeId)?.Name,
                    ItemId = CurrentDetail.ItemID,
                    GroupId = CurrentDetail.GroupId,
                    ExistingBarcode = CurrentDetail.Barcode
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentDetail.Barcode = barcode;
                BarcodeSearchText = barcode;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to generate barcode: {ex.Message}");
            }
        }


        // ── Add Items to Confirmed Grid ───────────────────────────────

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

            foreach (var row in validRows)
            {
                if (row.UnitPrice <= 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Validation",
                        $"Unit price must be greater than 0 for '{row.ItemName}' ({row.Barcode}).");
                    continue;
                }

                if (row.IssueQty > row.StockQuantity && row.StockQuantity > 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Stock Warning",
                        $"Issue qty ({row.IssueQty:N2}) exceeds available stock ({row.StockQuantity:N0}) " +
                        $"for '{row.ItemName}'. Skipping.");
                    continue;
                }

                if (TransferDetails.Any(i => i.Barcode == row.Barcode))
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Duplicate",
                        $"Barcode '{row.Barcode}' already added. Skipping.");
                    continue;
                }

                // Build the new detail line via the service factory so any
                // default properties (audit fields, etc.) are set consistently.
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
                newLine.UnitPrice = row.UnitPrice;
                newLine.TotalAmount = row.TotalAmount;
                newLine.SerialNo = row.SerialNo;
                newLine.CreatedRemarks = row.CreatedRemarks;
                newLine.IsNewItem = row.IsNewItem;
                newLine.CreatedAt = DateTime.Now;

                TransferDetails.Add(newLine);
                addedCount++;
            }

            if (addedCount == 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Nothing Added",
                    "No items were added. Please fix the validation errors and try again.");
                return;
            }

            if (addedCount < validRows.Count)
            {
                notificationService.Notify(NotificationSeverity.Info, "Partial Add",
                    $"{addedCount} of {validRows.Count} item(s) added. " +
                    $"{validRows.Count - addedCount} skipped due to errors.");
            }

            // Reset preview and barcode search
            PreviewItems.Clear();
            await PreviewGrid.Reload();
            await ItemsGrid.Reload();
            ResetSharedFields();
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;

            // Clear Color/Size/Barcode — preserve Group/Product for rapid entry
            CurrentDetail.ColorId = null;
            CurrentDetail.SizeId = null;
            CurrentDetail.Barcode = null;

            if (addedCount == validRows.Count)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"{addedCount} item(s) added to transfer.");
            }
        }


        // ── Edit Item in Confirmed Grid ───────────────────────────────

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
                UnitPrice = item.UnitPrice,
                SerialNo = item.SerialNo,
                CreatedRemarks = item.CreatedRemarks,
                TotalAmount = item.TotalAmount,
                StockQuantity = 0
            });

            PreviewGrid?.Reload();

            SharedUnitPrice = item.UnitPrice;
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
                UnitPrice = item.UnitPrice,
                TotalAmount = item.TotalAmount,
                SerialNo = item.SerialNo,
                CreatedRemarks = item.CreatedRemarks
            };

            BarcodeSearchText = item.Barcode ?? string.Empty;
            DisableItemFields = true;

            if (item.GroupId.HasValue)
                SubGroups = await LoadSubGroupsByGroup(item.GroupId.Value);
            if (item.SubGroupId.HasValue)
                Designs = await LoadDesignsBySubGroup(item.SubGroupId.Value);

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
            if (row.UnitPrice <= 0)
            {
                notificationService.Notify(NotificationSeverity.Error, "Validation",
                    "Unit price must be greater than 0.");
                return;
            }

            RecalculatePreviewRow(row);

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
                UnitPrice = row.UnitPrice,
                TotalAmount = row.TotalAmount,
                SerialNo = row.SerialNo,
                CreatedRemarks = row.CreatedRemarks,
                IsNewItem = row.IsNewItem,
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

        /// <summary>
        /// Removes a single detail line from the confirmed grid.
        /// If the line has already been persisted (TransferDetailID > 0) it is
        /// also soft-deleted on the server via <see cref="ITransferService.DeleteTransferDetail"/>.
        /// </summary>
        protected async Task DeleteItem(TransferDetailDTO item)
        {
            // If the record was already saved to the database, delete it server-side first.
            if (item.TransferDetailID > 0)
            {
                var result = await TransferService.DeleteTransferDetail(
                    item.TransferDetailID, 1);

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


        // ── Save Transfer ─────────────────────────────────────────────

        /// <summary>
        /// Validates and persists (create or update) the transfer via
        /// <see cref="ITransferService.SaveUpdateTransfer"/>.
        /// The service receives the master DTO with its Details collection
        /// populated — it decides internally whether to INSERT or UPDATE.
        /// </summary>
        protected async Task SaveTransfer()
        {
            // ── Client-side structural validation ──
            if (!ValidateTransfer()) return;

            // ── Service-side business validation ──
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
                    NavigationManager.NavigateTo("/TransferList");
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

        /// <summary>
        /// Basic UI-level guard checks before we even hit the service layer.
        /// Business rules are delegated to <see cref="ITransferService.ValidateTransfer"/>.
        /// </summary>
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


        // ── Cancel / Navigation ───────────────────────────────────────

        protected void Cancel()
        {
            if (IsEditItemMode)
            {
                CancelEditItem();
                notificationService.Notify(NotificationSeverity.Info, "Cancelled",
                    "Edit cancelled. No changes were saved.");
            }
            else
            {
                NavigationManager.NavigateTo("/TransferList");
            }
        }


        // ── Clear Barcode Search ──────────────────────────────────────

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


        // ── Reset Left Panel ──────────────────────────────────────────

        protected void ResetLeftPanel()
        {
            Transfer.StoreId = null;
            Transfer.ToStoreId = null;
            Transfer.TransTypeID = 0;
            Transfer.DeliveryTypeId = 0;
            Transfer.DeliveryAddress = null;
            Transfer.RequisitionID = null;
            Transfer.Comments = null;
            ToStores = Stores.ToList();

            CurrentDetail = TransferService.CreateNewDetailLine();
            SubGroups = new List<SubGroupModelDTO>();
            Designs = new List<DesignModelDTO>();
            BarcodeSearchText = string.Empty;
            DisableItemFields = false;
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            ResetSharedFields();
            StateHasChanged();
        }


        // ── Helpers ───────────────────────────────────────────────────

        private void ResetSharedFields()
        {
            SharedUnitPrice = 0;
            SharedIssueQty = null;
            SharedSerialNo = string.Empty;
            SharedCreatedRemarks = string.Empty;
        }

        private async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId) =>
            await _serviceUnitOfWork.SubGroupService.LoadSubGroupsByGroup(groupId)
            ?? new List<SubGroupModelDTO>();

        private async Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId) =>
            await _serviceUnitOfWork.DesignService.LoadDesignsBySubGroup(subGroupId)
            ?? new List<DesignModelDTO>();

        public void Dispose() => ItemsGrid?.Dispose();
    }
}