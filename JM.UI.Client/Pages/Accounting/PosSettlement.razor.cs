using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Entities.Model.FinancialAccounts;
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
    public partial class PosSettlementComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;

        protected int CurrentUserId { get; set; }
        protected int CurrentStoreId { get; set; }

        protected List<SettlementPendingRowDTO> Pending { get; set; } = new();
        protected List<SettlementHistoryRowDTO> History { get; set; } = new();

        protected List<FinancialAccountDropdownDTO> BankAccounts { get; set; } = new();
        protected List<ChartOfAccountDTO> CommissionLedgers { get; set; } = new();

        protected SettlementPendingRowDTO? Active { get; set; }
        protected int ReceivingFinancialAccountId { get; set; }
        protected string CommissionLedgerCode { get; set; } = "550002";
        protected decimal TransferAmount { get; set; }
        protected decimal CommissionAmount { get; set; }
        protected decimal SettlementAmount { get; set; }
        protected bool ShowModal { get; set; }
        protected bool IsSaving { get; set; }

        protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);
        protected DateTime ToDate { get; set; } = DateTime.Today;

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            if (CurrentStoreId <= 0) CurrentStoreId = 1;
            await LoadLookups();
            await Load();
        }

        private async Task LoadLookups()
        {
            try
            {
                var fas = await ServiceUnitOfWork.FinancialAccountsService.GetFinancialAccountsForDropdown(null)
                    ?? new List<FinancialAccountDropdownDTO>();
                // Receiving account = bank / card-POS clearing accounts (type 2).
                BankAccounts = fas.Where(x => x.FinancialAccountTypeId == 2).ToList();

                var expenses = await ServiceUnitOfWork.ChartOfAccountService.GetExpenseAccounts()
                    ?? new List<ChartOfAccountDTO>();
                // Commission ledgers live under the Financial Charges group (55xxxx).
                CommissionLedgers = expenses
                    .Where(x => x.Code.StartsWith("55", StringComparison.Ordinal))
                    .ToList();
                if (CommissionLedgers.Count == 0)
                    CommissionLedgers = expenses.ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try { var r = await _localStorage.GetAsync<string>(key); return int.TryParse(r.Value, out var v) ? v : 0; }
            catch { return 0; }
        }

        protected async Task Load()
        {
            try
            {
                Pending = (await ServiceUnitOfWork.TreasuryService.GetSettlementPendingAsync(CurrentStoreId, FromDate, ToDate))?.ToList() ?? new();
                History = (await ServiceUnitOfWork.TreasuryService.ListSettlementAsync(CurrentStoreId, FromDate, ToDate))?.ToList() ?? new();
                StateHasChanged();
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected void OpenModal(SettlementPendingRowDTO row)
        {
            Active = row;
            SettlementAmount = row.Amount;
            TransferAmount = row.Amount;
            CommissionAmount = 0;
            ReceivingFinancialAccountId = BankAccounts.FirstOrDefault()?.Id ?? 0;
            ShowModal = true;
        }

        protected void CloseModal() { ShowModal = false; Active = null; }

        protected void OnTransferInput()
        {
            CommissionAmount = Math.Round(SettlementAmount - TransferAmount, 2);
        }

        protected void OnCommissionInput()
        {
            TransferAmount = Math.Round(SettlementAmount - CommissionAmount, 2);
        }

        protected bool IsValid => Math.Abs(TransferAmount + CommissionAmount - SettlementAmount) < 0.005m
            && TransferAmount >= 0 && CommissionAmount >= 0 && SettlementAmount > 0;

        protected async Task Confirm()
        {
            if (Active == null || !IsValid) return;
            IsSaving = true;
            try
            {
                var req = new SettlementTransferRequestDTO
                {
                    PaymentTransactionId = Active.PaymentTransactionId,
                    StoreId = Active.StoreId,
                    UserId = CurrentUserId,
                    ReceivingFinancialAccountId = ReceivingFinancialAccountId,
                    CommissionLedgerCode = CommissionLedgerCode,
                    TransferAmount = TransferAmount,
                    CommissionAmount = CommissionAmount,
                    EntryDate = DateTime.Now
                };
                var res = await ServiceUnitOfWork.TreasuryService.PostSettlementAsync(req);
                if (res.Success)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Settled", res.Message);
                    CloseModal();
                    await Load();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
                }
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
            finally { IsSaving = false; StateHasChanged(); }
        }
    }
}
