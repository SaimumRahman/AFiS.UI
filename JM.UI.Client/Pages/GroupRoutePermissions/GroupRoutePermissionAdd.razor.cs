using JM.UI.Entities.Model.GroupRole;
using JM.UI.Entities.Model.GroupRoutePermission;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.Routes;
using JM.UI.Entities.Model.UserGroup;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Action;

public partial class GroupRoutePermissionAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? GroupId { get; set; }

    protected List<GroupRoleDTO> GroupRoles { get; set; } = new();
    protected List<RouteModelDTO> Routes { get; set; } = new();
    protected IEnumerable<RouteModelDTO> FilteredRoutes => Routes.Where(r =>
        string.IsNullOrEmpty(SearchText) ||
        r.RouteName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        (r.RoutePath?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
    );

    protected int? SelectedGroupId { get; set; }
    protected List<int> SelectedRouteIds { get; set; } = new();
    protected List<int> ExistingRouteIds { get; set; } = new();
    protected string SearchText { get; set; } = string.Empty;

    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected string PageTitle => "Manage Group Route Permissions";
    protected string PageIcon => "vpn_key";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            IsLoading = true;

            // Load groups
            GroupRoles = (await _serviceUnitOfWork.GroupRoleService.GetGroupRoles()).ToList();

            // Load routes
            Routes = (await _serviceUnitOfWork.RouteService.GetRoutes()).ToList();


            // If GroupId parameter is provided, pre-select the group
            if (GroupId.HasValue && GroupId.Value > 0)
            {
                SelectedGroupId = GroupId.Value;
                await OnGroupChanged(SelectedGroupId);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task OnGroupChanged(object value)
    {
        try
        {
            if (value == null) return;

            SelectedGroupId = Convert.ToInt32(value);

            // Reset route selections
            SelectedRouteIds.Clear();
            foreach (var route in Routes)
            {
                route.IsSelected = false;
            }

            // Load existing permissions for this group
            var existingPermissions = (await _serviceUnitOfWork
                .GroupRoutePermissionService
                .GetGroupRoutePermissionByGroupId(Convert.ToInt32(SelectedGroupId)))
                .ToList();

            // Extract permitted RouteIds
            var permittedRouteIds = existingPermissions
                .Select(p => p.RouteId)
                .ToHashSet();

            // Pre-select existing routes
            foreach (var route in Routes.Where(r => permittedRouteIds.Contains(r.RouteId)))
            {
                route.IsSelected = true;
                SelectedRouteIds.Add(route.RouteId);
            }

            StateHasChanged();
        }
        catch (Exception ex)
        {
            notificationService.Notify(
                NotificationSeverity.Error,
                "Error",
                $"Failed to load group permissions: {ex.Message}"
            );
        }
    }

    protected async Task OnRouteSelectionChanged(RouteModelDTO route, bool isChecked)
    {
        route.IsSelected = isChecked;

        if (isChecked)
        {
            if (!SelectedRouteIds.Contains(route.RouteId))
            {
                SelectedRouteIds.Add(route.RouteId);
            }
        }
        else
        {
            SelectedRouteIds.Remove(route.RouteId);
        }

        SelectedRouteIds = SelectedRouteIds.Distinct().OrderBy(x => x).ToList();
        await InvokeAsync(StateHasChanged);  
    }





    protected void SelectAllRoutes()
    {
        SelectedRouteIds.Clear();
        foreach (var route in FilteredRoutes)
        {
            route.IsSelected = true;
            SelectedRouteIds.Add(route.RouteId);
        }
        StateHasChanged();
    }

    protected void DeselectAllRoutes()
    {
        foreach (var route in FilteredRoutes)
        {
            route.IsSelected = false;
        }
        SelectedRouteIds.Clear();
        StateHasChanged();
    }

    protected void OnSearchChanged(string searchValue)
    {
        SearchText = searchValue ?? string.Empty;
        StateHasChanged();
    }

    protected async Task Save()
    {
        if (SelectedGroupId == null || SelectedGroupId == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select a group.");
            return;
        }

        if (SelectedRouteIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select at least one route.");
            return;
        }

        try
        {
            IsProcessing = true;

            // Determine routes to add and remove
            var routesToAdd = SelectedRouteIds.Except(ExistingRouteIds).ToList();
            var routesToRemove = ExistingRouteIds.Except(SelectedRouteIds).ToList();

            int addedCount = 0;
            int removedCount = 0;
            var errors = new List<string>();

            // Remove unselected permissions
            if (routesToRemove.Any())
            {
                var allPermissions = await _serviceUnitOfWork.GroupRoutePermissionService.GetGroupRoutePermissions();
                var permissionsToDelete = allPermissions
                    .Where(p => p.GroupId == SelectedGroupId && routesToRemove.Contains(p.RouteId))
                    .ToList();

                foreach (var permission in permissionsToDelete)
                {
                    var result = await _serviceUnitOfWork.GroupRoutePermissionService.DeleteGroupRoutePermission(permission.Id);
                    if (result.IsSuccessStatus)
                    {
                        removedCount++;
                    }
                    else
                    {
                        errors.Add($"Failed to remove route {permission.RouteId}");
                    }
                }
            }

            // Add new permissions
            if (routesToAdd.Any())
            {
                foreach (var routeId in routesToAdd)
                {
                    var permission = new GroupRoutePermissionModelDTO
                    {
                        GroupId = SelectedGroupId.Value,
                        RouteId = routeId
                    };

                    var result = await _serviceUnitOfWork.GroupRoutePermissionService.SaveUpdateGroupRoutePermission(permission);
                    if (result.IsSuccessStatus)
                    {
                        addedCount++;
                    }
                    else
                    {
                        errors.Add($"Failed to add route {routeId}");
                    }
                }
            }

            // Show result notification
            if (errors.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Partial Success",
                    $"Added: {addedCount}, Removed: {removedCount}. Errors: {string.Join(", ", errors)}");
            }
            else if (addedCount > 0 || removedCount > 0)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"Permissions saved successfully! Added: {addedCount}, Removed: {removedCount}");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Info, "No Changes",
                    "No changes were made to the permissions.");
            }

            // Reload existing permissions
            await OnGroupChanged(SelectedGroupId);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save permissions: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/GroupRoutePermissionList");
    }

    protected async Task Reset()
    {
        SearchText = string.Empty;
        SelectedGroupId = null;
        SelectedRouteIds.Clear();
        ExistingRouteIds.Clear();

        foreach (var route in Routes)
        {
            route.IsSelected = false;
        }

        if (GroupId.HasValue && GroupId.Value > 0)
        {
            SelectedGroupId = GroupId.Value;
            await OnGroupChanged(SelectedGroupId);
        }

        StateHasChanged();
    }
}

