using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Approval
{
    public partial class DeliveryPendinglListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<PendingApprovalDTO> PendingApprovalGrid = default!;
        protected IEnumerable<PendingApprovalDTO> PendingApproval { get; set; } = new List<PendingApprovalDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPendingApproval();
        }

        private async Task LoadPendingApproval()
        {
            try
            {
                IsLoading = true;
                var result = await sessionStorage.GetAsync<string>("UserId");
                PendingApproval = await _serviceUnitOfWork.PendingApprovalService.GetAllPendingDelivery(Convert.ToInt32(result.Value));
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

        protected async Task DeleteApprovalLevel(PendingApprovalDTO level)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete approval level '{level.CurrentLevelName}' (Level {level.EntityDisplayName})?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ApprovalLevelService.DeleteApprovalLevel(level.PendingApprovalID);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Level deleted successfully.");
                    await LoadPendingApproval();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete level.");
                }
            }
        }

        protected async Task Approve(PendingApprovalDTO item)
        {
            var confirm = await dialogService.Confirm(
                $"Approve '{item.EntityDisplayName}' at level '{item.CurrentLevelName}'?",
                "Confirm Approval",
                new ConfirmOptions
                {
                    OkButtonText = "Approve",
                    CancelButtonText = "Cancel"
                });

            if (confirm != true)
                return;

            try
            {
                IsLoading = true;

                var result = await _serviceUnitOfWork
                    .PendingApprovalService
                    .Approve(item);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(
                        NotificationSeverity.Success,
                        "Approved",
                        result.Message ?? "Approval completed successfully.");

                    await LoadPendingApproval();
                }
                else
                {
                    notificationService.Notify(
                        NotificationSeverity.Error,
                        "Failed",
                        result.Message ?? "Approval failed.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    ex.Message);
            }
            finally
            {
                IsLoading = false;
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
            PendingApprovalGrid?.Dispose();
        }
    }
}