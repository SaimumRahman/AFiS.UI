using JM.UI.Entities.Model.Designs;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Designs
{
    public partial class DesignsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<DesignModelDTO> DesignsGrid = default!;
        protected IEnumerable<DesignModelDTO> DesignsList = new List<DesignModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadDesigns();
        }

        protected async Task LoadDesigns()
        {
            try
            {
                IsLoading = true;
                DesignsList = await _serviceUnitOfWork.DesignService.GetDesigns();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load designs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddDesign()
        {
            NavigationManager.NavigateTo("/DesignsAdd");
        }

        protected void EditDesign(DesignModelDTO design)
        {
            NavigationManager.NavigateTo($"/DesignsAdd/{design.Id}");
        }

        protected async Task DeleteDesign(DesignModelDTO design)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Design '{design.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.DesignService.DeleteDesign(design.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadDesigns();
            }
        }

        public void Dispose()
        {
            DesignsGrid?.Dispose();
        }
    }
}
