using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfer
{
    public partial class DispatchedTransferListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<TransferMasterDTO> TransferGrid = default!;

        protected IEnumerable<TransferMasterDTO> Transfers { get; set; } = new List<TransferMasterDTO>();
        protected IEnumerable<TransferMasterDTO> FilteredTransfers { get; set; } = new List<TransferMasterDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected bool IsLoading { get; set; } = false;
        protected bool IsSaving { get; set; } = false;
        protected int? StoreId { get; set; } = null;
        protected int CurrentUserId { get; set; } = 0;
        protected bool IsStoreDropdownDisabled => CurrentUserId != 1;

        // Selected transfer (single selection)
        protected TransferMasterDTO? SelectedTransfer { get; set; } = null;
        protected bool HasSelectedTransfer => SelectedTransfer != null;

        // Barcode scan
        protected string BarcodeInput { get; set; } = string.Empty;
        protected string? BarcodeMatchMessage { get; set; } = null;
        protected BadgeStyle BarcodeMatchBadgeStyle { get; set; } = BadgeStyle.Info;
        protected Dictionary<int, int> BarcodeScanCounts { get; set; } = new();

        /// <summary>
        /// True when any loaded detail (across all expanded masters) has IsReceived == 1
        /// but the master itself is not yet fully received — i.e. there is unsaved receive work.
        /// </summary>
        protected bool HasPendingReceivedDetails =>
            Transfers.Any(t => t.Details != null &&
                          t.Details.Any(d => d.IsReceived == true));

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            CurrentUserId = await GetLocalStorageInt("UserId");
            await LoadStores();

            if (CurrentUserId == 1)
            {
                await LoadTransfers(0);
            }
            else
            {
                StoreId = await GetLocalStorageInt("StoreId");
                await LoadTransfers(StoreId);
            }
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
        // ─── Barcode Scan ─────────────────────────────────────────────────────────

        protected async Task OnBarcodeKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                await OnBarcodeSearch();
        }

        protected async Task OnBarcodeSearch()
        {
            var barcode = BarcodeInput?.Trim();
            if (string.IsNullOrWhiteSpace(barcode)) return;

            if (!HasSelectedTransfer)
            {
                BarcodeMatchMessage = "Please select a transfer first.";
                BarcodeMatchBadgeStyle = BadgeStyle.Warning;
                StateHasChanged();
                return;
            }

            // Ensure details are loaded for selected transfer
            if (SelectedTransfer!.Details == null)
            {
                try
                {
                    var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(SelectedTransfer.TransferId);
                    SelectedTransfer.Details = details.ToList();
                }
                catch
                {
                    BarcodeMatchMessage = "Failed to load transfer details.";
                    BarcodeMatchBadgeStyle = BadgeStyle.Danger;
                    StateHasChanged();
                    return;
                }
            }

            // Search only within selected transfer

            var matchedDetail = SelectedTransfer.Details
                .FirstOrDefault(d => barcode.All(char.IsDigit)
                    ? string.Equals(d.ReturnRefNo, barcode, StringComparison.OrdinalIgnoreCase)
                    : string.Equals(d.Barcode, barcode, StringComparison.OrdinalIgnoreCase));

            if (matchedDetail == null)
            {
                BarcodeMatchMessage = $"Barcode \"{barcode}\" not found in {SelectedTransfer.TransferNo}";
                BarcodeMatchBadgeStyle = BadgeStyle.Danger;
                StateHasChanged();
                return;
            }

            // Increment scan count for this item (cap at IssueQty)
            var detailId = matchedDetail.TransferDetailID;
            if (!BarcodeScanCounts.ContainsKey(detailId))
                BarcodeScanCounts[detailId] = 0;

            if (BarcodeScanCounts[detailId] < (int)matchedDetail.IssueQty)
                BarcodeScanCounts[detailId]++;

            // Auto-check only when scan count reaches the required quantity
            if (BarcodeScanCounts[detailId] >= (int)matchedDetail.IssueQty)
            {
                matchedDetail.IsReceived = true;
                CheckAndFlagMasterReceived(SelectedTransfer);
            }

            BarcodeMatchMessage = $"Scanned: {matchedDetail.ItemName} ({BarcodeScanCounts[detailId]}/{matchedDetail.IssueQty}) in {SelectedTransfer.TransferNo}";
            BarcodeMatchBadgeStyle = BadgeStyle.Success;

            StateHasChanged();
        }

        protected void ClearBarcodeSearch()
        {
            BarcodeInput = string.Empty;
            BarcodeMatchMessage = null;
            StateHasChanged();
        }

        // ─── Detail receive toggles ───────────────────────────────────────────────

        protected void ToggleDetailReceived(TransferMasterDTO master, TransferDetailDTO detail, bool isChecked)
        {
            detail.IsReceived = isChecked ? true : false;
            if (isChecked)
                BarcodeScanCounts[detail.TransferDetailID] = (int)detail.IssueQty;
            else
                BarcodeScanCounts.Remove(detail.TransferDetailID);
            CheckAndFlagMasterReceived(master);
            StateHasChanged();
        }

        protected void ToggleAllDetailReceived(TransferMasterDTO master, bool isChecked)
        {
            if (master.Details == null) return;
            foreach (var d in master.Details)
            {
                d.IsReceived = isChecked ? true : false;
                if (isChecked)
                    BarcodeScanCounts[d.TransferDetailID] = (int)d.IssueQty;
                else
                    BarcodeScanCounts.Remove(d.TransferDetailID);
            }
            CheckAndFlagMasterReceived(master);
            StateHasChanged();
        }

        /// <summary>
        /// If every detail of a master is IsReceived == 1, mark the master locally as well.
        /// </summary>
        private void CheckAndFlagMasterReceived(TransferMasterDTO master)
        {
            if (master.Details == null || !master.Details.Any()) return;

            if (master.Details.All(d => d.IsReceived == true))
            {
                master.IsReceived = true;
                master.ReceivedDate = DateTime.Now;
                master.ReceivedBy = 1; // implement in PosComponentBase or inject
            }
        }

        protected bool CanMarkReceived(TransferMasterDTO master)
        {
            if (master.Details == null || !master.Details.Any()) return false;
            return master.Details.All(d => d.IsReceived == true);
        }

        protected async Task SelectTransfer(TransferMasterDTO transfer)
        {
            // Toggle selection — clicking the same transfer deselects
            SelectedTransfer = SelectedTransfer?.TransferId == transfer.TransferId ? null : transfer;

            if (HasSelectedTransfer)
            {
                // Load details for the selected transfer
                if (SelectedTransfer!.Details == null)
                {
                    try
                    {
                        var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(SelectedTransfer.TransferId);
                        SelectedTransfer.Details = details.ToList();
                    }
                    catch
                    {
                        // Ignore load failure; user can expand manually
                    }
                }

                // Auto-expand the selected row
                if (SelectedTransfer.Details != null)
                    await TransferGrid.ExpandRow(SelectedTransfer);
            }

            BarcodeInput = string.Empty;
            BarcodeMatchMessage = null;
            BarcodeScanCounts.Clear();
            StateHasChanged();
        }

        // ─── Mark single master received (button) ────────────────────────────────

        protected async Task MarkMasterReceived(TransferMasterDTO transfer)
        {
            if (!CanMarkReceived(transfer))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning",
                    "All items must be marked as received before confirming.");
                return;
            }

            try
            {
                IsLoading = true;

                // Ensure details are loaded first
                if (transfer.Details == null)
                {
                    var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(transfer.TransferId);
                    transfer.Details = details.ToList();
                }

                // Mark master locally
                transfer.IsReceived = true;
                transfer.ReceivedDate = DateTime.Now;
                transfer.ReceivedBy = 1;

                // --- Call your API here when ready ---
                // var result = await _serviceUnitOfWork.TransferService.MarkMasterReceived(
                //     transfer.TransferId, transfer.ReceivedDate.Value, transfer.ReceivedBy);
                //
                // if (!result.IsSuccessStatus)
                // {
                //     notificationService.Notify(NotificationSeverity.Warning, "Warning", result.Message);
                //     return;
                // }

                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"Transfer {transfer.TransferNo} marked as received.");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to mark received: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        // ─── Final Save ───────────────────────────────────────────────────────────

        /// <summary>
        /// Sends all locally-checked detail rows (IsReceived == 1) to the API.
        /// After saving, checks each master — if all its details are received, updates master too.
        /// </summary>
        protected async Task SaveReceivedDetails()
        {
            try
            {
                IsSaving = true;

                // Collect all detail IDs that are marked received across all loaded masters
                var receivedDetailIds = Transfers
                    .Where(t => t.Details != null)
                    .SelectMany(t => t.Details!)
                    .Where(d => d.IsReceived == true)
                    .Select(d => d.TransferDetailID)
                    .ToList();

                if (!receivedDetailIds.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Warning",
                        "No detail items are marked as received.");
                    return;
                }

                // --- Call your API here when ready ---
                // var detailResult = await _serviceUnitOfWork.TransferService
                //     .UpdateDetailReceivedStatus(receivedDetailIds, 1);
                // if (!detailResult.IsSuccessStatus) { ... return; }

                // For masters where all details are now received, update master status
                var fullyReceivedMasterIds = Transfers
    .Where(t => t.Details != null &&
                t.Details.Any(d => receivedDetailIds.Contains(d.TransferDetailID)))
    .Select(t => t.TransferId)
    .ToList();

                if (fullyReceivedMasterIds.Any())
                {
                    // --- Call your API here when ready ---
                    await _serviceUnitOfWork.TransferService.UpdateReceivedStatus(receivedDetailIds, fullyReceivedMasterIds, DateTime.Now, 1);

                    await OnInitializedAsync();
                }

                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"Receipt confirmed for {receivedDetailIds.Count} item(s). " +
                    (fullyReceivedMasterIds.Any()
                        ? $"{fullyReceivedMasterIds.Count} transfer(s) fully received."
                        : string.Empty));

                // Reload to reflect persisted state
                await LoadTransfers(StoreId ?? 0);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to save received status: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
                StateHasChanged();
            }
        }

        protected async Task OnStoreChanged(int storeId)
        {
            await LoadTransfers(storeId);
            StateHasChanged();
        }

        // ─── Data Loading ─────────────────────────────────────────────────────────

        private async Task LoadStores()
        {
            try
            {
                var stores = await _serviceUnitOfWork.StoreService.GetStores();

                if (CurrentUserId == 1)
                {
                    Stores = stores;
                }
                else
                {
                    var currentStoreId = await GetLocalStorageInt("StoreId");
                    Stores = stores.Where(s => s.Id == currentStoreId).ToList();
                    StoreId = Stores.FirstOrDefault()?.Id;
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load stores: {ex.Message}");
            }
        }

        private async Task LoadTransfers(int? storeId)
        {
            try
            {
                IsLoading = true;
                BarcodeMatchMessage = null;
                BarcodeInput = string.Empty;
                SelectedTransfer = null;
                BarcodeScanCounts.Clear();

                Transfers = await _serviceUnitOfWork.TransferService.GetDispatchedTransfers(storeId ?? 0);
                FilteredTransfers = Transfers;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load transfers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OnRowExpand(TransferMasterDTO master)
        {
            try
            {
                if (master.Details == null || !master.Details.Any())
                {
                    var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(master.TransferId);
                    master.Details = details.ToList();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load transfer details: {ex.Message}");
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        protected void AddTransfer() => NavigationManager.NavigateTo("/TransferAdd");

        protected void EditTransfer(TransferMasterDTO transfer) =>
            NavigationManager.NavigateTo($"/TransferAdd/{transfer.TransferId}");

        protected BadgeStyle GetStatusBadgeStyle(int? statusId) => statusId switch
        {
            1 => BadgeStyle.Warning,
            2 => BadgeStyle.Info,
            3 => BadgeStyle.Success,
            4 => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };

        protected string GetStatusLabel(int? statusId) => statusId switch
        {
            1 => "Pending",
            2 => "In Transit",
            3 => "Received",
            4 => "Cancelled",
            _ => "Unknown"
        };

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? "—"
                : value.Length <= maxChars ? value : value[..maxChars] + "...";

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text,
                new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            TransferGrid?.Dispose();
        }
    }
}