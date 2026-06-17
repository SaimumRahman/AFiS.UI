using JM.UI.Client.Services;
using JM.UI.Entities.Model.GroupRole;
using JM.UI.Entities.Model.UserGroup;
using JM.UI.Entities.Model.Users;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.Users;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

public partial class UserGroupAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public IUserAuthService userAuthService { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected UserGroupDTO UserGroup { get; set; } = new();
    protected IEnumerable<int> SelectedUserIds { get; set; } = new List<int>();
    protected List<User> Users { get; set; } = new();
    protected List<GroupRoleDTO> Groups { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Group Users" : "Assign Users to Group";
    protected string PageIcon => IsEditMode ? "edit" : "group_add";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDropdownData();

        if (IsEditMode)
        {
            await LoadUserGroup();
        }
        else
        {
            InitializeUserGroup();
        }
    }

    private async Task LoadDropdownData()
    {
        try
        {
            Users = await userAuthService.GetAllUsers();
            Groups = (await _serviceUnitOfWork.GroupRoleService.GetGroupRoles()).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load data: {ex.Message}");
        }
    }

    private async Task LoadUserGroup()
    {
        try
        {
            IsLoading = true;

            // In edit mode, Id is the GroupId
            // Load all users currently in this group
            var userGroups = await _serviceUnitOfWork.UserGroupService.GetUserGroupsByGroupId(Id!.Value);
            var userGroupsList = userGroups.ToList();

            if (!userGroupsList.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "No users found in this group.");
                // Still allow editing - user can add users to empty group
            }

            // Set the GroupId from the parameter
            UserGroup.GroupId = Id.Value;

            // If there are existing assignments, get the group info from the first one
            if (userGroupsList.Any())
            {
                var firstAssignment = userGroupsList.First();
                UserGroup.GroupName = firstAssignment.GroupName;
                UserGroup.Description = firstAssignment.Description;
                UserGroup.IsSystem = firstAssignment.IsSystem;
            }
            else
            {
                // Load group info separately if no users assigned yet
                var group = Groups.FirstOrDefault(g => g.GroupId == Id.Value);
                if (group != null)
                {
                    UserGroup.GroupName = group.GroupName;
                }
            }

            // Select all users currently in the group
            SelectedUserIds = userGroupsList.Select(ug => ug.UserId).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group users: {ex.Message}");
            NavigationManager.NavigateTo("/UserGroupList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeUserGroup()
    {
        UserGroup = new UserGroupDTO
        {
            UserGroupId = 0,
            UserId = 0,
            GroupId = 0
        };
        SelectedUserIds = new List<int>();
    }

    protected async Task Save()
    {
        if (SelectedUserIds == null || !SelectedUserIds.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select at least one user.");
            return;
        }

        if (UserGroup.GroupId <= 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select a group.");
            return;
        }

        try
        {
            IsProcessing = true;

            if (IsEditMode)
            {
                // Edit mode: update all users in the group
                var result = await _serviceUnitOfWork.UserGroupService.UpdateGroupUsers(
                    UserGroup.GroupId,
                    SelectedUserIds.ToList()
                );

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        result.Message ?? "Group users updated successfully!");
                    NavigationManager.NavigateTo("/UserGroupList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            else
            {
                // Add mode: assign new users to group
                var result = await _serviceUnitOfWork.UserGroupService.AssignUsersToGroup(
                    UserGroup.GroupId,
                    SelectedUserIds.ToList()
                );

                if (result.IsSuccessStatus)
                {
                    var successMessage = result.Message ?? $"{SelectedUserIds.Count()} user(s) assigned to group successfully!";
                    notificationService.Notify(NotificationSeverity.Success, "Success", successMessage);
                    NavigationManager.NavigateTo("/UserGroupList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode)
        {
            // In edit mode, just save
            await Save();
            return;
        }

        if (SelectedUserIds == null || !SelectedUserIds.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select at least one user.");
            return;
        }

        if (UserGroup.GroupId <= 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select a group.");
            return;
        }

        try
        {
            IsProcessing = true;

            var result = await _serviceUnitOfWork.UserGroupService.AssignUsersToGroup(
                UserGroup.GroupId,
                SelectedUserIds.ToList()
            );

            if (result.IsSuccessStatus)
            {
                var successMessage = result.Message ?? $"{SelectedUserIds.Count()} user(s) assigned to group successfully!";
                notificationService.Notify(NotificationSeverity.Success, "Success", successMessage);
                InitializeUserGroup();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/UserGroupList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadUserGroup();
        }
        else
        {
            InitializeUserGroup();
        }
        StateHasChanged();
    }

    protected List<string> GetSelectedUserNames()
    {
        if (SelectedUserIds == null || !SelectedUserIds.Any())
            return new List<string> { "Not Selected" };

        return Users.Where(u => SelectedUserIds.Contains(u.UserId))
                   .Select(u => u.UserName)
                   .ToList();
    }

    protected string GetSelectedGroupName()
    {
        if (IsEditMode && !string.IsNullOrEmpty(UserGroup.GroupName))
        {
            return UserGroup.GroupName;
        }

        var group = Groups.FirstOrDefault(g => g.GroupId == UserGroup.GroupId);
        return group?.GroupName ?? "Not Selected";
    }
}