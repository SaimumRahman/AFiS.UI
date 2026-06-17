using JM.UI.Entities.Model.Sizes;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Sizes;

public partial class SizesAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? SizesID { get; set; }

    protected SizesDTO Sizes { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected bool IsEditMode => SizesID.HasValue && SizesID.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Sizes" : "Add New Sizes";
    protected string PageIcon => IsEditMode ? "edit" : "work";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
            await LoadSizes();
        //else
        //    InitializeSizes();
    }

    private async Task LoadSizes()
    {
        try
        {
            IsLoading = true;
            var Sizes = await _serviceUnitOfWork.SizesService.GetSizesById(SizesID!.Value);
            if (Sizes == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Sizes not found.");
                NavigationManager.NavigateTo("/SizesList");
                return;
            }
            Sizes = Sizes;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Sizes: {ex.Message}");
            NavigationManager.NavigateTo("/SizesList");
        }
        finally { IsLoading = false; }
    }

    //private void InitializeSizes()
    //{
    //    Sizes = _serviceUnitOfWork.SizesService.CreateNewSizes();
    //}

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.SizesService.ValidateSizes(Sizes);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.SizesService.SaveUpdateSizes(Sizes);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Sizes updated successfully!" : "Sizes created successfully!");
                NavigationManager.NavigateTo("/SizesList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Sizes: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.SizesService.ValidateSizes(Sizes);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.SizesService.SaveUpdateSizes(Sizes);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Sizes created successfully!");
                //InitializeSizes();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Sizes: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/SizesList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadSizes();
        //else InitializeSizes();
        StateHasChanged();
    }
}