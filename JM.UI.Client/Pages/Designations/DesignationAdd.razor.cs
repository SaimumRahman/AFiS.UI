using JM.UI.Entities.Model.Designations;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Designations;

public partial class DesignationAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? DesignationID { get; set; }

    protected DesignationDTO Designation { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected bool IsEditMode => DesignationID.HasValue && DesignationID.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Designation" : "Add New Designation";
    protected string PageIcon => IsEditMode ? "edit" : "work";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
            await LoadDesignation();
        //else
        //    InitializeDesignation();
    }

    private async Task LoadDesignation()
    {
        try
        {
            IsLoading = true;
            var designation = await _serviceUnitOfWork.DesignationService.GetDesignationById(DesignationID!.Value);
            if (designation == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Designation not found.");
                NavigationManager.NavigateTo("/Designations");
                return;
            }
            Designation = designation;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load designation: {ex.Message}");
            NavigationManager.NavigateTo("/Designations");
        }
        finally { IsLoading = false; }
    }

    //private void InitializeDesignation()
    //{
    //    Designation = _serviceUnitOfWork.DesignationService.CreateNewDesignation();
    //}

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.DesignationService.ValidateDesignation(Designation);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.DesignationService.SaveUpdateDesignation(Designation);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Designation updated successfully!" : "Designation created successfully!");
                NavigationManager.NavigateTo("/Designations");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save designation: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.DesignationService.ValidateDesignation(Designation);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.DesignationService.SaveUpdateDesignation(Designation);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Designation created successfully!");
                //InitializeDesignation();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save designation: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/Designations");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadDesignation();
        //else InitializeDesignation();
        StateHasChanged();
    }
}