using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.FinancialAccounts;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.FinancialAccounts;

public partial class FinancialAccountsAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? Id { get; set; }

    protected FinancialAccountDTO FinancialAccount { get; set; } = new();
    protected List<FinancialAccountTypeDTO> AccountTypes { get; set; } = new();
    protected List<MFSTypeDTO> MFSTypes { get; set; } = new();
    protected List<BanksDTO> Banks { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Financial Account" : "Add New Financial Account";
    protected string PageIcon => IsEditMode ? "edit" : "account_balance";

    protected string? SelectedAccountTypeName =>
        AccountTypes.FirstOrDefault(t => t.Id == FinancialAccount.FinancialAccountTypeId)?.Name;

    protected bool IsBankType => SelectedAccountTypeName?.Contains("Bank", StringComparison.OrdinalIgnoreCase) == true;
    protected bool IsMFSType => SelectedAccountTypeName?.Contains("MFS", StringComparison.OrdinalIgnoreCase) == true;

    protected void OnAccountTypeChanged(object value)
    {
        if (!IsBankType) FinancialAccount.BankId = null;
        if (!IsMFSType) FinancialAccount.MFSTypeId = null;
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDropdowns();

        if (IsEditMode)
            await LoadFinancialAccount();
    }

    private async Task LoadDropdowns()
    {
        try
        {
            AccountTypes = (await _serviceUnitOfWork.FinancialAccountsService.GetFinancialAccountTypes()).ToList();
            MFSTypes = (await _serviceUnitOfWork.FinancialAccountsService.GetMFSTypes()).ToList();
            Banks = (await _serviceUnitOfWork.FinancialAccountsService.GetBanks()).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load dropdowns: {ex.Message}");
        }
    }

    private async Task LoadFinancialAccount()
    {
        try
        {
            IsLoading = true;
            var data = await _serviceUnitOfWork.FinancialAccountsService.GetFinancialAccountById(Id!.Value);
            if (data == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Financial Account not found.");
                NavigationManager.NavigateTo("/FinancialAccountsList");
                return;
            }
            FinancialAccount = data;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Financial Account: {ex.Message}");
            NavigationManager.NavigateTo("/FinancialAccountsList");
        }
        finally { IsLoading = false; }
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.FinancialAccountsService.ValidateFinancialAccount(FinancialAccount);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.FinancialAccountsService.SaveUpdateFinancialAccount(FinancialAccount);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Financial Account updated successfully!" : "Financial Account created successfully!");
                NavigationManager.NavigateTo("/FinancialAccountsList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Financial Account: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.FinancialAccountsService.ValidateFinancialAccount(FinancialAccount);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.FinancialAccountsService.SaveUpdateFinancialAccount(FinancialAccount);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Financial Account created successfully!");
                FinancialAccount = new FinancialAccountDTO();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Financial Account: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/FinancialAccountsList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadFinancialAccount();
        else FinancialAccount = new FinancialAccountDTO();
        StateHasChanged();
    }
}