using JM.UI.Client.Services;
using JM.UI.Entities.Model.GroupRole;
using JM.UI.Entities.Model.UserGroup;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

public partial class UserGroupListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected List<UserGroupDTO> UserGroups { get; set; } = new();
    protected List<UserGroupDTO> FilteredUserGroups { get; set; } = new();
    protected List<GroupRoleDTO> Groups { get; set; } = new();
    protected RadzenDataGrid<UserGroupDTO> grid = default!;

    protected bool IsLoading { get; set; } = false;
    protected bool IsDeleting { get; set; } = false;
    protected string SearchText { get; set; } = string.Empty;
    protected int? SelectedGroupFilter { get; set; } = null;
    protected int TotalRecords { get; set; } = 0;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadGroups();
        await LoadUserGroups();
    }

    private async Task LoadGroups()
    {
        try
        {
            Groups = (await _serviceUnitOfWork.GroupRoleService.GetGroupRoles()).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load groups: {ex.Message}");
        }
    }

    protected async Task LoadUserGroups()
    {
        try
        {
            IsLoading = true;
            var result = await _serviceUnitOfWork.UserGroupService.GetAllUserGroups();
            UserGroups = result.ToList();
            ApplyFilters();
            TotalRecords = FilteredUserGroups.Count;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load user groups: {ex.Message}");
            UserGroups = new List<UserGroupDTO>();
            FilteredUserGroups = new List<UserGroupDTO>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ApplyFilters()
    {
        var query = UserGroups.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(ug =>
                (ug.UserName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (ug.GroupName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (ug.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (ug.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        if (SelectedGroupFilter.HasValue && SelectedGroupFilter.Value > 0)
        {
            query = query.Where(ug => ug.GroupId == SelectedGroupFilter.Value);
        }

        FilteredUserGroups = query.ToList();
        TotalRecords = FilteredUserGroups.Count;
    }

    protected void OnSearchChanged(string value)
    {
        SearchText = value;
        ApplyFilters();
    }

    protected void OnGroupFilterChanged(object value)
    {
        SelectedGroupFilter = value as int?;
        ApplyFilters();
    }

    protected void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedGroupFilter = null;
        ApplyFilters();
    }

    protected void AddNew()
    {
        NavigationManager.NavigateTo("/UserGroupAdd");
    }

    protected void EditUserGroup(int id)
    {
        NavigationManager.NavigateTo($"/UserGroupAdd/{id}");
    }

    protected async Task DeleteUserGroup(UserGroupDTO userGroup)
    {
        var confirmed = await dialogService.Confirm(
            $"Are you sure you want to remove '{userGroup.UserName}' from '{userGroup.GroupName}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Remove",
                CancelButtonText = "Cancel"
            }
        );

        if (confirmed == true)
        {
            await PerformDelete(userGroup);
        }
    }

    private async Task PerformDelete(UserGroupDTO userGroup)
    {
        try
        {
            IsDeleting = true;

            var result = await _serviceUnitOfWork.UserGroupService.RemoveUserFromGroup(
                userGroup.UserId,
                userGroup.GroupId
            );

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    "User removed from group successfully!"
                );
                await LoadUserGroups();
            }
            else
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    result.Message ?? "Failed to remove user from group"
                );
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(
                NotificationSeverity.Error,
                "Error",
                $"Failed to remove user from group: {ex.Message}"
            );
        }
        finally
        {
            IsDeleting = false;
        }
    }

    protected async Task ExportToExcel()
    {
        try
        {
            if (FilteredUserGroups == null || !FilteredUserGroups.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "No data to export");
                return;
            }

            notificationService.Notify(NotificationSeverity.Info, "Info", "Export functionality to be implemented");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Export failed: {ex.Message}");
        }
    }

    protected async Task RefreshData()
    {
        await LoadUserGroups();
        notificationService.Notify(NotificationSeverity.Info, "Refreshed", "Data refreshed successfully");
    }

    protected BadgeStyle GetGroupBadgeColor(string groupName) =>
    groupName switch
    {
        "Admin" => BadgeStyle.Danger,
        "User" => BadgeStyle.Primary,
        "Manager" => BadgeStyle.Success,
        _ => BadgeStyle.Secondary
    };

}
