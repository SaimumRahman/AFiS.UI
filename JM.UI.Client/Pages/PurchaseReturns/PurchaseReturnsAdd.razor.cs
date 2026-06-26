using JM.UI.Entities.Model.PurchaseReturnItems;
using JM.UI.Entities.Model.PurchaseReturns;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Newtonsoft.Json;
using JM.UI.Entities.Model.Users;

namespace JM.UI.Client.Pages.PurchaseReturns
{
    public partial class PurchaseReturnsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseReturnModelDTO PurchaseReturn { get; set; } = new();
        protected List<ReturnRefStockDetailDTO> ReturnItems { get; set; } = new();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();

        // Barcode scan
        protected string ScanBarcodeText { get; set; } = "";
        protected bool IsScanning { get; set; } = false;

        // Supplier lock
        protected int? LockedSupplierId { get; set; }
        protected string? LockedSupplierName { get; set; }
        protected bool IsSupplierLocked { get; set; } = false;

        // WH store
        protected StoreDTO? WhStore { get; set; }

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Return" : "New Purchase Return";

        // Summary
        protected int DistinctItemCount => ReturnItems.Select(i => i.Barcode).Distinct().Count();
        protected decimal TotalQty => ReturnItems.Sum(i => i.Quantity);
        protected decimal TotalValue => ReturnItems.Sum(i => i.Quantity * i.TradePrice);

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();
            await SetUserInfo();

            if (IsEditMode)
                await LoadPurchaseReturn();
        }

        private async Task SetUserInfo()
        {
            try
            {
                var userInfoResult = await sessionStorage.GetAsync<string>("UserInfo");
                if (userInfoResult.Success && !string.IsNullOrEmpty(userInfoResult.Value))
                {
                    var userInfo = JsonConvert.DeserializeObject<AuthenticatedUserResponse>(userInfoResult.Value);
                    if (userInfo != null)
                        PurchaseReturn.UserName = userInfo.Username;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user info: {ex.Message}");
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                Stores = await _serviceUnitOfWork.StoreService.GetStores();

                // Find WH store and lock it
                WhStore = Stores.FirstOrDefault(s =>
                    s.Code?.Equals("WH", StringComparison.OrdinalIgnoreCase) == true);
                if (WhStore != null)
                    PurchaseReturn.StoreId = WhStore.Id;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPurchaseReturn()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseReturnService.GetPurchaseReturnById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase Return not found.");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                    return;
                }

                PurchaseReturn = result;

                var items = await _serviceUnitOfWork.PurchaseReturnItemService.GetItemsByReturnId(Id.Value);
                ReturnItems = items.Select(i => new ReturnRefStockDetailDTO
                {
                    ProductName = i.ItemName ?? "",
                    Barcode = i.Barcode,
                    CurrentStock = i.CurrentStock,
                    PurchasePrice = i.TradePrice,
                    Quantity = i.Quantity,
                    TradePrice = i.TradePrice
                }).ToList();

                // Re-lock supplier from loaded data
                if (ReturnItems.Any())
                {
                    var first = ReturnItems.First();
                    if (first.SupplierId != null)
                    {
                        LockedSupplierId = first.SupplierId;
                        LockedSupplierName = first.SupplierName;
                        IsSupplierLocked = true;
                        PurchaseReturn.SupplierId = first.SupplierId.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase return: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseReturnsList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ── Barcode Scan ────────────────────────────────────────
        protected void OnScanBarcodeKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                ScanBarcode();
        }

        protected async void ScanBarcode()
        {
            var input = ScanBarcodeText?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty", "Please scan or enter a barcode.");
                return;
            }

            if (WhStore == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "No Store", "WH store not found. Cannot scan.");
                return;
            }

            try
            {
                IsScanning = true;
                StateHasChanged();

                var results = (await _serviceUnitOfWork.PurchaseReturnService
                    .GetReturnRefStockDetails(input, WhStore.Id)).ToList();

                if (!results.Any())
                {
                    notificationService.Notify(NotificationSeverity.Error, "Not Found",
                        $"No item found with barcode/ref '{input}'.");
                    return;
                }

                var scannedItem = results.First();
                scannedItem.TradePrice = scannedItem.PurchasePrice ?? 0;

                // ── Supplier lock check ──
                if (!IsSupplierLocked)
                {
                    if (scannedItem.SupplierId.HasValue)
                    {
                        LockedSupplierId = scannedItem.SupplierId;
                        LockedSupplierName = scannedItem.SupplierName;
                        IsSupplierLocked = true;
                        PurchaseReturn.SupplierId = scannedItem.SupplierId.Value;
                        notificationService.Notify(NotificationSeverity.Info, "Supplier Locked",
                            $"Supplier set to '{scannedItem.SupplierName}' for this return.");
                    }
                }
                else
                {
                    if (scannedItem.SupplierId != LockedSupplierId)
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Supplier Mismatch",
                            $"Item '{scannedItem.ProductName}' belongs to '{(scannedItem.SupplierName ?? "another supplier")}', " +
                            $"but this return is locked to '{LockedSupplierName}'. Cannot add.");
                        ScanBarcodeText = "";
                        IsScanning = false;
                        StateHasChanged();
                        return;
                    }
                }

                // ── Stock availability check ──
                var existing = ReturnItems.FirstOrDefault(i =>
                    i.Barcode?.Equals(scannedItem.Barcode, StringComparison.OrdinalIgnoreCase) == true);
                var newQty = existing != null ? existing.Quantity + 1 : 1;

                if (scannedItem.CurrentStock.HasValue && newQty > scannedItem.CurrentStock.Value)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Insufficient Stock",
                        $"Return qty ({newQty:N0}) exceeds current stock ({scannedItem.CurrentStock.Value:N0}) for '{scannedItem.ProductName}'.");
                    ScanBarcodeText = "";
                    IsScanning = false;
                    StateHasChanged();
                    return;
                }

                // ── Add or increment item ──
                if (existing != null)
                {
                    existing.Quantity += 1;
                }
                else
                {
                    scannedItem.Quantity = 1;
                    ReturnItems.Add(scannedItem);
                }

                ScanBarcodeText = "";
                notificationService.Notify(NotificationSeverity.Success, "Added", $"✓ {scannedItem.ProductName}");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load item: {ex.Message}");
            }
            finally
            {
                IsScanning = false;
                StateHasChanged();
            }
        }

        protected void RemoveLineItem(ReturnRefStockDetailDTO item)
        {
            ReturnItems.Remove(item);

            if (!ReturnItems.Any())
            {
                LockedSupplierId = null;
                LockedSupplierName = null;
                IsSupplierLocked = false;
                PurchaseReturn.SupplierId = 0;
            }
        }

        protected async Task Save()
        {
            if (!ReturnItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty Items", "Please add at least one item to return.");
                return;
            }

            try
            {
                IsProcessing = true;

                var userInfoResult = await sessionStorage.GetAsync<string>("UserInfo");
                if (userInfoResult.Success && !string.IsNullOrEmpty(userInfoResult.Value))
                {
                    var userInfo = JsonConvert.DeserializeObject<AuthenticatedUserResponse>(userInfoResult.Value);
                    if (userInfo != null)
                        PurchaseReturn.UserName = userInfo.Username;
                }

                // Map ReturnItems → PurchaseReturn.Items (for persistence)
                PurchaseReturn.Items = ReturnItems.Select((r, idx) => new PurchaseReturnItemModelDTO
                {
                    ItemName = r.ProductName,
                    Barcode = r.Barcode,
                    Quantity = r.Quantity,
                    TradePrice = r.TradePrice
                }).ToList();

                var result = await _serviceUnitOfWork.PurchaseReturnService.SaveUpdatePurchaseReturn(PurchaseReturn);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase Return updated successfully!" : "Purchase Return created successfully!");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase return: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Reset()
        {
            ScanBarcodeText = "";
            LockedSupplierId = null;
            LockedSupplierName = null;
            IsSupplierLocked = false;
            ReturnItems.Clear();
            PurchaseReturn = new PurchaseReturnModelDTO
            {
                ReturnDate = DateTime.Now,
                UserName = PurchaseReturn.UserName,
                StoreId = WhStore?.Id ?? 0
            };
            StateHasChanged();
        }

        protected void Cancel() => NavigationManager.NavigateTo("/PurchaseReturnsList");
    }
}
