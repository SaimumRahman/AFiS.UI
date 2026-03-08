using JM.UI.Entities.Model.Groups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Groups
{
    public partial class GroupsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected GroupModelDTO Group { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Group" : "New Group";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            if (IsEditMode)
            {
                await LoadGroup();
            }
        }

        private async Task LoadGroup()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.GroupService.GetGroupById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Group not found.");
                    NavigationManager.NavigateTo("/GroupsList");
                    return;
                }

                Group = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load group: {ex.Message}");
                NavigationManager.NavigateTo("/GroupsList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.GroupService.SaveUpdateGroup(Group);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Group updated successfully!" : "Group created successfully!");
                    NavigationManager.NavigateTo("/GroupsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save group: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/GroupsList");
        }
    }
}
