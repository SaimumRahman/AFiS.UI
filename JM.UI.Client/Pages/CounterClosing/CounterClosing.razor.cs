using JM.UI.Entities.Model.CounterClosing;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.CounterClosing
{
    public partial class CounterClosingComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CounterTransactionDTO> TransactionsGrid = default!;
        protected RadzenDataGrid<CounterClosingDTO> HistoryGrid = default!;

        protected List<StoreDTO> Stores = new();
        protected List<CounterDTO> Counters = new();
        protected List<CounterClosingDTO> History = new();

        protected int? SelectedStoreId;
        protected int? SelectedCounterId;
        protected DateTime? SelectedBusinessDate;

        protected CounterClosingPreviewDTO? Preview;
        protected decimal? ActualCash;
        protected string? Note;
        protected List<CashMovementRow> Movements = new();

        protected string? SearchText;

        protected bool IsLoading;
        protected bool IsSubmitting;
        protected bool IsLoadingHistory;
        protected bool IsStoreLocked;
        protected bool CanManageCounters;

        protected int CurrentUserId;
        protected string? CurrentUserName;

        protected CounterClosingDTO? ReopenTarget;
        protected string? ReopenReason;
        protected bool IsReopening;

        /* ── Totals (client preview only; the server recomputes on submit) ────── */

        protected decimal TotalCashIn => Movements.Where(m => m.Direction == "IN").Sum(m => m.Amount ?? 0);
        protected decimal TotalCashOut => Movements.Where(m => m.Direction == "OUT").Sum(m => m.Amount ?? 0);

        protected decimal ExpectedCash => Preview == null
            ? 0
            : Preview.OpeningCash + Preview.CashSales + TotalCashIn - TotalCashOut;

        protected decimal Difference => Math.Round((ActualCash ?? 0) - ExpectedCash, 2);

        protected bool HasDifference => ActualCash.HasValue && Difference != 0;

        protected string DifferenceColor => Difference < 0 ? "#c62828" : Difference > 0 ? "#ef6c00" : "#2e7d32";

        protected string DifferenceLabel => Difference < 0 ? "Shortage" : Difference > 0 ? "Overage" : "Matched";

        protected decimal PaymentTotal => Preview?.PaymentSummary?.Sum(p => p.Amount) ?? 0;

        protected List<CounterTransactionDTO> Transactions
        {
            get
            {
                var all = Preview?.Transactions ?? new List<CounterTransactionDTO>();
                if (string.IsNullOrWhiteSpace(SearchText))
                    return all;

                var term = SearchText.Trim();
                return all.Where(t =>
                        (t.InvoiceNo?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (t.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (t.PaymentMethod?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (t.Status?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }
        }

        /* ── Lifecycle ───────────────────────────────────────────────────────── */

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await TokenService.InitializeTokenAsync();

                CurrentUserId = await GetLocalStorageInt("UserId");
                CurrentUserName = await GetStoredUserName();

                var localStoreId = await GetLocalStorageInt("StoreId");
                Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new List<StoreDTO>();

                // "Admin" = a member of any group with IsSystem = 1 (the API
                // resolves this; UserId 1 is always included as a fallback).
                try
                {
                    CanManageCounters = await _serviceUnitOfWork.CounterClosingService
                        .CanManageCounters(CurrentUserId, CurrentUserName);
                }
                catch
                {
                    // A failed permission probe falls back to the legacy rule.
                    CanManageCounters = CurrentUserId == 1;
                }

                if (localStoreId > 0 && Stores.Any(s => s.Id == localStoreId))
                    SelectedStoreId = localStoreId;
                else
                    SelectedStoreId = Stores.FirstOrDefault()?.Id;

                // System-group admins may pick any branch; everyone else is
                // pinned to the store on their session.
                IsStoreLocked = localStoreId > 0 && !CanManageCounters;

                await LoadCounters();
                await LoadHistory();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load failed", ex.Message, 5000);
            }
        }

        /* ── Data loading ────────────────────────────────────────────────────── */

        protected async Task OnStoreChanged()
        {
            SelectedCounterId = null;
            await LoadCounters();
            await LoadHistory();
        }

        protected async Task LoadCounters()
        {
            if (!SelectedStoreId.HasValue)
                return;

            try
            {
                IsLoading = true;
                Counters = (await _serviceUnitOfWork.CounterClosingService
                        .GetCounters(SelectedStoreId.Value))?.ToList() ?? new List<CounterDTO>();

                if (!SelectedCounterId.HasValue || !Counters.Any(c => c.Id == SelectedCounterId.Value))
                {
                    SelectedCounterId = Counters.FirstOrDefault(c => c.IsDefault)?.Id
                                        ?? Counters.FirstOrDefault()?.Id;
                }

                await LoadPreview();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Counters", ex.Message, 5000);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task LoadPreview()
        {
            if (!SelectedStoreId.HasValue || !SelectedCounterId.HasValue)
            {
                Preview = null;
                return;
            }

            try
            {
                IsLoading = true;
                Preview = await _serviceUnitOfWork.CounterClosingService
                    .GetPreview(SelectedStoreId.Value, SelectedCounterId.Value, SelectedBusinessDate);

                Movements = new List<CashMovementRow>();
                ActualCash = null;
                Note = null;

                if (Preview == null)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Not found",
                        "That counter does not belong to the selected store.", 5000);
                    return;
                }

                if (Preview.IsClosed)
                {
                    // Closed days are read-only: hydrate the stored reconciliation.
                    ActualCash = Preview.ActualCash;
                    Note = Preview.Note;
                    Movements = (Preview.CashMovements ?? new List<CounterCashMovementDTO>())
                        .Select(m => new CashMovementRow
                        {
                            Direction = m.Direction,
                            Amount = m.Amount,
                            Reason = m.Reason
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Preview", ex.Message, 5000);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task LoadHistory()
        {
            try
            {
                IsLoadingHistory = true;
                History = (await _serviceUnitOfWork.CounterClosingService
                        .GetHistory(null, SelectedStoreId, 50))?.ToList() ?? new List<CounterClosingDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "History", ex.Message, 5000);
            }
            finally
            {
                IsLoadingHistory = false;
                StateHasChanged();
            }
        }

        /* ── Cash movements ──────────────────────────────────────────────────── */

        protected void AddMovement(string direction)
        {
            Movements.Add(new CashMovementRow { Direction = direction, Amount = null });
        }

        protected void RemoveMovement(CashMovementRow row)
        {
            Movements.Remove(row);
        }

        /* ── Submit ──────────────────────────────────────────────────────────── */

        protected async Task SubmitClosing()
        {
            if (Preview == null || !SelectedCounterId.HasValue || !SelectedStoreId.HasValue)
                return;

            if (Preview.IsClosed)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Already closed",
                    "This counter is already closed for that business day.", 5000);
                return;
            }

            if (Preview.PendingCount > 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Open drafts",
                    $"{Preview.PendingCount} draft sale(s) are still open on this counter. Complete or discard them first.", 6000);
                return;
            }

            if (!ActualCash.HasValue)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Actual cash required",
                    "Enter the cash counted in the drawer.", 5000);
                return;
            }

            if (HasDifference && string.IsNullOrWhiteSpace(Note))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Note required",
                    "A note is required when the counted cash differs from the expected cash.", 5000);
                return;
            }

            var missingReason = Movements.FirstOrDefault(m => (m.Amount ?? 0) > 0 && string.IsNullOrWhiteSpace(m.Reason));
            if (missingReason != null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Reason required",
                    "Every cash in / out movement needs a short reason.", 5000);
                return;
            }

            var confirm = await dialogService.Confirm(
                $"Close {Preview.CounterName} for business day {Preview.BusinessDate:dd-MMM-yyyy}?\n\n" +
                $"Opening: {Preview.OpeningCash:N2} | Cash sales: {Preview.CashSales:N2} | " +
                $"In: {TotalCashIn:N2} | Out: {TotalCashOut:N2}\n" +
                $"Expected: {ExpectedCash:N2} | Counted: {ActualCash.Value:N2} | Difference: {Difference:N2}\n\n" +
                "No new sale, return or edit will be allowed on this counter for that business day.",
                "Confirm day-end closing",
                new ConfirmOptions { OkButtonText = "Close counter", CancelButtonText = "Cancel" });

            if (confirm != true)
                return;

            try
            {
                IsSubmitting = true;
                StateHasChanged();

                var dto = new SubmitCounterCloseDTO
                {
                    StoreId = SelectedStoreId.Value,
                    CounterId = SelectedCounterId.Value,
                    BusinessDate = Preview.BusinessDate,
                    ActualCash = ActualCash.Value,
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                    ClosedBy = CurrentUserId,
                    ClosedByName = CurrentUserName,
                    CashMovements = Movements
                        .Where(m => (m.Amount ?? 0) > 0)
                        .Select(m => new CounterCashMovementDTO
                        {
                            Direction = m.Direction,
                            Amount = m.Amount!.Value,
                            Reason = m.Reason?.Trim()
                        }).ToList()
                };

                var result = await _serviceUnitOfWork.CounterClosingService.CloseCounter(dto);

                notificationService.Notify(
                    result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Counter closed" : "Closing failed",
                    result.Message, 6000);

                if (result.IsSuccessStatus)
                {
                    await LoadPreview();
                    await LoadHistory();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Closing failed", ex.Message, 6000);
            }
            finally
            {
                IsSubmitting = false;
                StateHasChanged();
            }
        }

        /* ── Reopen (manager, audited) ───────────────────────────────────────── */

        protected void BeginReopen(CounterClosingDTO closing)
        {
            ReopenTarget = closing;
            ReopenReason = null;
        }

        protected void CancelReopen()
        {
            ReopenTarget = null;
            ReopenReason = null;
        }

        protected async Task ConfirmReopenClick()
        {
            if (ReopenTarget == null)
                return;

            if (string.IsNullOrWhiteSpace(ReopenReason) || ReopenReason.Trim().Length < 5)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Reason required",
                    "Enter a reopen reason of at least 5 characters.", 5000);
                return;
            }

            try
            {
                IsReopening = true;
                StateHasChanged();

                var dto = new ReopenCounterCloseDTO
                {
                    ClosingId = ReopenTarget.Id,
                    ReopenedBy = CurrentUserId,
                    ReopenedByName = CurrentUserName,
                    Reason = ReopenReason.Trim()
                };

                var result = await _serviceUnitOfWork.CounterClosingService.ReopenCounter(dto);

                notificationService.Notify(
                    result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Counter reopened" : "Reopen failed",
                    result.Message, 6000);

                if (result.IsSuccessStatus)
                {
                    var businessDate = ReopenTarget.BusinessDate;
                    CancelReopen();

                    // If the reopened day is the one on screen, reload it so the
                    // cash fields become editable again.
                    if (Preview != null && Preview.BusinessDate.Date == businessDate.Date)
                        await LoadPreview();

                    await LoadHistory();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Reopen failed", ex.Message, 6000);
            }
            finally
            {
                IsReopening = false;
                StateHasChanged();
            }
        }

        protected void CancelReopenClick()
        {
            CancelReopen();
        }

        protected async Task OpenHistoryRow(CounterClosingDTO closing)
        {
            SelectedStoreId = closing.StoreId;
            await LoadCounters();
            SelectedCounterId = closing.CounterId;
            SelectedBusinessDate = closing.BusinessDate;
            await LoadPreview();
        }

        protected async Task RefreshClick()
        {
            await LoadPreview();
            await LoadHistory();
        }

        protected async Task StoreChanged()
        {
            SelectedCounterId = null;
            await LoadCounters();
            await LoadHistory();
        }

        protected async Task CounterChanged()
        {
            await LoadPreview();
        }

        protected async Task DatePickerChanged()
        {
            await LoadPreview();
        }

        protected async Task LoadDayClick()
        {
            await LoadPreview();
        }

        /* ── Counter master ──────────────────────────────────────────────────── */

        protected async Task OpenCounterManagement()
        {
            if (!SelectedStoreId.HasValue)
                return;

            var storeName = Stores.FirstOrDefault(s => s.Id == SelectedStoreId.Value)?.Name ?? string.Empty;

            var parameters = new Dictionary<string, object>
            {
                ["StoreId"] = SelectedStoreId.Value,
                ["StoreName"] = storeName,
                ["Stores"] = Stores,
                ["CurrentUserId"] = CurrentUserId,
                ["CurrentUserName"] = CurrentUserName ?? string.Empty,
                ["CanManage"] = CanManageCounters,
                ["IsAdmin"] = CanManageCounters
            };

            var changed = await dialogService.OpenAsync<JM.UI.Client.Pages.Dialog.CounterClosing.CounterManagementDialog>(
                "Manage counters",
                parameters,
                new DialogOptions
                {
                    Width = "900px",
                    Height = "auto",
                    CloseDialogOnOverlayClick = false,
                    Resizable = true,
                    Draggable = true
                });

            if (changed is bool hasChanges && hasChanges)
            {
                await LoadCounters();
                await LoadHistory();
            }
        }

        protected async Task ReloadHistoryClick()
        {
            await LoadHistory();
        }

        protected async Task AddCashInClick()
        {
            AddMovement("IN");
        }

        protected async Task AddCashOutClick()
        {
            AddMovement("OUT");
        }

        protected async Task ConfirmReopen()
        {
            if (ReopenTarget == null)
                return;

            if (string.IsNullOrWhiteSpace(ReopenReason) || ReopenReason.Trim().Length < 5)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Reason required",
                    "Enter a reopen reason of at least 5 characters.", 5000);
                return;
            }

            try
            {
                IsReopening = true;
                StateHasChanged();

                var dto = new ReopenCounterCloseDTO
                {
                    ClosingId = ReopenTarget.Id,
                    ReopenedBy = CurrentUserId,
                    ReopenedByName = CurrentUserName,
                    Reason = ReopenReason.Trim()
                };

                var result = await _serviceUnitOfWork.CounterClosingService.ReopenCounter(dto);

                notificationService.Notify(
                    result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Counter reopened" : "Reopen failed",
                    result.Message, 6000);

                if (result.IsSuccessStatus)
                {
                    var businessDate = ReopenTarget.BusinessDate;
                    CancelReopen();

                    // If the reopened day is the one on screen, reload it so the
                    // cash fields become editable again.
                    if (Preview != null && Preview.BusinessDate.Date == businessDate.Date)
                        await LoadPreview();

                    await LoadHistory();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Reopen failed", ex.Message, 6000);
            }
            finally
            {
                IsReopening = false;
                StateHasChanged();
            }
        }

        /* ── Helpers ─────────────────────────────────────────────────────────── */

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && int.TryParse(result.Value, out var parsed))
                    return parsed;
            }
            catch
            {
                // Storage is unavailable during prerender; treat as zero.
            }

            return 0;
        }

        private async Task<string?> GetStoredUserName()
        {
            try
            {
                var result = await _localStorage.GetAsync<string>("UserInfo");
                if (!result.Success || string.IsNullOrWhiteSpace(result.Value))
                    return null;

                using var doc = JsonDocument.Parse(result.Value);
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name.Equals("Username", StringComparison.OrdinalIgnoreCase)
                        || property.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value.GetString();
                    }
                }
            }
            catch
            {
                // Fall back to the user id shown in the header.
            }

            return null;
        }

        protected string StatusColor(CounterClosingDTO closing) =>
            closing.Status == "CLOSED" ? "#2e7d32" : "#ef6c00";

        protected string TransactionTypeColor(string type) => type switch
        {
            "Return" => "#c62828",
            "Exchange" => "#6a1b9a",
            _ => "#1565c0"
        };

        public void Dispose()
        {
            TransactionsGrid?.Dispose();
            HistoryGrid?.Dispose();
        }

        public class CashMovementRow
        {
            public string Direction { get; set; } = "OUT";
            public decimal? Amount { get; set; }
            public string? Reason { get; set; }
        }
    }
}
