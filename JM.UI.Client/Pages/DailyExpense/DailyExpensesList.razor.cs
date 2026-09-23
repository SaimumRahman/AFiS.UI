using JM.Infrastructure.Models;
using JM.UI.Client.Pages.Dialog.DailyExpense;
using JM.UI.Entities.Model.DailyExpense;
using JM.UI.Entities.Model.FinancialAccounts;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.DailyExpense
{
    public partial class DailyExpensesListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<DailyExpenseDTO> ExpensesGrid = default!;
        protected List<DailyExpenseDTO> Expenses { get; set; } = new();
        protected List<StoreDTO> Stores { get; set; } = new();
        protected List<FinancialAccountDropdownDTO> FinancialAccounts { get; set; } = new();
        protected DateTime FromDate { get; set; } = DateTime.Today;
        protected DateTime ToDate { get; set; } = DateTime.Today;
        protected int? SelectedStoreId { get; set; }
        protected int UserId { get; set; }
        protected bool IsLoading { get; set; }

        protected decimal TotalAmount => Expenses.Sum(x => x.Amount);

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            UserId = await GetLocalStorageInt("UserId");
            await LoadLookups();
            await LoadExpenses();
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

        private async Task LoadLookups()
        {
            try
            {
                Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new List<StoreDTO>();
                FinancialAccounts = (await _serviceUnitOfWork.FinancialAccountsService.GetFinancialAccountsForDropdown())?.ToList() ?? new List<FinancialAccountDropdownDTO>();
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
                Expenses = (await _serviceUnitOfWork.DailyExpenseService.GetDailyExpenses(FromDate, ToDate, SelectedStoreId))?.ToList() ?? new List<DailyExpenseDTO>();
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

        protected async Task AddExpense()
        {
            int localStoreId = await GetLocalStorageInt("StoreId");
            int defaultStore = localStoreId > 0 ? localStoreId : (Stores.FirstOrDefault()?.Id ?? 0);

            var parameters = new Dictionary<string, object>
            {
                { "StoreId", defaultStore },
                { "UserId", UserId }
            };

            var result = await dialogService.OpenAsync<ExpenseEntryDialog>("Expense Entry", parameters);
            if (result is ResponseResult res && res.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", res.Message);
                await LoadExpenses();
            }
        }

        protected async Task EditExpense(DailyExpenseDTO expense)
        {
            var parameters = new Dictionary<string, object>
            {
                { "StoreId", expense.StoreId },
                { "UserId", UserId },
                { "EditId", expense.DailyExpenseId }
            };

            var result = await dialogService.OpenAsync<ExpenseEntryDialog>("Edit Expense", parameters);
            if (result is ResponseResult res && res.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", res.Message);
                await LoadExpenses();
            }
        }

        protected async Task DeleteExpense(DailyExpenseDTO expense)
        {
            var confirm = await dialogService.Confirm(
                $"Delete expense '{expense.Description}' of {expense.Amount:N2}?",
                "Confirm Delete",
                new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel"
                });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.DailyExpenseService.DeleteDailyExpense(expense.DailyExpenseId, UserId);
                notificationService.Notify(
                    result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error",
                    result.Message);

                if (result.IsSuccessStatus) await LoadExpenses();
            }
        }

        public void Dispose() => ExpensesGrid?.Dispose();
    }
}