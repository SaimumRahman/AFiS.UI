using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Approval
{
    public partial class ApprovalLevelApproverListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ApprovalLevelApproverModelDTO> ApprovalLevelApproversGrid = default!;
        protected IEnumerable<ApprovalLevelApproverModelDTO> ApprovalLevelApprovers { get; set; } = new List<ApprovalLevelApproverModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadApprovalLevelApprovers();
        }

        private async Task LoadApprovalLevelApprovers()
        {
            try
            {
                IsLoading = true;
                ApprovalLevelApprovers = await _serviceUnitOfWork.ApprovalLevelApproverService.GetApprovalLevelApprovers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval level approvers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddApprovalLevelApprover()
        {
            NavigationManager.NavigateTo("/ApprovalLevelApproverAdd");
        }

        protected void EditApprovalLevelApprover(ApprovalLevelApproverModelDTO approver)
        {
            NavigationManager.NavigateTo($"/ApprovalLevelApproverAdd/{approver.Id}");
        }

        protected async Task DeleteApprovalLevelApprover(ApprovalLevelApproverModelDTO approver)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete approver",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ApprovalLevelApproverService.DeleteApprovalLevelApprover(approver.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Approver deleted successfully.");
                    await LoadApprovalLevelApprovers();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete approver.");
                }
            }
        }

        protected async Task ToggleStatus(ApprovalLevelApproverModelDTO approver)
        {
            var result = await _serviceUnitOfWork.ApprovalLevelApproverService.ToggleApprovalLevelApproverStatus(approver.Id);
            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Status updated.");
                await LoadApprovalLevelApprovers();
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
            ApprovalLevelApproversGrid?.Dispose();
        }
    }
}