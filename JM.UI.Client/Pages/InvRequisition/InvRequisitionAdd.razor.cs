using JM.UI.Entities.Model.InvRequisition;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.InvRequisition
{
    public partial class InvRequisitionAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public ProtectedLocalStorage _localStorage { get; set; } = default!;
        [Inject] public DialogService dialogService { get; set; } = default!;
        [Parameter] public int? Id { get; set; }

        protected InvRequisitionMasterDTO Requisition { get; set; } = new();
        protected List<InvRequisitionDetailDTO> RequisitionDetails { get; set; } = new();
        protected InvRequisitionDetailDTO CurrentDetail { get; set; } = new();

        protected List<InvRequisitionPreviewRow> PreviewItems { get; set; } = new();
        protected RadzenDataGrid<InvRequisitionPreviewRow> PreviewGrid = new();

        protected decimal? SharedQty { get; set; }
        protected decimal? SharedUnitPrice { get; set; }
        protected string SharedRemarks { get; set; } = string.Empty;

        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<StoreDTO> ToStores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<ItemDTO> AvailableItems { get; set; } = new List<ItemDTO>();

        protected bool IsLoading { get; set; } = false;
        protected bool IsProcessing { get; set; } = false;
        protected bool IsSearchingBarcode { get; set; } = false;
        protected bool IsScanningBarcode { get; set; } = false;
        protected bool DisableItemFields { get; set; } = false;
        protected bool IsEditItemMode { get; set; } = false;
        protected bool IsToStoreReadOnly { get; set; } = false;
        protected int CurrentUserId { get; set; } = 0;
        protected int UserStoreId { get; set; } = 0;
        protected string BarcodeSearchText { get; set; } = string.Empty;
        protected string ScanBarcodeText { get; set; } = string.Empty;
        private bool _isFirstRender = true;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Requisition" : "New Requisition";

        protected RadzenDataGrid<InvRequisitionDetailDTO> ItemsGrid = default!;

        protected override async Task OnInitializedAsync()
        {
            NavigationGuard.IsGuardActive = true;
            await TokenService.InitializeTokenAsync();

            if (IsEditMode)
                await LoadRequisition();
            else
                InitializeRequisition();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _isFirstRender)
            {
                _isFirstRender = false;
                await LoadLookupData();
                StateHasChanged();
            }
        }

        private void InitializeRequisition()
        {
            Requisition = _serviceUnitOfWork.InvRequisitionService.CreateNew();
            RequisitionDetails = new List<InvRequisitionDetailDTO>();
            CurrentDetail = new InvRequisitionDetailDTO();
            PreviewItems = new List<InvRequisitionPreviewRow>();
        }

        private async Task LoadLookupData()
        {
            try
            {
                UserStoreId = await GetLocalStorageInt("StoreId");
                CurrentUserId = await GetLocalStorageInt("UserId");

                var stores = await _serviceUnitOfWork.StoreService.GetStores()
                             ?? new List<StoreDTO>();
                Stores = stores;

                if (CurrentUserId == 1)
                {
                    ToStores = stores;
                    IsToStoreReadOnly = false;
                }
                else
                {
                    ToStores = stores.Where(s => s.Id == UserStoreId).ToList();
                    IsToStoreReadOnly = true;
                    if (ToStores.Any())
                    {
                        Requisition.ToStore = UserStoreId;
                        Requisition.ToStoreName = ToStores.First().Name;
                    }
                }

                if (stores.Any())
                {
                    var fromStore = stores.FirstOrDefault(s => s.Id == UserStoreId) ?? stores.First();
                    Requisition.FromStore = fromStore.Id;
                    Requisition.FromStoreName = fromStore.Name;
                }

                await LoadItemsForStore(Requisition.FromStore);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        protected async Task LoadRequisition()
        {
            try
            {
                IsLoading = true;
                var master = await _serviceUnitOfWork.InvRequisitionService.GetById(Id!.Value);
                if (master is null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Not Found", $"Requisition #{Id} not found.");
                    NavigationManager.NavigateTo("/InvRequisitionList");
                    return;
                }
                Requisition = master;
                RequisitionDetails = master.Details?.ToList() ?? new List<InvRequisitionDetailDTO>();
                CurrentDetail = new InvRequisitionDetailDTO();
                PreviewItems = new List<InvRequisitionPreviewRow>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load requisition: {ex.Message}");
                NavigationManager.NavigateTo("/InvRequisitionList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadItemsForStore(int? storeId)
        {
            try
            {
                if (!storeId.HasValue || storeId.Value <= 0) return;
                var items = await _serviceUnitOfWork.ItemService.GetItemsByStoreId(storeId.Value);
                AvailableItems = items ?? new List<ItemDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InvReq] LoadItemsForStore error: {ex.Message}");
            }
        }

        protected async void OnFromStoreChanged(int? storeId)
        {
            if (!storeId.HasValue || storeId.Value <= 0) return;
            var store = Stores.FirstOrDefault(s => s.Id == storeId.Value);
            Requisition.FromStore = storeId.Value;
            Requisition.FromStoreName = store?.Name ?? string.Empty;
            PreviewItems.Clear();
            BarcodeSearchText = string.Empty;
            ScanBarcodeText = string.Empty;
            await LoadItemsForStore(storeId.Value);
            StateHasChanged();
        }

        protected async Task OnBarcodeDropdownChanged(object value)
        {
            var barcode = value?.ToString();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                IsSearchingBarcode = true;
                var items = AvailableItems.Where(i =>
                    i.Barcode != null && i.Barcode.Contains(barcode, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!items.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "No items found for this barcode.");
                    return;
                }

                PreviewItems = items.Select(i => new InvRequisitionPreviewRow
                {
                    ItemId = i.Id,
                    ItemName = i.Name ?? "Unknown",
                    Barcode = i.Barcode ?? barcode,
                    Qty = SharedQty ?? 1,
                    UnitPrice = i.SalePrice ?? 0,
                    Amount = (SharedQty ?? 1) * (i.SalePrice ?? 0),
                    Remarks = SharedRemarks
                }).ToList();

                PreviewGrid?.Reload();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Search Error", ex.Message);
            }
            finally
            {
                IsSearchingBarcode = false;
            }
        }

        protected void ClearBarcodeSearch()
        {
            BarcodeSearchText = string.Empty;
            PreviewItems.Clear();
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected async Task OnScanBarcodeKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(ScanBarcodeText))
                await ProcessScannedBarcode();
        }

        protected void OnScanBarcodeChange(string value)
        {
            ScanBarcodeText = value;
        }

        protected async Task ProcessScannedBarcode()
        {
            var barcode = ScanBarcodeText?.Trim();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            try
            {
                IsScanningBarcode = true;
                var item = AvailableItems.FirstOrDefault(i =>
                    i.Barcode != null && i.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase));

                if (item == null)
                {
                    try
                    {
                        item = await _serviceUnitOfWork.TransferService
                                   .SearchByBarcodeExact(barcode, Requisition.FromStore.Value);
                    }
                    catch (Exception innerEx) when (
                        innerEx.Message.Contains("does not contain any JSON") ||
                        innerEx.Message.Contains("isFinalBlock") ||
                        innerEx.Message.Contains("Unexpected error") ||
                        innerEx.Message.Contains("JSON"))
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Not Found", $"No item found with barcode '{barcode}'.");
                        return;
                    }
                }

                var existing = RequisitionDetails.FirstOrDefault(d =>
                    d.Barcode != null && d.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Qty += 1;
                    existing.Amount = existing.Qty * existing.UnitPrice;
                    await ItemsGrid.Reload();
                    notificationService.Notify(NotificationSeverity.Info, "Qty Updated", $"Increased qty for '{item.Name}'.");
                    ScanBarcodeText = string.Empty;
                    return;
                }

                RequisitionDetails.Add(new InvRequisitionDetailDTO
                {
                    ItemID = item.Id,
                    ItemName = item.Name,
                    Barcode = item.Barcode ?? barcode,
                    Qty = 1,
                    UnitPrice = item.SalePrice,
                    Amount = item.SalePrice,
                    CreateOn = DateTime.Now
                });

                await ItemsGrid.Reload();
                ScanBarcodeText = string.Empty;
                notificationService.Notify(NotificationSeverity.Success, "Added", $"'{item.Name}' added with qty 1.");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Scan Error", ex.Message);
            }
            finally
            {
                IsScanningBarcode = false;
                StateHasChanged();
            }
        }

        protected void OnSharedFieldChanged()
        {
            foreach (var row in PreviewItems)
            {
                if (SharedQty.HasValue)
                    row.Qty = SharedQty.Value;
                if (SharedUnitPrice.HasValue)
                    row.UnitPrice = SharedUnitPrice.Value;
                row.Remarks = SharedRemarks;
                row.Amount = row.Qty * row.UnitPrice;
            }
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void OnPreviewRowChanged(InvRequisitionPreviewRow row)
        {
            row.Amount = row.Qty * row.UnitPrice;
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void RemovePreviewRow(InvRequisitionPreviewRow row)
        {
            PreviewItems.Remove(row);
            PreviewGrid?.Reload();
            StateHasChanged();
        }

        protected void AddItemToGrid()
        {
            var toAdd = PreviewItems.Where(r => r.Qty > 0).ToList();
            if (!toAdd.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "No items with qty > 0 to add.");
                return;
            }

            foreach (var row in toAdd)
            {
                var existing = RequisitionDetails.FirstOrDefault(d =>
                    d.Barcode != null && d.Barcode.Equals(row.Barcode, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.Qty += row.Qty;
                    existing.Amount = existing.Qty * existing.UnitPrice;
                }
                else
                {
                    RequisitionDetails.Add(new InvRequisitionDetailDTO
                    {
                        ItemID = row.ItemId,
                        ItemName = row.ItemName,
                        Barcode = row.Barcode,
                        Qty = row.Qty,
                        UnitPrice = row.UnitPrice,
                        Amount = row.Amount,
                        Remarks = row.Remarks,
                        CreateOn = DateTime.Now
                    });
                }
            }

            PreviewItems.Clear();
            SharedQty = null;
            SharedUnitPrice = null;
            SharedRemarks = string.Empty;
            BarcodeSearchText = string.Empty;
            PreviewGrid?.Reload();
            ItemsGrid?.Reload();
            StateHasChanged();

            notificationService.Notify(NotificationSeverity.Success, "Added", $"{toAdd.Count} item(s) added to requisition.");
        }

        protected async Task EditItem(InvRequisitionDetailDTO item)
        {
            IsEditItemMode = true;
            CurrentDetail = new InvRequisitionDetailDTO
            {
                ItemID = item.ItemID,
                ItemName = item.ItemName,
                Barcode = item.Barcode,
                Qty = item.Qty,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount,
                Remarks = item.Remarks
            };

            var result = await dialogService.OpenAsync<InvRequisitionItemEditDialog>("Edit Item",
                new Dictionary<string, object> { { "Detail", CurrentDetail } },
                new DialogOptions { Width = "500px", Resizable = true, Draggable = true });

            if (result == true)
            {
                item.ItemID = CurrentDetail.ItemID;
                item.ItemName = CurrentDetail.ItemName;
                item.Barcode = CurrentDetail.Barcode;
                item.Qty = CurrentDetail.Qty;
                item.UnitPrice = CurrentDetail.UnitPrice;
                item.Amount = CurrentDetail.Qty * CurrentDetail.UnitPrice;
                item.Remarks = CurrentDetail.Remarks;
                ItemsGrid?.Reload();
                StateHasChanged();
            }
            IsEditItemMode = false;
        }

        protected void DeleteItem(InvRequisitionDetailDTO item)
        {
            RequisitionDetails.Remove(item);
            ItemsGrid?.Reload();
            StateHasChanged();
        }

        protected async Task HandleSave()
        {
            try
            {
                IsProcessing = true;
                Requisition.Details = RequisitionDetails;

                var result = await _serviceUnitOfWork.InvRequisitionService.SaveUpdate(Requisition);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Requisition saved successfully.");
                    NavigationManager.NavigateTo("/InvRequisitionList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Validation Error", result.Message ?? "Failed to save.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task TryResetLeftPanel()
        {
            var confirm = await dialogService.Confirm(
                "Reset all fields? Unsaved preview data will be lost.", "Reset?",
                new ConfirmOptions { OkButtonText = "Reset", CancelButtonText = "Cancel" });
            if (confirm == true)
            {
                PreviewItems.Clear();
                SharedQty = null;
                SharedUnitPrice = null;
                SharedRemarks = string.Empty;
                BarcodeSearchText = string.Empty;
                ScanBarcodeText = string.Empty;
                StateHasChanged();
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/InvRequisitionList");
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && int.TryParse(result.Value, out var val))
                    return val;
            }
            catch { }
            return 0;
        }
    }
}
