using JM.UI.Client.Pages.Dialog.Todo;
using JM.UI.Entities.Model.Todo;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Todo;

public partial class TodoDashboardComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    // ── Identity / lookups (all resolved by the API from the bearer token) ──
    protected TodoCurrentUserDTO Me { get; set; } = new();
    protected TodoLookupsDTO Lookups { get; set; } = new();

    // ── Dashboard state ──
    protected TodoDashboardDTO Dashboard { get; set; } = new();
    protected List<TodoTaskListItemDTO> Tasks => Dashboard.Items;
    protected TodoCountsDTO Counts => Dashboard.Counts;

    protected int ActiveTab { get; set; }
    protected int PageNumber { get; set; } = 1;
    protected int PageSize { get; set; } = 20;
    protected readonly int[] PageSizes = { 20, 50, 100 };

    // ── Filters ──
    protected string SearchText { get; set; } = string.Empty;
    protected int? StatusFilter { get; set; }
    protected int? FlagFilter { get; set; }
    protected bool MineOnly { get; set; }
    protected bool OverdueOnly { get; set; }
    protected bool DueTodayOnly { get; set; }

    protected List<StatusOption> StatusOptions { get; } = new()
    {
        new StatusOption { Value = 0, Text = "Open" },
        new StatusOption { Value = 1, Text = "Completed" }
    };

    // ── Deleted tasks (Admin) ──
    protected List<TodoDeletedTaskDTO> DeletedItems { get; set; } = new();
    protected int DeletedPage { get; set; } = 1;
    protected int DeletedTotalCount { get; set; }
    protected int DeletedTotalPages { get; set; }
    protected bool DeletedLoading { get; set; }

    // ── Activity log (Admin) ──
    protected List<TodoActivityLogDTO> ActivityItems { get; set; } = new();
    protected int? ActivityUserId { get; set; }
    protected int ActivityPage { get; set; } = 1;
    protected int ActivityTotalCount { get; set; }
    protected int ActivityTotalPages { get; set; }
    protected bool ActivityLoading { get; set; }

    // ── Misc ──
    protected bool IsLoading { get; set; }
    protected string? LoadError { get; set; }

    private bool _initialised;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadIdentity();
        await LoadDashboard();
        _initialised = true;
    }

    // ─────────────────────────────────────────────────────────────── loading

    private async Task LoadIdentity()
    {
        try
        {
            Lookups = await _serviceUnitOfWork.TodoService.GetLookups();
            Me = Lookups.Me ?? new TodoCurrentUserDTO();
        }
        catch (Exception ex)
        {
            LoadError = "Could not load your To-Do profile: " + ex.Message;
        }
    }

    protected async Task LoadDashboard()
    {
        IsLoading = true;
        LoadError = null;
        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            Dashboard = await _serviceUnitOfWork.TodoService.GetDashboard(
                StatusFilter, FlagFilter, search, MineOnly, OverdueOnly, DueTodayOnly, PageNumber, PageSize);
        }
        catch (Exception ex)
        {
            Dashboard = new TodoDashboardDTO { Page = PageNumber, PageSize = PageSize };
            LoadError = "Could not load tasks: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task LoadDeleted()
    {
        if (!Me.IsAdmin) return;

        DeletedLoading = true;
        try
        {
            var page = await _serviceUnitOfWork.TodoService.GetDeleted(DeletedPage, 20);
            DeletedItems = page.Items ?? new();
            DeletedTotalCount = page.TotalCount;
            DeletedTotalPages = page.TotalPages;
        }
        catch (Exception ex)
        {
            LoadError = "Could not load deleted tasks: " + ex.Message;
        }
        finally
        {
            DeletedLoading = false;
        }
    }

    protected async Task LoadActivity()
    {
        if (!Me.IsAdmin) return;

        ActivityLoading = true;
        try
        {
            var page = await _serviceUnitOfWork.TodoService.GetActivity(ActivityUserId, ActivityPage, 50);
            ActivityItems = page.Items ?? new();
            ActivityTotalCount = page.TotalCount;
            ActivityTotalPages = page.TotalPages;
        }
        catch (Exception ex)
        {
            LoadError = "Could not load the activity log: " + ex.Message;
        }
        finally
        {
            ActivityLoading = false;
        }
    }

    // ─────────────────────────────────────────────────────────────── filters

    protected async Task ApplyFilters()
    {
        if (!_initialised) return;
        PageNumber = 1;
        await LoadDashboard();
    }

    protected async Task ClearFilters()
    {
        SearchText = string.Empty;
        StatusFilter = null;
        FlagFilter = null;
        MineOnly = false;
        OverdueOnly = false;
        DueTodayOnly = false;
        ActiveTab = 0;
        PageNumber = 1;
        await LoadDashboard();
    }

    /// <summary>KPI tiles double as quick filters.</summary>
    protected async Task FilterByKpi(int kpi)
    {
        ActiveTab = 0;
        PageNumber = 1;

        switch (kpi)
        {
            case 1: // Overdue
                OverdueOnly = !OverdueOnly;
                DueTodayOnly = false;
                StatusFilter = 0;
                break;
            case 2: // Due today
                DueTodayOnly = !DueTodayOnly;
                OverdueOnly = false;
                StatusFilter = 0;
                break;
            case 3: // Completed
                StatusFilter = StatusFilter == 1 ? null : 1;
                OverdueOnly = false;
                DueTodayOnly = false;
                break;
            default: // Open
                StatusFilter = StatusFilter == 0 && !OverdueOnly && !DueTodayOnly ? null : 0;
                OverdueOnly = false;
                DueTodayOnly = false;
                break;
        }

        await LoadDashboard();
    }

    protected async Task SelectTab(int tab)
    {
        ActiveTab = tab;
        LoadError = null;

        if (tab == 1) await LoadDeleted();
        else if (tab == 2) await LoadActivity();
        else await LoadDashboard();
    }

    // ─────────────────────────────────────────────────────────────── paging

    protected async Task GoToPage(int page)
    {
        PageNumber = page < 1 ? 1 : page;
        await LoadDashboard();
    }

    protected async Task OnPageSizeChanged()
    {
        PageNumber = 1;
        await LoadDashboard();
    }

    protected async Task GoToDeletedPage(int page)
    {
        DeletedPage = page < 1 ? 1 : page;
        await LoadDeleted();
    }

    protected async Task ApplyActivityFilter()
    {
        ActivityPage = 1;
        await LoadActivity();
    }

    protected async Task GoToActivityPage(int page)
    {
        ActivityPage = page < 1 ? 1 : page;
        await LoadActivity();
    }

    // ─────────────────────────────────────────────────────────────── actions

    protected async Task NewTask()
    {
        var created = await dialogService.OpenAsync<TodoCreateEditDialog>(
            "New To-Do Task",
            new Dictionary<string, object>
            {
                { nameof(TodoCreateEditDialog.TaskId), 0 },
                { nameof(TodoCreateEditDialog.Lookups), Lookups }
            },
            new DialogOptions
            {
                Width = "860px",
                Resizable = true,
                Draggable = true,
                ShowClose = true
            });

        if (created is bool ok && ok)
        {
            PageNumber = 1;
            await LoadDashboard();
            Notify("Task created.");
        }
    }

    protected async Task OpenTask(TodoTaskListItemDTO task)
    {
        var changed = await dialogService.OpenAsync<TodoDetailDialog>(
            "Task Details",
            new Dictionary<string, object>
            {
                { nameof(TodoDetailDialog.TaskId), task.Id },
                { nameof(TodoDetailDialog.Lookups), Lookups }
            },
            new DialogOptions
            {
                Width = "820px",
                Resizable = true,
                Draggable = true,
                ShowClose = true
            });

        if (changed is bool ok && ok)
        {
            await ReloadCurrentView();
        }
    }

    protected async Task QuickReopen(TodoTaskListItemDTO task)
    {
        var result = await _serviceUnitOfWork.TodoService.Reopen(task.Id);
        if (result.Success)
        {
            Notify(result.Message);
            await ReloadCurrentView();
        }
        else
        {
            ShowError(result.Message);
        }
    }

    protected async Task Restore(TodoDeletedTaskDTO row)
    {
        var confirmed = await dialogService.Confirm(
            $"Restore \"{row.Title}\" back to the task list?", "Restore task",
            new ConfirmOptions { OkButtonText = "Restore", CancelButtonText = "Cancel" });

        if (confirmed != true) return;

        var result = await _serviceUnitOfWork.TodoService.RestoreTask(row.Id);
        if (result.Success)
        {
            Notify(result.Message);
            await LoadDeleted();
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private async Task ReloadCurrentView()
    {
        if (ActiveTab == 1) await LoadDeleted();
        else if (ActiveTab == 2) await LoadActivity();
        else await LoadDashboard();
    }

    // ─────────────────────────────────────────────────────────────── display

    protected static string StripeClass(int flag) => flag switch
    {
        3 => "td-stripe-urgent",
        2 => "td-stripe-important",
        _ => "td-stripe-general"
    };

    protected static BadgeStyle FlagBadgeStyle(int flag) => flag switch
    {
        3 => BadgeStyle.Danger,
        2 => BadgeStyle.Warning,
        _ => BadgeStyle.Light
    };

    protected static bool HasAudience(TodoTaskListItemDTO task)
        => task.AllBranches || task.BranchNames.Count > 0 || task.GroupNames.Count > 0
           || task.DesignationNames.Count > 0 || task.UserNames.Count > 0;

    protected static string FormatLocal(DateTime? local)
        => local.HasValue ? local.Value.ToString("dd MMM yyyy, hh:mm tt") : "-";

    protected static string FormatLocal(DateTime local)
        => local == default ? "-" : local.ToString("dd MMM yyyy, hh:mm tt");

    protected static string DueText(TodoTaskListItemDTO task)
    {
        if (!task.DueAtLocal.HasValue) return "No due date";

        var due = task.DueAtLocal.Value;
        if (task.Status == 1)
            return "Was due " + due.ToString("dd MMM yyyy, hh:mm tt");

        return due.ToString("dd MMM yyyy, hh:mm tt") + " (BD)";
    }

    private void Notify(string message)
        => notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "To-Do List",
            Detail = message,
            Duration = 3000
        });

    private void ShowError(string message)
        => notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "To-Do List",
            Detail = message,
            Duration = 6000
        });

    protected class StatusOption
    {
        public int? Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
