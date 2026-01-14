using JM.UI.Entities.Model.Accounts;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounts
{
    public partial class AccountsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<AccountModelDTO> AccountsGrid = default!;
        protected IEnumerable<AccountModelDTO> AccountsList = new List<AccountModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadAccounts();
        }

        protected async Task LoadAccounts()
        {
            try
            {
                IsLoading = true;
                AccountsList = await _serviceUnitOfWork.AccountsService.GetAccounts();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load accounts: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddAccount()
        {
            NavigationManager.NavigateTo("/AccountsAdd");
        }

        protected void EditAccount(AccountModelDTO account)
        {
            NavigationManager.NavigateTo($"/AccountsAdd/{account.Id}");
        }

        protected async Task DeleteAccount(AccountModelDTO account)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Account '{account.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.AccountsService.DeleteAccount(account.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadAccounts();
            }
        }

        public void Dispose()
        {
            AccountsGrid?.Dispose();
        }
    }
}
