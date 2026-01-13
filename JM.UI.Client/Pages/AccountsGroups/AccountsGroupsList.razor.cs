using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Bank;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.AccountsGroups;

public partial class AccountsGroupsListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<AccountsGroupsDTO> AccountsGroupsGrid = default!;
    protected IEnumerable<AccountsGroupsDTO> AccountsGroups { get; set; } = new List<AccountsGroupsDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadAccountsGroupss();
    }

    private async Task LoadAccountsGroupss()
    {
        IsLoading = true;
        try
        {
            AccountsGroups = await _serviceUnitOfWork.AccountsGroupsService.GetAccountsGroups();
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

    protected void AddAccountsGroups() => NavigationManager.NavigateTo("/AccountsGroupsAdd");

    protected void EditAccountsGroups(AccountsGroupsDTO d)
        => NavigationManager.NavigateTo($"/AccountsGroupsAdd/{d.Id}");

    protected async Task DeleteAccountsGroups(AccountsGroupsDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete AccountsGroups '{d.Name}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.AccountsGroupsService.DeleteAccountsGroups(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadAccountsGroupss();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.AccountsGroupsService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => AccountsGroupsGrid?.Dispose();
}