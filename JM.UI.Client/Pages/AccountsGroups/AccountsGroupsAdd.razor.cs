using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.AccountsGroups
{
    public partial class AccountsGroupsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected AccountsGroupsModelDTO AccountsGroup { get; set; } = new();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Accounts Group" : "Add Accounts Group";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadStores();

            if (IsEditMode)
            {
                await LoadAccountsGroup();
            }
        }

        private async Task LoadStores()
        {
            try
            {
                Stores = await _serviceUnitOfWork.StoreService.GetStores();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stores: {ex.Message}");
            }
        }

        private async Task LoadAccountsGroup()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.AccountsGroupsService.GetAccountsGroupsById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Accounts Group not found.");
                    NavigationManager.NavigateTo("/AccountsGroupsList");
                    return;
                }

                AccountsGroup = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load accounts group: {ex.Message}");
                NavigationManager.NavigateTo("/AccountsGroupsList");
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
                var result = await _serviceUnitOfWork.AccountsGroupsService.SaveUpdateAccountsGroups(AccountsGroup);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Accounts Group updated successfully!" : "Accounts Group saved successfully!");
                    NavigationManager.NavigateTo("/AccountsGroupsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save accounts group: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/AccountsGroupsList");
        }
    }
}
