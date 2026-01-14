using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Model.Bank;
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
    [Parameter] public int? AccountsGroupsID { get; set; }

        [Parameter] public int? Id { get; set; }

        protected AccountsGroupsModelDTO AccountsGroup { get; set; } = new();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Accounts Group" : "Add Accounts Group";


    protected List<StoreDTO> Stores { get; set; } = new();
    protected bool IsEditMode => AccountsGroupsID.HasValue && AccountsGroupsID.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit AccountsGroups" : "Add New AccountsGroups";
    protected string PageIcon => IsEditMode ? "edit" : "work";

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

    private async Task LoadDropdowns()
    {
        try
        {
            // Load Stores
            var storesTask = _serviceUnitOfWork.StoreService.GetStores();
           

            await Task.WhenAll( storesTask);

            Stores = (await storesTask).ToList();
           
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load dropdown data: {ex.Message}");
        }
    }

    //private void InitializeAccountsGroups()
    //{
    //    AccountsGroups = _serviceUnitOfWork.AccountsGroupsService.CreateNewAccountsGroups();
    //}

        protected async Task Save()
        {
        var validation = await _serviceUnitOfWork.AccountsGroupsService.ValidateAccountsGroups(AccountsGroups);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

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
        finally { IsProcessing = false; }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/AccountsGroupsList");
        }
    }
}
