using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Shift;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Shift;

public partial class ShiftAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected ShiftDTO Shift { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Shift" : "Add New Shift";
    protected string PageIcon => IsEditMode ? "edit" : "business";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadShift();
        }
        else
        {
            InitializeShift();
        }
    }

    private async Task LoadShift()
    {
        try
        {
            IsLoading = true;
            var Shift = await _serviceUnitOfWork.ShiftService.GetShiftById(Id!.Value);

            if (Shift == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Shift not found.");
                NavigationManager.NavigateTo("/ShiftList");
                return;
            }

            Shift = Shift;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Shift: {ex.Message}");
            NavigationManager.NavigateTo("/ShiftList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeShift()
    {
        Shift = _serviceUnitOfWork.ShiftService.CreateNewShift();
    }

    protected async Task Save()
    {
        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int userId = 0;

        if (!string.IsNullOrEmpty(userObj.Value))
        {
            int.TryParse(userObj.Value, out userId);
        }

        if (IsEditMode)
        {
            Shift.ModifiedBy = userId.ToString();
        }
        else
        {
            Shift.CreatedBy = userId.ToString();
        }

        var validation = await _serviceUnitOfWork.ShiftService.ValidateShift(Shift);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ShiftService.SaveUpdateShift(Shift);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Shift updated successfully!" : "Shift created successfully!");
                NavigationManager.NavigateTo("/ShiftList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Shift: {ex.Message}");
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
        int userId = 0;

        if (!string.IsNullOrEmpty(userObj.Value))
        {
            int.TryParse(userObj.Value, out userId);
        }

        Shift.CreatedBy = userId.ToString();

        var validation = await _serviceUnitOfWork.ShiftService.ValidateShift(Shift);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ShiftService.SaveUpdateShift(Shift);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Shift created successfully!");
                InitializeShift();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Shift: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/ShiftList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadShift();
        }
        else
        {
            InitializeShift();
        }
        StateHasChanged();
    }
}