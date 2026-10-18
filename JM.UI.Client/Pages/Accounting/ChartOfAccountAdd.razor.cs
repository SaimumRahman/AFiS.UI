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
    protected bool IsCreateMode => !IsEditMode;
    protected string PageTitle => IsEditMode ? "Edit Account" : "Add New Account";

    /// <summary>Preview of the number the server would assign (null until the parent is known).</summary>
    protected NextAccountNoDTO? Preview { get; set; }

    protected bool HasParent => Account.ParentId is > 0;
    protected bool IsAutoNumberingAllowed => Preview?.IsAutoAllowed ?? false;

    /// <summary>Hint shown under the Code box.</summary>
    protected string CodeHint
    {
        get
        {
            if (IsEditMode)
                return "Account numbers cannot be changed after creation.";

            if (!HasParent)
                return "Select a parent account to get the next free number.";

            if (Preview == null)
                return "Checking the next free number…";

            if (!Preview.IsAutoAllowed)
                return string.IsNullOrWhiteSpace(Preview.Message)
                    ? "Enter the account number manually."
                    : Preview.Message;

            return string.IsNullOrWhiteSpace(Account.Code)
                ? "The number is generated from the parent when saved."
                : $"Auto-filled from the parent — next free number was {Preview.Code}.";
        }
    }

    protected class AccountTreeNode
    {
        public int AccountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public List<AccountTreeNode> Children { get; set; } = new();
        public string Display => $"{Code} - {Name}";
    }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDropdowns();

        if (IsEditMode)
        {
            await LoadAccount();
        }
        else
        {
            // A new head is usable straight away and carries a branch, matching the live
            // chart where every account has requires_branch = 1.
            Account.IsActive = true;
            Account.RequiresBranch = true;

            await ApplyParentFromQueryAsync();
        }
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
            nodes[g.AccountId] = new AccountTreeNode { AccountId = g.AccountId, Code = g.Code, Name = g.Name, Nature = g.Nature };

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

    private async Task ApplyParentFromQueryAsync()
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

            var match = GroupAccounts.FirstOrDefault(g => g.AccountId == parentId);
            if (match != null && !string.IsNullOrWhiteSpace(match.Nature))
                Account.Nature = match.Nature;

            await LoadNextAccountNoAsync();
        }
    }

    /// <summary>
    /// Asks the server for the next free account number under the selected parent and
    /// fills the Code box. When the parent is too deep (or its series is exhausted) the
    /// number has to be typed manually and the reason is shown.
    /// </summary>
    protected async Task LoadNextAccountNoAsync(bool applyCode = true)
    {
        Preview = null;

        if (!IsCreateMode || !HasParent)
            return;

        try
        {
            Preview = await _serviceUnitOfWork.ChartOfAccountService.GetNextAccountNo(Account.ParentId!.Value, Account.IsGroup);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Numbering", $"Could not get the next account number: {ex.Message}");
            return;
        }

        if (Preview == null)
        {
            if (applyCode)
                Account.Code = string.Empty;

            return;
        }

        if (Preview.IsAutoAllowed && !string.IsNullOrWhiteSpace(Preview.Code))
        {
            if (applyCode)
                Account.Code = Preview.Code;
        }
        else if (applyCode)
        {
            Account.Code = string.Empty;
            notificationService.Notify(NotificationSeverity.Info, "Manual account number required",
                string.IsNullOrWhiteSpace(Preview.Message) ? "Enter the account number manually." : Preview.Message);
        }
    }

    protected async Task OnIsGroupChanged(bool _) => await LoadNextAccountNoAsync();

    protected async Task Save()
    {
        if (IsCreateMode && HasParent && !IsAutoNumberingAllowed && string.IsNullOrWhiteSpace(Account.Code))
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                string.IsNullOrWhiteSpace(Preview?.Message) ? "Enter the account number manually." : Preview!.Message);
            return;
        }

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
                    string.IsNullOrWhiteSpace(result.Message)
                        ? (IsEditMode ? "Account updated successfully!" : "Account created successfully!")
                        : result.Message);
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

                // The nature is inherited from the parent account.
                if (!string.IsNullOrWhiteSpace(node.Nature))
                    Account.Nature = node.Nature;
            }

            isOpen = false;
            await popup.CloseAsync(trigger);
            await LoadNextAccountNoAsync();
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
        Preview = null;

        // A new head must sit under a group, so the auto-filled number no longer applies.
        if (IsCreateMode)
            Account.Code = string.Empty;
    }
}
