using JM.UI.Entities.Model.Stores;
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
    public partial class BranchLoanComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;

        protected int CurrentUserId { get; set; }
        protected int CurrentStoreId { get; set; }

        protected List<BranchLoanRowDTO> Loans { get; set; } = new();
        protected List<BranchLoanAuditRowDTO> Audit { get; set; } = new();
        protected List<StoreDTO> Stores { get; set; } = new();

        protected int SenderStoreId { get; set; }
        protected int ReceiverStoreId { get; set; }
        protected decimal Amount { get; set; }
        protected string Remarks { get; set; } = string.Empty;
        protected decimal SenderBalance { get; set; }

        // repay drawer
        protected BranchLoanRowDTO? RepayTarget { get; set; }
        protected string ReceivingLedgerCode { get; set; } = "111001";
        protected List<TreasuryLedgerDTO> RepaySourceAccounts { get; set; } = new();
        protected List<BranchLoanRepaySplitDTO> RepaySplits { get; set; } = new() { new() { SourceLedgerCode = "111001", Amount = 0 } };
        protected bool ShowRepay { get; set; }

        protected bool IsSending { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            if (CurrentStoreId <= 0) CurrentStoreId = 1;
            SenderStoreId = CurrentStoreId;
            await LoadStores();
            await LoadSenderBalance();
            await Load();
        }

        protected async Task OnSenderStoreChanged(object value)
        {
            await LoadSenderBalance();
        }

        private async Task LoadSenderBalance()
        {
            try
            {
                var cash = await ServiceUnitOfWork.DailyExpenseService.GetAvailableCash(SenderStoreId, 0, isPettyCash: false);
                SenderBalance = cash?.AvailableCash ?? 0m;
            }
            catch
            {
                SenderBalance = 0m;
            }
            StateHasChanged();
        }

        private async Task LoadStores()
        {
            try
            {
                var stores = await ServiceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
                Stores = stores
                    .Where(s => !string.Equals(s.Name?.Trim(), "Damaged", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        private async Task LoadRepaySourceAccounts()
        {
            try
            {
                // Cash + Petty Cash + bank / MFS ledgers, with their live balances.
                RepaySourceAccounts = (await ServiceUnitOfWork.TreasuryService
                        .GetContraAccountsAsync(CurrentStoreId, "HO", DateTime.Today))?
                    .ToList() ?? new List<TreasuryLedgerDTO>();
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
                Loans = (await ServiceUnitOfWork.TreasuryService.ListBranchLoansAsync(CurrentStoreId))?.ToList() ?? new();
                Audit = (await ServiceUnitOfWork.TreasuryService.ListBranchLoanAuditAsync())?.ToList() ?? new();
                StateHasChanged();
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected async Task Send()
        {
            if (SenderStoreId == ReceiverStoreId || Amount <= 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Check sender, receiver and amount.");
                return;
            }
            IsSending = true;
            try
            {
                var req = new BranchLoanSendRequestDTO
                {
                    SenderStoreId = SenderStoreId,
                    ReceiverStoreId = ReceiverStoreId,
                    UserId = CurrentUserId,
                    Amount = Amount,
                    Remarks = Remarks,
                    EntryDate = DateTime.Now
                };
                var res = await ServiceUnitOfWork.TreasuryService.SendBranchLoanAsync(req);
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Sent", res.Message); Amount = 0; Remarks = string.Empty; await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
            finally { IsSending = false; StateHasChanged(); }
        }

        protected async Task Accept(BranchLoanRowDTO row)
        {
            try
            {
                var res = await ServiceUnitOfWork.TreasuryService.AcceptBranchLoanAsync(new BranchLoanAcceptRequestDTO { LoanId = row.LoanId, UserId = CurrentUserId, EntryDate = DateTime.Now });
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Accepted", res.Message); await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected async Task OpenRepay(BranchLoanRowDTO row)
        {
            RepayTarget = row;
            RepaySplits = new List<BranchLoanRepaySplitDTO> { new() { SourceLedgerCode = "111001", Amount = row.RemainingAmount } };
            ShowRepay = true;
            await LoadRepaySourceAccounts();
        }

        protected void CloseRepay() { ShowRepay = false; RepayTarget = null; }

        protected void AddRepaySplit()
        {
            RepaySplits.Add(new BranchLoanRepaySplitDTO { SourceLedgerCode = "111001", Amount = 0 });
        }

        protected void RemoveRepaySplit(int idx)
        {
            if (RepaySplits.Count > 1) RepaySplits.RemoveAt(idx);
        }

        protected decimal RepayTotal => RepaySplits.Sum(s => s.Amount);

        protected async Task SubmitRepay()
        {
            if (RepayTarget == null) return;
            try
            {
                var req = new BranchLoanRepayRequestDTO
                {
                    LoanId = RepayTarget.LoanId,
                    UserId = CurrentUserId,
                    ReceivingLedgerCode = ReceivingLedgerCode,
                    Splits = RepaySplits,
                    EntryDate = DateTime.Now
                };
                var res = await ServiceUnitOfWork.TreasuryService.RepayBranchLoanAsync(req);
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Repaid", res.Message); CloseRepay(); await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected async Task Reverse(BranchLoanRowDTO row)
        {
            try
            {
                var res = await ServiceUnitOfWork.TreasuryService.ReverseBranchLoanAsync(row.LoanId, CurrentUserId);
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Reversed", res.Message); await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }
    }
}
