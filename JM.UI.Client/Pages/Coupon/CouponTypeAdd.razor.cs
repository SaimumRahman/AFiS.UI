using JM.UI.Entities.Model.Coupon;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Coupon;

public partial class CouponTypeAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CouponTypeDTO CouponType { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Coupon Type" : "Add New Coupon Type";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
            await LoadCouponType();
        else
            CouponType = _serviceUnitOfWork.CouponTypeService.CreateNew();
    }

    private async Task LoadCouponType()
    {
        try
        {
            IsLoading = true;
            var couponType = await _serviceUnitOfWork.CouponTypeService.GetById(Id!.Value);

            if (couponType == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Coupon type not found.");
                NavigationManager.NavigateTo("/CouponTypeList");
                return;
            }

            CouponType = couponType;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load coupon type: {ex.Message}");
            NavigationManager.NavigateTo("/CouponTypeList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.CouponTypeService.Validate(CouponType);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CouponTypeService.SaveUpdate(CouponType);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Coupon type updated successfully!" : "Coupon type created successfully!");
                NavigationManager.NavigateTo("/CouponTypeList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save coupon type: {ex.Message}");
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

        var validation = await _serviceUnitOfWork.CouponTypeService.Validate(CouponType);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CouponTypeService.SaveUpdate(CouponType);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Coupon type created successfully!");
                CouponType = _serviceUnitOfWork.CouponTypeService.CreateNew();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save coupon type: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/CouponTypeList");

    protected async Task Reset()
    {
        if (IsEditMode)
            await LoadCouponType();
        else
            CouponType = _serviceUnitOfWork.CouponTypeService.CreateNew();
        StateHasChanged();
    }
}
