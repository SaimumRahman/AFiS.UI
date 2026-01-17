using JM.UI.Entities.Model.Groups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Groups
{
    public partial class GroupsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<GroupModelDTO> GroupsGrid = default!;
        protected IEnumerable<GroupModelDTO> GroupsList = new List<GroupModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadGroups();
        }

        protected async Task LoadGroups()
        {
            try
            {
                IsLoading = true;
                GroupsList = await _serviceUnitOfWork.GroupService.GetGroups();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load groups: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddGroup()
        {
            NavigationManager.NavigateTo("/GroupsAdd");
        }

        protected void EditGroup(GroupModelDTO group)
        {
            NavigationManager.NavigateTo($"/GroupsAdd/{group.Id}");
        }

        protected async Task DeleteGroup(GroupModelDTO group)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Group '{group.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.GroupService.DeleteGroup(group.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadGroups();
            }
        }

        public void Dispose()
        {
            GroupsGrid?.Dispose();
        }
    }
}
