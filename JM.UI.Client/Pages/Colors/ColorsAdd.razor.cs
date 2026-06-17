using JM.UI.Entities.Model.Colors;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Colors;

public partial class ColorsAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? Id { get; set; }

    protected ColorsDTO Colors { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Colors" : "Add New Colors";
    protected string PageIcon => IsEditMode ? "edit" : "work";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
            await LoadColors();
        //else
        //    InitializeColors();
    }

    private async Task LoadColors()
    {
        try
        {
            IsLoading = true;
            var result = await _serviceUnitOfWork.ColorsService.GetColorsById(Id!.Value);
            if (result == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Colors not found.");
                NavigationManager.NavigateTo("/ColorsList");
                return;
            }
            Colors = result;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Colors: {ex.Message}");
            NavigationManager.NavigateTo("/ColorsList");
        }
        finally { IsLoading = false; }
    }

    //private void InitializeColors()
    //{
    //    Colors = _serviceUnitOfWork.ColorsService.CreateNewColors();
    //}

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.ColorsService.ValidateColors(Colors);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ColorsService.SaveUpdateColors(Colors);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Colors updated successfully!" : "Colors created successfully!");
                NavigationManager.NavigateTo("/ColorsList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Colors: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.ColorsService.ValidateColors(Colors);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ColorsService.SaveUpdateColors(Colors);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Colors created successfully!");
                //InitializeColors();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Colors: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/ColorsList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadColors();
        StateHasChanged();
    }
}