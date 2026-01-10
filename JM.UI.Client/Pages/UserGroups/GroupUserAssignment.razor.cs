using JM.UI.Entities.Model.CoreUsers;
using JM.UI.Entities.Model.UserGroup;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.UserGroup;

public partial class GroupUserAssignmentComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int GroupId { get; set; }

    protected GroupUsersDTO GroupUsers { get; set; } = new();
    protected IList<CoreUserDTO> SelectedAvailableUsers { get; set; } = new List<CoreUserDTO>();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected string AvailableSearchText { get; set; } = string.Empty;
    protected string AssignedSearchText { get; set; } = string.Empty;

    protected IEnumerable<CoreUserDTO> FilteredAvailableUsers =>
        string.IsNullOrWhiteSpace(AvailableSearchText)
            ? GroupUsers.AvailableUsers
            : GroupUsers.AvailableUsers.Where(u =>
                (u.UserName?.Contains(AvailableSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.FullName?.Contains(AvailableSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Email?.Contains(AvailableSearchText, StringComparison.OrdinalIgnoreCase) ?? false));

    protected IEnumerable<CoreUserDTO> FilteredAssignedUsers =>
        string.IsNullOrWhiteSpace(AssignedSearchText)
            ? GroupUsers.AssignedUsers
            : GroupUsers.AssignedUsers.Where(u =>
                (u.UserName?.Contains(AssignedSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.FullName?.Contains(AssignedSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Email?.Contains(AssignedSearchText, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadGroupUsers();
    }

    protected async Task LoadGroupUsers()
    {
        try
        {
            IsLoading = true;
            GroupUsers = await _serviceUnitOfWork.UserGroupService.GetGroupUsersDetail(GroupId);
            SelectedAvailableUsers.Clear();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void SelectAllAvailable(bool? isSelected)
    {
        if (isSelected == true)
        {
            SelectedAvailableUsers = FilteredAvailableUsers.ToList();
        }
        else
        {
            SelectedAvailableUsers.Clear();
        }
        StateHasChanged();
    }

    protected void ToggleUserSelection(CoreUserDTO user, bool? isSelected, bool isAvailableList)
    {
        if (isAvailableList)
        {
            if (isSelected == true && !SelectedAvailableUsers.Contains(user))
            {
                SelectedAvailableUsers.Add(user);
            }
            else if (isSelected == false)
            {
                SelectedAvailableUsers.Remove(user);
            }
        }
        StateHasChanged();
    }

    protected async Task AssignSelectedUsers()
    {
        if (!SelectedAvailableUsers.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Warning", "Please select at least one user to assign");
            return;
        }

        var userIds = SelectedAvailableUsers.Select(u => u.UserId).ToList();

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.UserGroupService.AssignUsersToGroup(GroupId, userIds);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Users assigned successfully!");
                await LoadGroupUsers();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to assign users: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected async Task RemoveUser(CoreUserDTO user)
    {
        var confirm = await dialogService.Confirm(
            $"Are you sure you want to remove '{user.UserName}' from this group?",
            "Confirm Remove",
            new ConfirmOptions { OkButtonText = "Yes, Remove", CancelButtonText = "Cancel" });

        if (confirm == true)
        {
            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.UserGroupService.RemoveUserFromGroup(user.UserId, GroupId);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "User removed successfully!");
                    await LoadGroupUsers();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to remove user: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }

    protected void GoBack()
    {
        NavigationManager.NavigateTo("/GroupRoleList");
    }

    protected void ShowTooltip(ElementReference elementReference, string text)
    {
        TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
    }
}