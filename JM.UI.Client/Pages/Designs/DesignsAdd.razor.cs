using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Designs
{
    public partial class DesignsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected DesignModelDTO Design { get; set; } = new();
        protected IEnumerable<SubGroupModelDTO> SubGroups { get; set; } = new List<SubGroupModelDTO>();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Design" : "New Design";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();
            if (IsEditMode)
            {
                await LoadDesign();
            }
            else
            {
                Design.Code = (await _serviceUnitOfWork.DesignService.GetDesignCode()).Code;
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                SubGroups = await _serviceUnitOfWork.SubGroupService.GetSubGroups();
                
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

        private async Task LoadDesign()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.DesignService.GetDesignById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Design not found.");
                    NavigationManager.NavigateTo("/DesignsList");
                    return;
                }

                Design = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load design: {ex.Message}");
                NavigationManager.NavigateTo("/DesignsList");
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
                var result = await _serviceUnitOfWork.DesignService.SaveUpdateDesign(Design);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Design updated successfully!" : "Design created successfully!");
                    NavigationManager.NavigateTo("/DesignsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save design: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/DesignsList");
        }
    }
}
