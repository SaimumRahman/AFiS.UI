using JM.Infrastructure.Models;
using JM.UI.Entities.Model.DailyExpense;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.DailyExpense
{
    public partial class PostLedgerComponent : PosComponentBase
    {
        /// <summary>The retail business day runs from 06:00 to 06:00 the next morning.</summary>
        private const int BusinessDayCutOffHour = 6;

        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public DailyExpensePostReportService DailyExpensePostReportService { get; set; } = default!;

        protected RadzenDataGrid<DailyExpenseDTO> ExpensesGrid = default!;
        protected List<StoreDTO> Stores { get; set; } = new();
        protected List<DailyExpenseDTO> PostableExpenses { get; set; } = new();
        protected List<DailyExpenseDTO> PostedToday { get; set; } = new();
        protected HashSet<int> SelectedIds { get; set; } = new();

        protected DateTime BusinessDay { get; set; } = CurrentBusinessDay();
        protected int? SelectedStoreId { get; set; }
        protected int UserId { get; set; }
        protected bool IsLoading { get; set; }
        protected bool IsPosting { get; set; }

        protected decimal SelectedTotal => PostableExpenses.Where(x => SelectedIds.Contains(x.DailyExpenseId)).Sum(x => x.Amount);

        // Inclusive start / exclusive end of the business day window.
        protected DateTime BusinessWindowStart => BusinessDay.Date.AddHours(BusinessDayCutOffHour);
        protected DateTime BusinessWindowEnd => BusinessDay.Date.AddDays(1).AddHours(BusinessDayCutOffHour);

        /// <summary>
        /// Anything entered before 06:00 belongs to the previous business day.
        /// </summary>
        private static DateTime CurrentBusinessDay()
        {
            DateTime now = DateTime.Now;
            return now.TimeOfDay < TimeSpan.FromHours(BusinessDayCutOffHour)
                ? now.Date.AddDays(-1)
                : now.Date;
        }

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            UserId = await GetLocalStorageInt("UserId");
            await LoadStores();
            await LoadExpenses();
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && !string.IsNullOrEmpty(result.Value)
                    && int.TryParse(result.Value, out int parsed) && parsed > 0)
                {
                    return parsed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
            }

            return 0;
        }

        private async Task LoadStores()
        {
            try
            {
                Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new List<StoreDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        protected async Task LoadExpenses()
        {
            IsLoading = true;
            try
            {
                // One business-day window (06:00 to 06:00) returns every expense of
                // that day; the page splits it into postable and already posted.
                var dayExpenses = await _serviceUnitOfWork.DailyExpenseService
                    .GetDailyExpensesForPosting(BusinessDay, SelectedStoreId);

                var all = dayExpenses?.ToList() ?? new List<DailyExpenseDTO>();
                PostableExpenses = all.Where(x => !x.IsPosted).ToList();
                PostedToday = all.Where(x => x.IsPosted).ToList();

                // Drop any selection that no longer exists after a reload.
                var available = PostableExpenses.Select(x => x.DailyExpenseId).ToHashSet();
                SelectedIds.RemoveWhere(id => !available.Contains(id));
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected bool IsSelected(int id) => SelectedIds.Contains(id);

        protected void ToggleRow(int id, bool selected)
        {
            if (selected)
                SelectedIds.Add(id);
            else
                SelectedIds.Remove(id);
        }

        protected void SelectAll()
        {
            foreach (var id in PostableExpenses.Select(x => x.DailyExpenseId))
                SelectedIds.Add(id);
        }

        protected void ClearSelection() => SelectedIds.Clear();

        protected async Task PostSelected()
        {
            if (SelectedIds.Count == 0)
                return;

            var confirm = await dialogService.Confirm(
                $"Post {SelectedIds.Count} expense(s) totalling {SelectedTotal:N2} to the general ledger for business day {BusinessDay:dd MMM yyyy}?",
                "Confirm Post",
                new ConfirmOptions { OkButtonText = "Yes, Post", CancelButtonText = "Cancel" });

            if (confirm != true)
                return;

            IsPosting = true;
            try
            {
                var postedIds = SelectedIds.ToList();

                var result = await _serviceUnitOfWork.DailyExpenseService.PostDailyExpenses(new PostDailyExpensesDTO
                {
                    Ids = postedIds,
                    PostedBy = UserId
                });

                notificationService.Notify(
                    result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Posted" : "Post Failed",
                    result.Message,
                    5000);

                if (result.IsSuccessStatus)
                {
                    SelectedIds.Clear();
                    await LoadExpenses();

                    // Download a PDF voucher of the expenses that were just posted.
                    await DownloadPostReport(postedIds);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Post Failed", ex.Message, 5000);
            }
            finally
            {
                IsPosting = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Builds and downloads a PDF voucher for the expenses that were just
        /// posted. Uses the reloaded "Posted Today" rows so voucher numbers and
        /// posted-by names are included.
        /// </summary>
        protected async Task DownloadPostReport(List<int> postedIds)
        {
            try
            {
                var expenses = PostedToday
                    .Where(x => postedIds.Contains(x.DailyExpenseId))
                    .OrderBy(x => x.StoreName)
                    .ThenBy(x => x.DailyExpenseId)
                    .ToList();

                if (expenses.Count == 0)
                    return;

                StoreDTO? store = SelectedStoreId.HasValue
                    ? Stores.FirstOrDefault(s => s.Id == SelectedStoreId.Value)
                    : null;

                var pdfBytes = DailyExpensePostReportService.GenerateDailyExpensePostReport(
                    expenses,
                    businessDay: BusinessDay,
                    businessWindowStart: BusinessWindowStart,
                    businessWindowEnd: BusinessWindowEnd,
                    storeName: string.IsNullOrWhiteSpace(store?.Name) ? "ASIA FASHION" : store!.Name,
                    storeAddress: store?.Address,
                    postedByName: expenses.FirstOrDefault()?.PostedByName);

                var fileName = $"DailyExpensePosting_{BusinessDay:yyyyMMdd}.pdf";
                await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Report Failed", ex.Message, 5000);
            }
        }

        protected async Task Unpost(DailyExpenseDTO expense)
        {
            var confirm = await dialogService.Confirm(
                $"Unpost '{expense.Description}' of {expense.Amount:N2}? The ledger voucher will be reversed and the expense becomes editable again.",
                "Confirm Unpost",
                new ConfirmOptions { OkButtonText = "Yes, Unpost", CancelButtonText = "Cancel" });

            if (confirm != true)
                return;

            var result = await _serviceUnitOfWork.DailyExpenseService.UnpostDailyExpense(new UnpostDailyExpenseDTO
            {
                Id = expense.DailyExpenseId,
                UnpostedBy = UserId
            });

            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Unposted" : "Unpost Failed",
                result.Message,
                5000);

            if (result.IsSuccessStatus)
                await LoadExpenses();
        }

        public void Dispose() => ExpensesGrid?.Dispose();
    }
}
