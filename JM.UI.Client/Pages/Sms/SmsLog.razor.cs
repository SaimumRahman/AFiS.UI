using JM.UI.Client.Pages.Dialog.Sms;
using JM.UI.Entities.Model.Sms;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Client.Pages.Sms
{
    public partial class SmsLogComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService dialogService { get; set; } = default!;

        protected SmsLogPageResult Result = new();
        protected IEnumerable<StoreDTO> StoreList = new List<StoreDTO>();
        protected List<int> SelectedStoreIds = new();
        protected DateTime? FromDate;
        protected DateTime? ToDate;
        protected string SearchText = string.Empty;
        protected bool IsLoading;
        protected bool CanSelectAnyStore;
        protected int CurrentUserId;
        protected int CurrentStoreId;
        protected int Page = 1;
        protected int PageSize = 25;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            CurrentUserId = await GetLocalStorageInt("UserId");
            CurrentStoreId = await GetLocalStorageInt("StoreId");
            await ResolveStoreSelectionPermission();
            await LoadStores();
            ToDate = DateTime.Today;
            FromDate = DateTime.Today.AddDays(-7);
            await LoadLog();
        }

        private async Task ResolveStoreSelectionPermission()
        {
            CanSelectAnyStore = CurrentUserId == 1;
            if (CanSelectAnyStore) return;
            try
            {
                CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                    .GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch { CanSelectAnyStore = false; }
        }

        private async Task LoadStores()
        {
            try
            {
                var stores = await _serviceUnitOfWork.StoreService.GetStores();
                StoreList = stores ?? new List<StoreDTO>();
                if (!CanSelectAnyStore && CurrentStoreId > 0)
                {
                    SelectedStoreIds = new List<int> { CurrentStoreId };
                }
                else
                {
                    SelectedStoreIds = StoreList.Select(s => s.Id).ToList();
                }
            }
            catch { StoreList = new List<StoreDTO>(); }
        }

        protected async Task LoadLog()
        {
            IsLoading = true;
            try
            {
                var storeIds = CanSelectAnyStore
                    ? string.Join(",", SelectedStoreIds)
                    : (CurrentStoreId > 0 ? CurrentStoreId.ToString() : string.Empty);
                Result = await _serviceUnitOfWork.SmsLogService.GetAsync(FromDate, ToDate, storeIds, SearchText, Page, PageSize);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load SMS log: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task ApplyFilters()
        {
            Page = 1;
            await LoadLog();
        }

        protected async Task ClearFilters()
        {
            FromDate = DateTime.Today.AddDays(-7);
            ToDate = DateTime.Today;
            SearchText = string.Empty;
            if (CanSelectAnyStore) SelectedStoreIds = StoreList.Select(s => s.Id).ToList();
            else if (CurrentStoreId > 0) SelectedStoreIds = new List<int> { CurrentStoreId };
            Page = 1;
            await LoadLog();
        }

        protected async Task OpenDetail(SmsLogDTO row)
        {
            await dialogService.OpenAsync<SmsLogDetailDialog>("SMS Details",
                new Dictionary<string, object> { { "Log", row } },
                new DialogOptions { Width = "480px", Height = "auto" });
        }

        protected string StatusBadgeStyle(string status) =>
            (status ?? string.Empty).Equals("Delivered", StringComparison.OrdinalIgnoreCase)
                ? "success" : "danger";

        protected async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var r = await _localStorage.GetAsync<string>(key);
                if (r.Success && int.TryParse(r.Value, out var v)) return v;
            }
            catch { }
            return 0;
        }

        public void Dispose() { }
    }
}
