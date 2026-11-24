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
    public partial class CashSwapComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;

        protected int CurrentUserId { get; set; }
        protected int CurrentStoreId { get; set; }

        protected List<CashSwapRowDTO> Swaps { get; set; } = new();
        protected List<StoreDTO> Stores { get; set; } = new();

        protected int FromStoreId { get; set; }
        protected int ToStoreId { get; set; }
        protected decimal Amount { get; set; }
        protected string Remarks { get; set; } = string.Empty;
        protected decimal FromBalance { get; set; }

        protected bool IsSending { get; set; }
        protected string? Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            if (CurrentStoreId <= 0) CurrentStoreId = 1;
            FromStoreId = CurrentStoreId;
            await LoadStores();
            await LoadFromBalance();
            await Load();
        }

        protected async Task OnFromStoreChanged(object value)
        {
            await LoadFromBalance();
        }

        private async Task LoadFromBalance()
        {
            try
            {
                var cash = await ServiceUnitOfWork.DailyExpenseService.GetAvailableCash(FromStoreId, 0, isPettyCash: false);
                FromBalance = cash?.AvailableCash ?? 0m;
            }
            catch
            {
                FromBalance = 0m;
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

        private async Task<int> GetLocalStorageInt(string key)
        {
            try { var r = await _localStorage.GetAsync<string>(key); return int.TryParse(r.Value, out var v) ? v : 0; }
            catch { return 0; }
        }

        protected async Task Load()
        {
            try
            {
                Swaps = (await ServiceUnitOfWork.TreasuryService.ListCashSwapsAsync(CurrentStoreId))?.ToList() ?? new();
                StateHasChanged();
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected async Task Send()
        {
            if (FromStoreId == ToStoreId || Amount <= 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Select different stores and a valid amount.");
                return;
            }
            IsSending = true;
            try
            {
                var req = new CashSwapSendRequestDTO
                {
                    FromStoreId = FromStoreId,
                    ToStoreId = ToStoreId,
                    UserId = CurrentUserId,
                    Amount = Amount,
                    Remarks = Remarks,
                    EntryDate = DateTime.Now
                };
                var res = await ServiceUnitOfWork.TreasuryService.SendCashSwapAsync(req);
                if (res.Success)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Sent", res.Message);
                    Amount = 0; Remarks = string.Empty;
                    await Load();
                }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
            finally { IsSending = false; StateHasChanged(); }
        }

        protected async Task Receive(CashSwapRowDTO row)
        {
            try
            {
                var res = await ServiceUnitOfWork.TreasuryService.ReceiveCashSwapAsync(new CashSwapActionRequestDTO { SwapId = row.SwapId, UserId = CurrentUserId });
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Received", res.Message); await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }

        protected async Task Cancel(CashSwapRowDTO row)
        {
            try
            {
                var res = await ServiceUnitOfWork.TreasuryService.CancelCashSwapAsync(new CashSwapActionRequestDTO { SwapId = row.SwapId, UserId = CurrentUserId });
                if (res.Success) { notificationService.Notify(NotificationSeverity.Success, "Cancelled", res.Message); await Load(); }
                else notificationService.Notify(NotificationSeverity.Error, "Failed", res.Message);
            }
            catch (Exception ex) { notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message); }
        }
    }
}
