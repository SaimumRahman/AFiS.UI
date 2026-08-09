using JM.UI.Entities.Model.FinancialAccounts;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.FinancialAccounts;

public partial class FinancialAccountsListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<FinancialAccountDTO> FinancialAccountsGrid = default!;
    protected IEnumerable<FinancialAccountDTO> FinancialAccounts { get; set; } = new List<FinancialAccountDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadFinancialAccounts();
    }

    private async Task LoadFinancialAccounts()
    {
        IsLoading = true;
        try
        {
            FinancialAccounts = await _serviceUnitOfWork.FinancialAccountsService.GetFinancialAccounts();
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

    protected void AddFinancialAccount() => NavigationManager.NavigateTo("/FinancialAccountsAdd");

    protected void EditFinancialAccount(FinancialAccountDTO d)
        => NavigationManager.NavigateTo($"/FinancialAccountsAdd/{d.Id}");

    protected async Task DeleteFinancialAccount(FinancialAccountDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete Financial Account '{d.AccountNo}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.FinancialAccountsService.DeleteFinancialAccount(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadFinancialAccounts();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.FinancialAccountsService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => FinancialAccountsGrid?.Dispose();
}