using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Approval
{
    public partial class ApprovalWorkflowListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ApprovalWorkflowModelDTO> ApprovalWorkflowsGrid = default!;
        protected IEnumerable<ApprovalWorkflowModelDTO> ApprovalWorkflows { get; set; } = new List<ApprovalWorkflowModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadApprovalWorkflows();
        }

        private async Task LoadApprovalWorkflows()
        {
            try
            {
                IsLoading = true;
                ApprovalWorkflows = await _serviceUnitOfWork.ApprovalWorkflowService.GetApprovalWorkflows();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval workflows: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddApprovalWorkflow()
        {
            NavigationManager.NavigateTo("/ApprovalWorkflowAdd");
        }

        protected void EditApprovalWorkflow(ApprovalWorkflowModelDTO workflow)
        {
            NavigationManager.NavigateTo($"/ApprovalWorkflowAdd/{workflow.Id}");
        }

        protected async Task DeleteApprovalWorkflow(ApprovalWorkflowModelDTO workflow)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete approval workflow '{workflow.WorkflowName}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ApprovalWorkflowService.DeleteApprovalWorkflow(workflow.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Workflow deleted successfully.");
                    await LoadApprovalWorkflows();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete workflow.");
                }
            }
        }

        protected async Task ToggleStatus(ApprovalWorkflowModelDTO workflow)
        {
            var result = await _serviceUnitOfWork.ApprovalWorkflowService.ToggleApprovalWorkflowStatus(workflow.Id);
            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Status updated.");
                await LoadApprovalWorkflows();
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
            ApprovalWorkflowsGrid?.Dispose();
        }
    }
}