using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounting;

public partial class ChartOfAccountsListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected List<TreeNode> TreeNodes { get; set; } = new();
    protected bool IsLoading { get; set; }

    protected class TreeNode
    {
        public ChartOfAccountDTO Account { get; set; } = default!;
        public int Depth { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadAccounts();
    }

    protected async Task LoadAccounts()
    {
        IsLoading = true;
        try
        {
            var accounts = (await _serviceUnitOfWork.ChartOfAccountService.GetChartOfAccounts())?.ToList()
                           ?? new List<ChartOfAccountDTO>();
            TreeNodes = BuildTree(accounts);
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

    private static List<TreeNode> BuildTree(List<ChartOfAccountDTO> accounts)
    {
        // account_id 0 does not exist, so it is safe to use as the root sentinel
        // for accounts whose parent_id is NULL.
        var byParent = accounts
            .GroupBy(a => a.ParentId ?? 0)
            .ToDictionary(g => g.Key, g => g.ToList());
        var result = new List<TreeNode>();

        void AddLevel(int parentId, int depth)
        {
            if (!byParent.TryGetValue(parentId, out var children))
                return;

            foreach (var child in children.OrderBy(c => c.Code.Length).ThenBy(c => c.Code))
            {
                result.Add(new TreeNode { Account = child, Depth = depth });
                AddLevel(child.AccountId, depth + 1);
            }
        }

        AddLevel(0, 0);
        return result;
    }

    protected void AddAccount() => NavigationManager.NavigateTo("/ChartOfAccountAdd");

    protected void AddChild(ChartOfAccountDTO parent)
        => NavigationManager.NavigateTo($"/ChartOfAccountAdd?parentId={parent.AccountId}");

    protected void EditAccount(ChartOfAccountDTO account)
        => NavigationManager.NavigateTo($"/ChartOfAccountAdd/{account.AccountId}");

    protected async Task DeleteAccount(ChartOfAccountDTO account)
    {
        var confirm = await dialogService.Confirm(
            $"Delete account '{account.Code} - {account.Name}'?",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.ChartOfAccountService.DeleteChartOfAccount(account.AccountId);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus)
                await LoadAccounts();
        }
    }

    protected static string NatureStyle(string? nature) => nature switch
    {
        "ASSET" => "background:#dbeafe;color:#1e40af;",
        "LIABILITY" => "background:#fee2e2;color:#b91c1c;",
        "EQUITY" => "background:#ede9fe;color:#6d28d9;",
        "INCOME" => "background:#dcfce7;color:#15803d;",
        "EXPENSE" => "background:#ffedd5;color:#c2410c;",
        _ => "background:#f1f5f9;color:#475569;"
    };
}
