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

        // Barcode scan
        protected string BarcodeInput { get; set; } = string.Empty;
        protected string? BarcodeMatchMessage { get; set; } = null;
        protected BadgeStyle BarcodeMatchBadgeStyle { get; set; } = BadgeStyle.Info;

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
            await LoadStores();
            await LoadTransfers(0);
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

            // Make sure all masters have their details loaded so we can search inside them.
            // For masters whose details haven't been loaded yet we call the service.
            foreach (var master in Transfers)
            {
                if (master.Details == null)
                {
                    try
                    {
                        var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(master.TransferId);
                        master.Details = details.ToList();
                    }
                    catch { /* skip if fails */ }
                }
            }

            // Find masters that contain a detail with this barcode
            var matchingMasters = Transfers
                .Where(t => t.Details != null &&
                            t.Details.Any(d => string.Equals(d.Barcode, barcode, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!matchingMasters.Any())
            {
                BarcodeMatchMessage = $"No match for \"{barcode}\"";
                BarcodeMatchBadgeStyle = BadgeStyle.Danger;
                FilteredTransfers = Transfers; // reset filter
                StateHasChanged();
                return;
            }

            // Filter grid to matching masters
            FilteredTransfers = matchingMasters;

            // Auto-check the matched detail rows (set IsReceived = 1 locally)
            foreach (var master in matchingMasters)
            {
                var matchedDetails = master.Details!
                    .Where(d => string.Equals(d.Barcode, barcode, StringComparison.OrdinalIgnoreCase));

                foreach (var detail in matchedDetails)
                    detail.IsReceived = true;

                // If all details of this master are now received, flag master too (locally)
                CheckAndFlagMasterReceived(master);
            }

            BarcodeMatchMessage = $"{matchingMasters.Count} transfer(s) matched";
            BarcodeMatchBadgeStyle = BadgeStyle.Success;

            StateHasChanged();
        }

        protected void ClearBarcodeSearch()
        {
            BarcodeInput = string.Empty;
            BarcodeMatchMessage = null;
            FilteredTransfers = Transfers;
            StateHasChanged();
        }

        // ─── Detail receive toggles ───────────────────────────────────────────────

        protected void ToggleDetailReceived(TransferMasterDTO master, TransferDetailDTO detail, bool isChecked)
        {
            detail.IsReceived = isChecked ? true : false;
            CheckAndFlagMasterReceived(master);
            StateHasChanged();
        }

        protected void ToggleAllDetailReceived(TransferMasterDTO master, bool isChecked)
        {
            if (master.Details == null) return;
            foreach (var d in master.Details)
                d.IsReceived = isChecked ? true : false;
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

        // ─── Mark single master received (button) ────────────────────────────────

        protected async Task MarkMasterReceived(TransferMasterDTO transfer)
        {
            try
            {
                IsLoading = true;

                // Ensure details are loaded first
                if (transfer.Details == null)
                {
                    var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(transfer.TransferId);
                    transfer.Details = details.ToList();
                }

                // Mark all details received locally
                foreach (var d in transfer.Details)
                    d.IsReceived = true;

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
                Stores = await _serviceUnitOfWork.StoreService.GetStores();
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