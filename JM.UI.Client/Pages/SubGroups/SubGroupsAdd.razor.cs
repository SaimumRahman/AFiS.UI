using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SubGroups
{
    public partial class SubGroupsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected SubGroupModelDTO SubGroup { get; set; } = new();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit SubGroup" : "New SubGroup";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();
            if (IsEditMode)
            {
                await LoadSubGroup();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                Groups = await _serviceUnitOfWork.GroupService.GetGroups();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSubGroup()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.SubGroupService.GetSubGroupById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "SubGroup not found.");
                    NavigationManager.NavigateTo("/SubGroupsList");
                    return;
                }

                SubGroup = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load sub-group: {ex.Message}");
                NavigationManager.NavigateTo("/SubGroupsList");
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
                var result = await _serviceUnitOfWork.SubGroupService.SaveUpdateSubGroup(SubGroup);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "SubGroup updated successfully!" : "SubGroup created successfully!");
                    NavigationManager.NavigateTo("/SubGroupsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save sub-group: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/SubGroupsList");
        }
    }
}
