using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Approval;

public partial class ApprovalWorkflowAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected ApprovalWorkflowModelDTO ApprovalWorkflow { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Approval Workflow" : "Add New Approval Workflow";
    protected string PageIcon => IsEditMode ? "edit" : "account_tree";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadApprovalWorkflow();
        }
        else
        {
            InitializeApprovalWorkflow();
        }
    }

    private async Task LoadApprovalWorkflow()
    {
        try
        {
            IsLoading = true;
            var approvalWorkflow = await _serviceUnitOfWork.ApprovalWorkflowService.GetApprovalWorkflowById(Id!.Value);

            if (approvalWorkflow == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Approval workflow not found.");
                NavigationManager.NavigateTo("/ApprovalWorkflowList");
                return;
            }

            ApprovalWorkflow = approvalWorkflow;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval workflow: {ex.Message}");
            NavigationManager.NavigateTo("/ApprovalWorkflowList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeApprovalWorkflow()
    {
        ApprovalWorkflow = _serviceUnitOfWork.ApprovalWorkflowService.CreateNewApprovalWorkflow();
    }

    protected async Task Save()
    {
        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int? userId = null;
        if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out int parsedUserId))
        {
            userId = parsedUserId;
        }
        ApprovalWorkflow.CreatedBy = 1;

        //if (IsEditMode)
        //{
        //    ApprovalWorkflow.LastModifiedBy = userId;
        //}
        //else
        //{
        //    ApprovalWorkflow.CreatedBy = userId;
        //}

        var validation = await _serviceUnitOfWork.ApprovalWorkflowService.ValidateApprovalWorkflow(ApprovalWorkflow);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ApprovalWorkflowService.SaveUpdateApprovalWorkflow(ApprovalWorkflow);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Approval workflow updated successfully!" : "Approval workflow created successfully!");
                NavigationManager.NavigateTo("/ApprovalWorkflowList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approval workflow: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode)
        {
            await Save();
            return;
        }

        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int? userId = null;
        if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        ApprovalWorkflow.CreatedBy = userId;

        var validation = await _serviceUnitOfWork.ApprovalWorkflowService.ValidateApprovalWorkflow(ApprovalWorkflow);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ApprovalWorkflowService.SaveUpdateApprovalWorkflow(ApprovalWorkflow);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Approval workflow created successfully!");
                InitializeApprovalWorkflow();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approval workflow: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/ApprovalWorkflowList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadApprovalWorkflow();
        }
        else
        {
            InitializeApprovalWorkflow();
        }
        StateHasChanged();
    }
}