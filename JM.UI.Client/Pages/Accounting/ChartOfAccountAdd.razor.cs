using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounting;

public partial class ChartOfAccountAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? Id { get; set; }

    protected ChartOfAccountDTO Account { get; set; } = new();
    protected List<ChartOfAccountDTO> GroupAccounts { get; set; } = new();
    protected List<AccountTreeNode> ParentTree { get; set; } = new();
    protected List<string> PartyTypes { get; set; } = new();
    protected List<string> NatureOptions { get; set; } = new() { "ASSET", "LIABILITY", "EQUITY", "INCOME", "EXPENSE" };
    protected bool IsProcessing { get; set; }
    protected bool IsLoading { get; set; }

    // Parent dropdown tree state.
    protected ElementReference trigger;
    protected RadzenPopup popup;
    protected object? ParentSelection;
    protected string? SelectedParentText;
    protected bool isOpen;

    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Account" : "Add New Account";

    protected class AccountTreeNode
    {
        public int AccountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<AccountTreeNode> Children { get; set; } = new();
        public string Display => $"{Code} - {Name}";
    }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDropdowns();

        if (IsEditMode)
            await LoadAccount();
        else
            ApplyParentFromQuery();
    }

    private async Task LoadDropdowns()
    {
        try
        {
            GroupAccounts = (await _serviceUnitOfWork.ChartOfAccountService.GetGroupAccounts())?.ToList()
                            ?? new List<ChartOfAccountDTO>();
            ParentTree = BuildParentTree(GroupAccounts);
            PartyTypes = (await _serviceUnitOfWork.ChartOfAccountService.GetPartyTypes())?.ToList()
                         ?? new List<string>();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load dropdowns: {ex.Message}");
        }
    }

    private async Task LoadAccount()
    {
        try
        {
            IsLoading = true;
            var data = await _serviceUnitOfWork.ChartOfAccountService.GetChartOfAccountById(Id!.Value);
            if (data == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Account not found.");
                NavigationManager.NavigateTo("/ChartOfAccountsList");
                return;
            }
            Account = data;
            SelectedParentText = ResolveParentText(Account.ParentId);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load account: {ex.Message}");
            NavigationManager.NavigateTo("/ChartOfAccountsList");
        }
        finally { IsLoading = false; }
    }

    private static List<AccountTreeNode> BuildParentTree(List<ChartOfAccountDTO> groups)
    {
        var nodes = new Dictionary<int, AccountTreeNode>();
        foreach (var g in groups)
            nodes[g.AccountId] = new AccountTreeNode { AccountId = g.AccountId, Code = g.Code, Name = g.Name };

        var roots = new List<AccountTreeNode>();
        foreach (var g in groups)
        {
            var node = nodes[g.AccountId];
            if (g.ParentId.HasValue && nodes.TryGetValue(g.ParentId.Value, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }

    private string? ResolveParentText(int? parentId)
    {
        if (!parentId.HasValue)
            return null;

        var match = GroupAccounts.FirstOrDefault(g => g.AccountId == parentId.Value);
        return match == null ? null : $"{match.Code} - {match.Name}";
    }

    private void ApplyParentFromQuery()
    {
        var uri = NavigationManager.Uri;
        const string marker = "parentId=";
        var idx = uri.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return;

        var value = uri[(idx + marker.Length)..];
        var amp = value.IndexOf('&');
        if (amp >= 0) value = value[..amp];

        if (int.TryParse(value, out var parentId) && parentId > 0)
        {
            Account.ParentId = parentId;
            SelectedParentText = ResolveParentText(parentId);
        }
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.ChartOfAccountService.ValidateChartOfAccount(Account);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ChartOfAccountService.SaveUpdateChartOfAccount(Account);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Account updated successfully!" : "Account created successfully!");
                NavigationManager.NavigateTo("/ChartOfAccountsList");
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
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/ChartOfAccountsList");

    protected void OnPartyTypeChanged(string partyType, bool isChecked)
    {
        Account.PartyTypes ??= new List<string>();

        if (isChecked)
        {
            if (!Account.PartyTypes.Contains(partyType))
                Account.PartyTypes.Add(partyType);
        }
        else
        {
            Account.PartyTypes.Remove(partyType);
        }
    }

    // ── Parent dropdown tree ──────────────────────────────────────────────

    protected async Task Toggle()
    {
        isOpen = !isOpen;
        await popup.ToggleAsync(trigger);
    }

    protected async Task OnParentSelect()
    {
        if (isOpen)
        {
            if (ParentSelection is AccountTreeNode node)
            {
                Account.ParentId = node.AccountId;
                SelectedParentText = node.Display;
            }

            isOpen = false;
            await popup.CloseAsync(trigger);
        }
    }

    protected void OnPopupClose()
    {
        isOpen = false;
    }

    protected void ClearParent()
    {
        Account.ParentId = null;
        SelectedParentText = null;
        ParentSelection = null;
    }
}
