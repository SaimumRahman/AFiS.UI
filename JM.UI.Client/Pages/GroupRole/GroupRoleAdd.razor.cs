using JM.UI.Entities.Model.GroupRole;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.GroupRole;

public partial class GroupRoleAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected GroupRoleDTO GroupRole { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Group Role" : "Add New Group Role";
    protected string PageIcon => IsEditMode ? "edit" : "group";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadGroupRole();
        }
        else
        {
            InitializeGroupRole();
        }
    }

    private async Task LoadGroupRole()
    {
        try
        {
            IsLoading = true;
            var groupRole = await _serviceUnitOfWork.GroupRoleService.GetGroupRoleById(Id!.Value);

            if (groupRole == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Group role not found.");
                NavigationManager.NavigateTo("/GroupRoleList");
                return;
            }

            GroupRole = groupRole;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group role: {ex.Message}");
            NavigationManager.NavigateTo("/GroupRoleList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeGroupRole()
    {
        GroupRole = _serviceUnitOfWork.GroupRoleService.CreateNewGroupRole();
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.GroupRoleService.ValidateGroupRole(GroupRole);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.GroupRoleService.SaveUpdateGroupRole(GroupRole);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Group role updated successfully!" : "Group role created successfully!");
                NavigationManager.NavigateTo("/GroupRoleList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save group role: {ex.Message}");
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
            await Save();
            return;
        }

        var validation = await _serviceUnitOfWork.GroupRoleService.ValidateGroupRole(GroupRole);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.GroupRoleService.SaveUpdateGroupRole(GroupRole);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Group role created successfully!");
                InitializeGroupRole();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save group role: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/GroupRoleList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadGroupRole();
        }
        else
        {
            InitializeGroupRole();
        }
        StateHasChanged();
    }
}