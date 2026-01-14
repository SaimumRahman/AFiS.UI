using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.AccountsGroups
{
    public partial class AccountsGroupsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<AccountsGroupsModelDTO> GroupsGrid = default!;
        protected IEnumerable<AccountsGroupsModelDTO> GroupsList = new List<AccountsGroupsModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadGroups();
        }

        protected async Task LoadGroups()
        {
            try
            {
                IsLoading = true;
                GroupsList = await _serviceUnitOfWork.AccountsGroupsService.GetAccountsGroups();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load accounts groups: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddGroup()
        {
            NavigationManager.NavigateTo("/AccountsGroupsAdd");
        }

        protected void EditGroup(AccountsGroupsModelDTO group)
        {
            NavigationManager.NavigateTo($"/AccountsGroupsAdd/{group.Id}");
        }

        protected async Task DeleteGroup(AccountsGroupsModelDTO group)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Accounts Group '{group.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.AccountsGroupsService.DeleteAccountsGroups(group.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadGroups();
            }
        }

        public void Dispose()
        {
            GroupsGrid?.Dispose();
        }
    }
}
