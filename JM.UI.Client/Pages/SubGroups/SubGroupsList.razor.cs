using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SubGroups
{
    public partial class SubGroupsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<SubGroupModelDTO> SubGroupsGrid = default!;
        protected IEnumerable<SubGroupModelDTO> SubGroupsList = new List<SubGroupModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadSubGroups();
        }

        protected async Task LoadSubGroups()
        {
            try
            {
                IsLoading = true;
                SubGroupsList = await _serviceUnitOfWork.SubGroupService.GetSubGroups();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load sub-groups: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddSubGroup()
        {
            NavigationManager.NavigateTo("/SubGroupsAdd");
        }

        protected void EditSubGroup(SubGroupModelDTO subGroup)
        {
            NavigationManager.NavigateTo($"/SubGroupsAdd/{subGroup.Id}");
        }

        protected async Task DeleteSubGroup(SubGroupModelDTO subGroup)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete SubGroup '{subGroup.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.SubGroupService.DeleteSubGroup(subGroup.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadSubGroups();
            }
        }

        public void Dispose()
        {
            SubGroupsGrid?.Dispose();
        }
    }
}
