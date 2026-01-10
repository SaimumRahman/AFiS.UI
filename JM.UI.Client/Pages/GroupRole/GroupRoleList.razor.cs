using JM.UI.Entities.Model.GroupRole;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.GroupRole
{
    public partial class GroupRoleListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<GroupRoleDTO> GroupRolesGrid = default!;
        protected IEnumerable<GroupRoleDTO> GroupRoles { get; set; } = new List<GroupRoleDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadGroupRoles();
        }

        private async Task LoadGroupRoles()
        {
            try
            {
                IsLoading = true;
                GroupRoles = await _serviceUnitOfWork.GroupRoleService.GetGroupRoles();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group roles: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddGroupRole()
        {
            NavigationManager.NavigateTo("/GroupRoleAdd");
        }

        protected void EditGroupRole(GroupRoleDTO groupRole)
        {
            NavigationManager.NavigateTo($"/GroupRoleAdd/{groupRole.GroupId}");
        }

        protected async Task DeleteGroupRole(GroupRoleDTO groupRole)
        {
            if (groupRole.IsSystem)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "System groups cannot be deleted.");
                return;
            }

            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete group role '{groupRole.GroupName}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.GroupRoleService.DeleteGroupRole(groupRole.GroupId);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Group role deleted successfully.");
                    await LoadGroupRoles();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete group role.");
                }
            }
        }

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            GroupRolesGrid?.Dispose();
        }
    }
}