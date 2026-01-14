using JM.UI.Entities.Model.Accounts;
using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounts
{
    public partial class AccountsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected AccountModelDTO Account { get; set; } = new();
        protected IEnumerable<AccountsGroupsDTO> AccountsGroups { get; set; } = new List<AccountsGroupsDTO>();
        protected IEnumerable<AccountModelDTO> ParentAccounts { get; set; } = new List<AccountModelDTO>();
        
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Account" : "Add Account";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadAccount();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var groupsTask = _serviceUnitOfWork.AccountsGroupsService.GetAccountsGroups();
                var accountsTask = _serviceUnitOfWork.AccountsService.GetAccounts();

                await Task.WhenAll(groupsTask, accountsTask);

                AccountsGroups = await groupsTask;
                ParentAccounts = await accountsTask;
                
                // If editing, filter out self from parent selection to avoid circular reference
                if (IsEditMode)
                {
                    ParentAccounts = ParentAccounts.Where(a => a.Id != Id).ToList();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAccount()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.AccountsService.GetAccountById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Account not found.");
                    NavigationManager.NavigateTo("/AccountsList");
                    return;
                }

                Account = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load account: {ex.Message}");
                NavigationManager.NavigateTo("/AccountsList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                
                // Basic audit info if new
                if (!IsEditMode)
                {
                    Account.CreatedBy = "Admin"; // Should be current user
                    Account.CreatedOn = DateTime.Now;
                }
                else
                {
                    Account.ModifiedBy = "Admin";
                    Account.ModifiedOn = DateTime.Now;
                }

                var result = await _serviceUnitOfWork.AccountsService.SaveUpdateAccount(Account);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Account updated successfully!" : "Account saved successfully!");
                    NavigationManager.NavigateTo("/AccountsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save account: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/AccountsList");
        }
    }
}
