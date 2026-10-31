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

public partial class ManualJournalListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected const string CompanyCode = "AFHO";

    protected RadzenDataGrid<ManualJournalSummaryDTO>? Grid;
    protected List<ManualJournalSummaryDTO> Items { get; set; } = new();
    protected List<AccountingBranchDTO> Branches { get; set; } = new();

    protected List<string> StatusOptions { get; } = new() { "DRAFT", "POSTED", "REVERSED" };

    protected DateTime FromDate { get; set; } = DateTime.Today.AddMonths(-1);
    protected DateTime ToDate { get; set; } = DateTime.Today;
    protected string? BranchFilter { get; set; }
    protected string? StatusFilter { get; set; }
    protected string? SearchText { get; set; }

    protected bool IsLoading { get; set; }
    protected int TotalCount { get; set; }
    protected int CurrentUserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");

        try
        {
            var context = await _serviceUnitOfWork.ManualJournalService.GetContext(
                CompanyCode, await GetLocalStorageInt("StoreId"), DateTime.Today);
            Branches = context?.Branches ?? new List<AccountingBranchDTO>();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load branches: {ex.Message}");
        }
    }

    protected async Task LoadData(LoadDataArgs args)
    {
        IsLoading = true;
        try
        {
            var pageSize = args.Top.GetValueOrDefault() > 0 ? args.Top!.Value : 20;
            var skip = args.Skip.GetValueOrDefault();
            var page = (skip / pageSize) + 1;

            var result = await _serviceUnitOfWork.ManualJournalService.GetList(new ManualJournalListRequestDTO
            {
                CompanyCode = CompanyCode,
                FromDate = FromDate.Date,
                ToDate = ToDate.Date,
                BranchCode = BranchFilter,
                Status = StatusFilter,
                Search = SearchText,
                Page = page,
                PageSize = pageSize
            });

            Items = result.Items ?? new List<ManualJournalSummaryDTO>();
            TotalCount = result.TotalCount;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load manual journals: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void NewJournal()
        => NavigationManager.NavigateTo("/ManualJournalEntry");

    protected void Open(ManualJournalSummaryDTO item)
        => NavigationManager.NavigateTo($"/ManualJournalEntry?entryId={item.EntryId}");

    protected async Task PostJournal(ManualJournalSummaryDTO item)
    {
        var confirmed = await dialogService.Confirm(
            $"Post draft journal of {item.TotalDebit:N2} on {item.EntryDate:dd-MMM-yyyy}? " +
            "Posting assigns the next JRN voucher number and makes the entry immutable.",
            "Post Journal",
            new ConfirmOptions { OkButtonText = "Post", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        await RunAction(
            () => _serviceUnitOfWork.ManualJournalService.Post(item.EntryId, CurrentUserId),
            "Journal posted.");
    }

    protected async Task ReverseJournal(ManualJournalSummaryDTO item)
    {
        var confirmed = await dialogService.Confirm(
            $"Reverse {item.VoucherDisplay}? A new contra journal is posted and the original is marked REVERSED. " +
            "You can then enter a corrected journal.",
            "Reverse Journal",
            new ConfirmOptions { OkButtonText = "Reverse", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        await RunAction(
            () => _serviceUnitOfWork.ManualJournalService.Reverse(item.EntryId, CurrentUserId),
            "Journal reversed.");
    }

    protected async Task DeleteDraft(ManualJournalSummaryDTO item)
    {
        var confirmed = await dialogService.Confirm(
            "Delete this draft permanently?",
            "Delete Draft",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        await RunAction(
            () => _serviceUnitOfWork.ManualJournalService.DeleteDraft(item.EntryId, CurrentUserId),
            "Draft deleted.");
    }

    private async Task RunAction(Func<Task<ManualJournalSaveResultDTO>> action, string successMessage)
    {
        try
        {
            IsLoading = true;
            var result = await action();

            notificationService.Notify(
                result.Success ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.Success ? "Success" : "Error",
                result.Success ? (result.Message ?? successMessage) : (result.Message ?? "The action failed."));

            if (result.Success && Grid != null)
                await Grid.Reload();
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

    protected static BadgeStyle StatusBadge(string? status) => status switch
    {
        "POSTED" => BadgeStyle.Success,
        "DRAFT" => BadgeStyle.Warning,
        "REVERSED" => BadgeStyle.Danger,
        _ => BadgeStyle.Secondary
    };

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(key);
            return int.TryParse(result.Value, out var value) ? value : 0;
        }
        catch
        {
            return 0;
        }
    }
}
