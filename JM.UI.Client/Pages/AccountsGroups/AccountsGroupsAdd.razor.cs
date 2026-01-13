using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.AccountsGroups;

public partial class AccountsGroupsAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? AccountsGroupsID { get; set; }

    protected AccountsGroupsDTO AccountsGroups { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;


    protected List<StoreDTO> Stores { get; set; } = new();
    protected bool IsEditMode => AccountsGroupsID.HasValue && AccountsGroupsID.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit AccountsGroups" : "Add New AccountsGroups";
    protected string PageIcon => IsEditMode ? "edit" : "work";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDropdowns();

        if (IsEditMode)
            await LoadAccountsGroups();
        //else
        //    InitializeAccountsGroups();
    }

    private async Task LoadAccountsGroups()
    {
        try
        {
            IsLoading = true;

            var data = await _serviceUnitOfWork.AccountsGroupsService
                .GetAccountsGroupsById(AccountsGroupsID!.Value);

            if (data == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "AccountsGroups not found.");
                NavigationManager.NavigateTo("/AccountsGroupsList");
                return;
            }

            AccountsGroups = data;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load AccountsGroups: {ex.Message}");
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
            var result = await _serviceUnitOfWork.AccountsGroupsService.SaveUpdateAccountsGroups(AccountsGroups);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "AccountsGroups updated successfully!" : "AccountsGroups created successfully!");
                NavigationManager.NavigateTo("/AccountsGroupsList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save AccountsGroups: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.AccountsGroupsService.ValidateAccountsGroups(AccountsGroups);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.AccountsGroupsService.SaveUpdateAccountsGroups(AccountsGroups);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "AccountsGroups created successfully!");
                //InitializeAccountsGroups();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save AccountsGroups: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/AccountsGroupsList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadAccountsGroups();
        //else InitializeAccountsGroups();
        StateHasChanged();
    }
}