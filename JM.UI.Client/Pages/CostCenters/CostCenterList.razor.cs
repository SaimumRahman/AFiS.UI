using JM.UI.Entities.Model.CostCenters;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.CostCenter
{
    public partial class CostCenterListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CostCenterDTO> CostCentersGrid = default!;
        protected IEnumerable<CostCenterDTO> CostCenters { get; set; } = new List<CostCenterDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCostCenters();
        }

        private async Task LoadCostCenters()
        {
            try
            {
                IsLoading = true;
                CostCenters = await _serviceUnitOfWork.CostCenterService.GetCostCenters();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load cost centers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddCostCenter()
        {
            NavigationManager.NavigateTo("/CostCenterAdd");
        }

        protected void EditCostCenter(CostCenterDTO costCenter)
        {
            NavigationManager.NavigateTo($"/CostCenterAdd/{costCenter.BranchId}");
        }

        protected async Task DeleteCostCenter(CostCenterDTO costCenter)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to deactivate cost center '{costCenter.Name}'?",
                "Confirm Deactivate",
                new ConfirmOptions { OkButtonText = "Yes, Deactivate", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.CostCenterService.DeleteCostCenter(costCenter.BranchId);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Cost center deactivated successfully.");
                    await LoadCostCenters();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to deactivate cost center.");
                }
            }
        }

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            CostCentersGrid?.Dispose();
        }
    }
}