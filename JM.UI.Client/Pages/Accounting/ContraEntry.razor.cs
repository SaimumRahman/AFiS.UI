using JM.UI.Entities.Model.Treasury;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounting
{
    public partial class ContraEntryComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;

        protected int CurrentUserId { get; set; }
        protected int CurrentStoreId { get; set; }
        protected string BranchCode { get; set; } = "HO";

        protected string ContraType { get; set; } = "CASH_TO_BANK";
        protected string? FromLedgerCode { get; set; }
        protected string? ToLedgerCode { get; set; }
        protected decimal Amount { get; set; }
        protected string Remarks { get; set; } = string.Empty;

        protected List<TreasuryLedgerDTO> Accounts { get; set; } = new();
        protected List<ContraListRowDTO> Recent { get; set; } = new();

        protected List<TreasuryLedgerDTO> CashAccounts => Accounts
            .Where(a => a.LedgerCode.StartsWith("1110", StringComparison.Ordinal)).ToList();

        protected List<TreasuryLedgerDTO> BankAccounts => Accounts
            .Where(a => !a.LedgerCode.StartsWith("1110", StringComparison.Ordinal)).ToList();

        protected bool IsPosting { get; set; }
        protected string? Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            if (CurrentStoreId <= 0) CurrentStoreId = 1;

            await LoadAccounts();
            await LoadRecent();
            OnContraTypeChanged(ContraType);
        }

        protected void OnContraTypeChanged(object value)
        {
            ContraType = value?.ToString() ?? "CASH_TO_BANK";
            if (ContraType == "CASH_TO_BANK")
            {
                FromLedgerCode = CashAccounts.FirstOrDefault(x => x.LedgerCode == "111001")?.LedgerCode ?? CashAccounts.FirstOrDefault()?.LedgerCode;
                ToLedgerCode = null;
            }
            else if (ContraType == "BANK_TO_CASH")
            {
                FromLedgerCode = null;
                ToLedgerCode = CashAccounts.FirstOrDefault(x => x.LedgerCode == "111001")?.LedgerCode ?? CashAccounts.FirstOrDefault()?.LedgerCode;
            }
            else
            {
                FromLedgerCode = null;
                ToLedgerCode = null;
            }
        }

        protected decimal BalanceOf(string? code)
            => Accounts.FirstOrDefault(a => a.LedgerCode == code)?.Balance ?? 0m;

        public sealed record ContraTypeOption(string Value, string Label);

        protected List<ContraTypeOption> ContraTypeOptions { get; } = new()
        {
            new("CASH_TO_BANK", "Cash to Bank"),
            new("BANK_TO_CASH", "Bank to Cash"),
            new("BANK_TO_BANK", "Bank to Bank"),
        };

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

        protected async Task LoadAccounts()
        {
            try
            {
                Accounts = (await ServiceUnitOfWork.TreasuryService.GetContraAccountsAsync(CurrentStoreId, BranchCode, DateTime.Today))?.ToList() ?? new();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        protected async Task LoadRecent()
        {
            try
            {
                Recent = (await ServiceUnitOfWork.TreasuryService.ListContraAsync(BranchCode, DateTime.Today.AddDays(-7), DateTime.Today))?.ToList() ?? new();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        protected async Task Post()
        {
            if (Amount <= 0 || string.IsNullOrWhiteSpace(FromLedgerCode) || string.IsNullOrWhiteSpace(ToLedgerCode))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Fill all required fields.");
                return;
            }
            if (FromLedgerCode == ToLedgerCode)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Source and target must differ.");
                return;
            }
            IsPosting = true;
            Message = null;
            try
            {
                var req = new ContraPostRequestDTO
                {
                    StoreId = CurrentStoreId,
                    UserId = CurrentUserId,
                    ContraType = ContraType,
                    FromLedgerCode = FromLedgerCode,
                    ToLedgerCode = ToLedgerCode,
                    Amount = Amount,
                    Remarks = Remarks,
                    EntryDate = DateTime.Now
                };
                var res = await ServiceUnitOfWork.TreasuryService.PostContraAsync(req);
                if (res.Success)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Posted", res.Message);
                    Amount = 0;
                    Remarks = string.Empty;
                    await LoadAccounts();
                    await LoadRecent();
                }
                else
                {
                    Message = res.Message;
                    notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally
            {
                IsPosting = false;
                StateHasChanged();
            }
        }
    }
}
