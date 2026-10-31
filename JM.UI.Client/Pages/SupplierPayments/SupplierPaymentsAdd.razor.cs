using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SupplierPayments
{
    public partial class SupplierPaymentsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Parameter] public int Id { get; set; }

        protected SupplierPaymentDTO PaymentModel { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> SuppliersList = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> StoresList = new List<StoreDTO>();

        // ── Payment source (cash / financial account / cheque) ──────────────
        protected string PaymentMode { get; set; } = "Cash";
        protected int? SelectedFinancialAccountId { get; set; }
        protected string? SelectedCashLedgerCode { get; set; }
        protected SupplierPaymentSourcesDTO? PaymentSources { get; set; }
        protected bool IsLoadingSources { get; set; }
        protected bool IsSaving { get; set; }

        protected int CurrentUserId { get; set; }
        protected int CurrentStoreId { get; set; }
        protected bool CanSelectAnyStore { get; set; }

        protected List<PaymentModeOption> PaymentModes { get; } = new()
        {
            new("Cash", "Cash"),
            new("FinancialAccount", "Bank / MFS"),
            new("Cheque", "Cheque")
        };

        public sealed record PaymentModeOption(string Value, string Label);

        protected List<SupplierPaymentSourceDTO> CashLedgers
            => PaymentSources?.CashLedgers ?? new List<SupplierPaymentSourceDTO>();

        protected List<SupplierPaymentSourceDTO> FinancialAccounts
            => PaymentSources?.FinancialAccounts ?? new List<SupplierPaymentSourceDTO>();

        protected bool IsSystemGroupUser => PaymentSources?.IsSystemGroupUser ?? false;

        protected SupplierPaymentSourceDTO? SelectedSource =>
            PaymentMode == "Cash"
                ? CashLedgers.FirstOrDefault(x => x.LedgerCode == SelectedCashLedgerCode)
                : FinancialAccounts.FirstOrDefault(x => x.FinancialAccountId == SelectedFinancialAccountId);

        protected decimal AvailableBalance => SelectedSource?.Balance ?? 0m;

        protected bool IsPostDatedCheque =>
            PaymentMode == "Cheque"
            && PaymentModel.ChequeDate.HasValue
            && PaymentModel.ChequeDate.Value.Date > DateTime.Today;

        protected bool IsInsufficient =>
            !IsPostDatedCheque
            && PaymentModel.PaymentAmount > 0
            && SelectedSource != null
            && AvailableBalance < PaymentModel.PaymentAmount;

        protected string SelectedLedgerText => SelectedSource == null
            ? "-"
            : $"{SelectedSource.LedgerName} ({SelectedSource.LedgerCode})";

        protected bool CanSave =>
            !IsSaving
            && PaymentModel.SupplierId > 0
            && PaymentModel.StoreId > 0
            && PaymentModel.PaymentAmount > 0
            && SelectedSource != null
            && (PaymentMode != "Cheque"
                || (!string.IsNullOrWhiteSpace(PaymentModel.ChequeNo) && PaymentModel.ChequeDate.HasValue))
            && (IsPostDatedCheque || !IsInsufficient);

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            await ResolveStoreSelectionPermissionAsync();

            await LoadDropdownsAsync();

            if (Id != 0)
            {
                var payment = await _serviceUnitOfWork.SupplierPaymentService.GetSupplierPaymentById(Id);
                if (payment != null)
                {
                    PaymentModel = payment;
                    InitializeModeFromModel();
                }
            }
            else
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                PaymentModel.PaymentDate = DateTime.Today;
                PaymentModel.PaymentMethod = "Cash";
                PaymentModel.UserName = authState.User.Identity?.Name ?? "System";
                PaymentModel.StoreId = CurrentStoreId;
                PaymentMode = "Cash";
            }

            await LoadPaymentSourcesAsync();
            ApplySourceSelection();
        }

        private async Task ResolveStoreSelectionPermissionAsync()
        {
            CanSelectAnyStore = CurrentUserId == 1;
            if (CanSelectAnyStore) return;

            try
            {
                CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                    .GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch
            {
                // Fail closed: a failed lookup must not hand out cross-store visibility.
                CanSelectAnyStore = false;
            }
        }

        private async Task LoadDropdownsAsync()
        {
            SuppliersList = await _serviceUnitOfWork.SupplierService.GetSuppliers()
                             ?? new List<SupplierModelDTO>();

            var stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
            StoresList = stores
                .Where(s => !string.Equals(s.Name?.Trim(), "Damaged", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void InitializeModeFromModel()
        {
            var method = PaymentModel.PaymentMethod ?? "Cash";

            if (string.Equals(method, "Cheque", StringComparison.OrdinalIgnoreCase) || PaymentModel.ChequeDate.HasValue)
                PaymentMode = "Cheque";
            else if (string.Equals(method, "Cash", StringComparison.OrdinalIgnoreCase))
                PaymentMode = "Cash";
            else
                PaymentMode = "FinancialAccount";

            // Legacy cash rows were sometimes posted to a bank ledger because a stale
            // BankId was left on the form. Treat those as financial-account payments so
            // editing does not silently move the money to the cash ledger.
            if (PaymentMode == "Cash"
                && !string.IsNullOrWhiteSpace(PaymentModel.PaymentLedgerCode)
                && !PaymentModel.PaymentLedgerCode.StartsWith("1110", StringComparison.Ordinal))
            {
                PaymentMode = PaymentModel.ChequeDate.HasValue ? "Cheque" : "FinancialAccount";
            }

            PaymentModel.PaymentMethod = PaymentMode;
            SelectedFinancialAccountId = PaymentModel.FinancialAccountId;
            SelectedCashLedgerCode = PaymentMode == "Cash"
                ? (string.IsNullOrWhiteSpace(PaymentModel.PaymentLedgerCode) ? "111001" : PaymentModel.PaymentLedgerCode)
                : null;
        }

        private async Task LoadPaymentSourcesAsync()
        {
            PaymentSources = null;

            if (PaymentModel.StoreId <= 0)
                return;

            IsLoadingSources = true;
            try
            {
                PaymentSources = await _serviceUnitOfWork.SupplierPaymentService
                    .GetPaymentSources(PaymentModel.StoreId);
            }
            catch (Exception ex)
            {
                notificationService.Notify(Radzen.NotificationSeverity.Error,
                    "Payment sources", $"Could not load cash / bank balances: {ex.Message}");
            }
            finally
            {
                IsLoadingSources = false;
            }
        }

        private void ApplySourceSelection()
        {
            if (PaymentMode == "Cash")
            {
                if (CashLedgers.Count == 0)
                {
                    SelectedCashLedgerCode = null;
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedCashLedgerCode)
                    || CashLedgers.All(x => x.LedgerCode != SelectedCashLedgerCode))
                {
                    SelectedCashLedgerCode = CashLedgers
                        .FirstOrDefault(x => x.LedgerCode == "111001")?.LedgerCode
                        ?? CashLedgers[0].LedgerCode;
                }

                return;
            }

            // Only auto-pick on edit; on a new payment the user must choose.
            if (Id == 0)
                return;

            if (SelectedFinancialAccountId.HasValue
                && FinancialAccounts.Any(x => x.FinancialAccountId == SelectedFinancialAccountId))
            {
                return;
            }

            SelectedFinancialAccountId = FinancialAccounts
                .FirstOrDefault(x => string.Equals(x.LedgerCode, PaymentModel.PaymentLedgerCode,
                    StringComparison.OrdinalIgnoreCase))
                ?.FinancialAccountId;
        }

        protected async Task OnStoreChanged(object value)
        {
            if (PaymentModel.StoreId <= 0)
                return;

            SelectedFinancialAccountId = null;
            SelectedCashLedgerCode = null;

            await LoadPaymentSourcesAsync();
            ApplySourceSelection();
        }

        protected void OnModeChanged(object value)
        {
            var mode = value?.ToString() ?? "Cash";

            // Accept the legacy "Bank" label if an older row sends it.
            if (string.Equals(mode, "Bank", StringComparison.OrdinalIgnoreCase))
                mode = "FinancialAccount";

            PaymentMode = mode;
            PaymentModel.PaymentMethod = mode;

            if (mode == "Cash")
            {
                SelectedFinancialAccountId = null;
                PaymentModel.ChequeNo = null;
                PaymentModel.ChequeDate = null;
                if (string.IsNullOrWhiteSpace(SelectedCashLedgerCode))
                    SelectedCashLedgerCode = CashLedgers.FirstOrDefault()?.LedgerCode;
            }
            else
            {
                SelectedCashLedgerCode = null;
            }

            if (mode != "Cheque")
            {
                PaymentModel.ChequeNo = null;
                PaymentModel.ChequeDate = null;
            }

            StateHasChanged();
        }

        protected void OnCashLedgerChanged(object value)
        {
            SelectedCashLedgerCode = value?.ToString();
            StateHasChanged();
        }

        protected void OnFinancialAccountChanged(object value)
        {
            SelectedFinancialAccountId = int.TryParse(value?.ToString(), out var id) && id > 0
                ? id
                : null;
            StateHasChanged();
        }

        protected async Task SavePayment(SupplierPaymentDTO payment)
        {
            if (!CanSave)
            {
                notificationService.Notify(Radzen.NotificationSeverity.Warning,
                    "Incomplete payment",
                    IsInsufficient
                        ? $"Insufficient balance in the selected account. Available {AvailableBalance:N2}."
                        : "Select a supplier, store, payment source and a valid amount.");
                return;
            }

            try
            {
                IsSaving = true;

                payment.PaymentMethod = PaymentMode;
                payment.FinancialAccountId = PaymentMode == "Cash" ? null : SelectedFinancialAccountId;
                payment.PaymentLedgerCode = PaymentMode == "Cash"
                    ? SelectedCashLedgerCode
                    : SelectedSource?.LedgerCode;

                // BankId is derived server-side from the selected financial account, so a
                // stale value from a previous mode can never redirect the ledger.
                payment.BankId = null;

                if (PaymentMode != "Cheque")
                {
                    payment.ChequeNo = null;
                    payment.ChequeDate = null;
                }

                if (PaymentMode == "Cash")
                    payment.TransactionNo = null;

                if (string.IsNullOrWhiteSpace(payment.UserName))
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    payment.UserName = authState.User.Identity?.Name ?? "System";
                }

                var result = await _serviceUnitOfWork.SupplierPaymentService
                    .SaveUpdateSupplierPayment(payment);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(Radzen.NotificationSeverity.Success,
                        "Success", "Payment saved successfully");
                    NavigationManager.NavigateTo("/SupplierPaymentsList");
                }
                else
                {
                    notificationService.Notify(Radzen.NotificationSeverity.Error,
                        "Payment not saved", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(Radzen.NotificationSeverity.Error,
                    "Error", $"Failed to save payment: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/SupplierPaymentsList");
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                return int.TryParse(result.Value, out var value) ? value : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
