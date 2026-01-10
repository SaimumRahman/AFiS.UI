using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Approval
{
    public partial class ApprovalLevelListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ApprovalLevelModelDTO> ApprovalLevelsGrid = default!;
        protected IEnumerable<ApprovalLevelModelDTO> ApprovalLevels { get; set; } = new List<ApprovalLevelModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadApprovalLevels();
        }

        private async Task LoadApprovalLevels()
        {
            try
            {
                IsLoading = true;
                ApprovalLevels = await _serviceUnitOfWork.ApprovalLevelService.GetApprovalLevels();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval levels: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddApprovalLevel()
        {
            NavigationManager.NavigateTo("/ApprovalLevelAdd");
        }

        protected void EditApprovalLevel(ApprovalLevelModelDTO level)
        {
            NavigationManager.NavigateTo($"/ApprovalLevelAdd/{level.Id}");
        }

        protected async Task DeleteApprovalLevel(ApprovalLevelModelDTO level)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete approval level '{level.LevelName}' (Level {level.LevelNumber})?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ApprovalLevelService.DeleteApprovalLevel(level.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Level deleted successfully.");
                    await LoadApprovalLevels();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete level.");
                }
            }
        }

        protected async Task ToggleStatus(ApprovalLevelModelDTO level)
        {
            var result = await _serviceUnitOfWork.ApprovalLevelService.ToggleApprovalLevelStatus(level.Id);
            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Status updated.");
                await LoadApprovalLevels();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to update status.");
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
            ApprovalLevelsGrid?.Dispose();
        }
    }
}