using JM.UI.Entities.Model.GroupActionPermission;
using JM.UI.Entities.Model.GroupRole;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Permission
{
    public partial class GroupActionPermissionsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Parameter] public int GroupId { get; set; }

        protected int SelectedGroupId { get; set; }
        protected string? SelectedGroupName { get; set; }
        protected List<GroupRoleDTO> Groups { get; set; } = new();
        protected List<GroupActionPermissionDTO> RoutePermissions { get; set; } = new();
        protected RadzenDataGrid<GroupActionPermissionDTO> PermissionsGrid = default!;
        protected bool IsSaving { get; set; } = false;
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadGroups();

            if (GroupId > 0)
            {
                SelectedGroupId = GroupId;
                await LoadGroupPermissions();
            }
        }

        private async Task LoadGroups()
        {
            try
            {
                // TODO: DATABASE CALL - Replace with actual database call
                 Groups = (await _serviceUnitOfWork.GroupRoleService.GetGroupRoles()).ToList();

            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load groups: {ex.Message}");
            }
        }

        protected async Task OnGroupChanged(object value)
        {
            if (value is int groupId && groupId > 0)
            {
                SelectedGroupId = groupId;
                await LoadGroupPermissions();
            }
            else
            {
                SelectedGroupId = 0;
                SelectedGroupName = null;
                RoutePermissions = new List<GroupActionPermissionDTO>();
            }
        }

        private async Task LoadGroupPermissions()
        {
            try
            {
                IsLoading = true;

                // Get selected group name
                var selectedGroup = Groups.FirstOrDefault(g => g.GroupId == SelectedGroupId);
                SelectedGroupName = selectedGroup?.GroupName;

                // TODO: DATABASE CALL - Single query to get all routes with their permissions for the group
                var routePermissions = await _serviceUnitOfWork.GroupActionPermissionService
                    .GetGroupActionPermissions(SelectedGroupId);

                // Transform the result into RoutePermissionModel
                RoutePermissions = routePermissions.Select(rp => new GroupActionPermissionDTO
                {
                    RouteId = rp.RouteId,
                    RouteName = rp.RouteName,
                    RoutePath = rp.RoutePath ?? rp.RouteName,
                    CanView = rp.HasViewPermission,
                    CanCreate = rp.HasCreatePermission,
                    CanEdit = rp.HasEditPermission,
                    CanDelete = rp.HasDeletePermission
                }).ToList();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load permissions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void SelectAllPermissions()
        {
            foreach (var route in RoutePermissions)
            {
                route.CanView = true;
                route.CanCreate = true;
                route.CanEdit = true;
                route.CanDelete = true;
            }
            StateHasChanged();
        }

        protected void ClearAllPermissions()
        {
            foreach (var route in RoutePermissions)
            {
                route.CanView = false;
                route.CanCreate = false;
                route.CanEdit = false;
                route.CanDelete = false;
            }
            StateHasChanged();
        }

        protected int GetTotalPermissionsCount()
        {
            return RoutePermissions.Sum(r =>
                (r.CanView ? 1 : 0) +
                (r.CanCreate ? 1 : 0) +
                (r.CanEdit ? 1 : 0) +
                (r.CanDelete ? 1 : 0)
            );
        }

        protected async Task OnSubmit()
        {
            try
            {
                if (SelectedGroupId == 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select a group.");
                    return;
                }

                IsSaving = true;

                // First, get all actions with their IDs
                var actions = await _serviceUnitOfWork.ActionService.GetAllActions();
                var viewActionId = actions.FirstOrDefault(a => a.ActionKey == "VIEW")?.ActionId ?? 0;
                var createActionId = actions.FirstOrDefault(a => a.ActionKey == "CREATE")?.ActionId ?? 0;
                var editActionId = actions.FirstOrDefault(a => a.ActionKey == "EDIT")?.ActionId ?? 0;
                var deleteActionId = actions.FirstOrDefault(a => a.ActionKey == "DELETE")?.ActionId ?? 0;

                // Build list of permissions to save
                var permissionsToSave = new List<GroupActionPermissionDTO>();

                foreach (var route in RoutePermissions)
                {
                    if (route.CanView && viewActionId > 0)
                    {
                        permissionsToSave.Add(new GroupActionPermissionDTO
                        {
                            GroupId = SelectedGroupId,
                            RouteId = route.RouteId,
                            ActionId = viewActionId
                        });
                    }
                    if (route.CanCreate && createActionId > 0)
                    {
                        permissionsToSave.Add(new GroupActionPermissionDTO
                        {
                            GroupId = SelectedGroupId,
                            RouteId = route.RouteId,
                            ActionId = createActionId
                        });
                    }
                    if (route.CanEdit && editActionId > 0)
                    {
                        permissionsToSave.Add(new GroupActionPermissionDTO
                        {
                            GroupId = SelectedGroupId,
                            RouteId = route.RouteId,
                            ActionId = editActionId
                        });
                    }
                    if (route.CanDelete && deleteActionId > 0)
                    {
                        permissionsToSave.Add(new GroupActionPermissionDTO
                        {
                            GroupId = SelectedGroupId,
                            RouteId = route.RouteId,
                            ActionId = deleteActionId
                        });
                    }
                }

                // Call service to save permissions
                var result = await _serviceUnitOfWork.GroupActionPermissionService.InsertUpdateGroupActionPermissions(SelectedGroupId, permissionsToSave);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message);
                    NavigationManager.NavigateTo("/GroupActionPermissionsAdd");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to save permissions.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"An error occurred: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/GroupActionPermissionsList");
        }
       
        public void Dispose()
        {
            PermissionsGrid?.Dispose();
        }
    }
}