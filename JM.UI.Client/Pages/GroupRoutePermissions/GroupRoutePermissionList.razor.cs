using JM.UI.Entities.Model.GroupRoutePermission;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.GroupRoutePermission
{
    public partial class GroupRoutePermissionListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<GroupRoutePermissionModelDTO> GroupRoutePermissionsGrid = default!;
        protected IEnumerable<GroupRoutePermissionModelDTO> GroupRoutePermissions { get; set; } = new List<GroupRoutePermissionModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadGroupRoutePermissions();
        }

        private async Task LoadGroupRoutePermissions()
        {
            try
            {
                IsLoading = true;
                GroupRoutePermissions = await _serviceUnitOfWork.GroupRoutePermissionService.GetGroupRoutePermissions();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group route permissions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddGroupRoutePermission()
        {
            NavigationManager.NavigateTo("/GroupRoutePermissionAdd");
        }

        protected void EditGroupRoutePermission(GroupRoutePermissionModelDTO permission)
        {
            NavigationManager.NavigateTo($"/GroupRoutePermissionAdd/{permission.Id}");
        }

        protected async Task DeleteGroupRoutePermission(GroupRoutePermissionModelDTO permission)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete permission for Group ID '{permission.GroupId}' and Route ID '{permission.RouteId}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.GroupRoutePermissionService.DeleteGroupRoutePermission(permission.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Permission deleted successfully.");
                    await LoadGroupRoutePermissions();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete permission.");
                }
            }
        }

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            GroupRoutePermissionsGrid?.Dispose();
        }
    }
}