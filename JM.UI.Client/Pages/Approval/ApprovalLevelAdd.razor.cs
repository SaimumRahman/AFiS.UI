using JM.UI.Entities.Model.Approval;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Approval;

public partial class ApprovalLevelAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected ApprovalLevelModelDTO ApprovalLevel { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Approval Level" : "Add New Approval Level";
    protected string PageIcon => IsEditMode ? "edit" : "approval";
    protected List<ApprovalWorkflowModelDTO> Workflows { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadWorkflows();

        if (IsEditMode)
        {
            await LoadApprovalLevel();
        }
        else
        {
            InitializeApprovalLevel();
        }
    }

    private async Task LoadWorkflows()
    {
        try
        {
            // Assuming you have a workflow service
             Workflows = (await _serviceUnitOfWork.ApprovalWorkflowService.GetApprovalWorkflows()).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load workflows: {ex.Message}");
        }
    }

    private async Task LoadApprovalLevel()
    {
        try
        {
            IsLoading = true;
            var approvalLevel = await _serviceUnitOfWork.ApprovalLevelService.GetApprovalLevelById(Id!.Value);

            if (approvalLevel == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Approval level not found.");
                NavigationManager.NavigateTo("/ApprovalLevelList");
                return;
            }

            ApprovalLevel = approvalLevel;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval level: {ex.Message}");
            NavigationManager.NavigateTo("/ApprovalLevelList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeApprovalLevel()
    {
        ApprovalLevel = _serviceUnitOfWork.ApprovalLevelService.CreateNewApprovalLevel();
    }

    protected async Task Save()
    {
        var userObj = await sessionStorage.GetAsync<string>("UserId");

        if (IsEditMode)
        {
            ApprovalLevel.LastModifiedBy = userObj.Value ?? "System";
        }
        else
        {
            ApprovalLevel.CreatedBy = userObj.Value ?? "System";
        }

        var validation = await _serviceUnitOfWork.ApprovalLevelService.ValidateApprovalLevel(ApprovalLevel);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ApprovalLevelService.SaveUpdateApprovalLevel(ApprovalLevel);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Approval level updated successfully!" : "Approval level created successfully!");
                NavigationManager.NavigateTo("/ApprovalLevelList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approval level: {ex.Message}");
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
        ApprovalLevel.CreatedBy = userObj.Value ?? "System";

        var validation = await _serviceUnitOfWork.ApprovalLevelService.ValidateApprovalLevel(ApprovalLevel);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ApprovalLevelService.SaveUpdateApprovalLevel(ApprovalLevel);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Approval level created successfully!");
                InitializeApprovalLevel();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approval level: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/ApprovalLevelList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadApprovalLevel();
        }
        else
        {
            InitializeApprovalLevel();
        }
        StateHasChanged();
    }
}

public class WorkflowDropdownItem
{
    public int WorkflowID { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
}