using JM.UI.Entities.Model.CostCenters;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.CostCenter;

public partial class CostCenterAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CostCenterDTO CostCenter { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Cost Center" : "Add New Cost Center";
    protected string PageIcon => IsEditMode ? "edit" : "account_balance";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadCostCenter();
        }
        else
        {
            InitializeCostCenter();
        }
    }

    private async Task LoadCostCenter()
    {
        try
        {
            IsLoading = true;
            var costCenter = await _serviceUnitOfWork.CostCenterService.GetCostCenterById(Id!.Value);

            if (costCenter == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Cost center not found.");
                NavigationManager.NavigateTo("/CostCenterList");
                return;
            }

            CostCenter = costCenter;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load cost center: {ex.Message}");
            NavigationManager.NavigateTo("/CostCenterList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeCostCenter()
    {
        CostCenter = _serviceUnitOfWork.CostCenterService.CreateNewCostCenter();
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.CostCenterService.ValidateCostCenter(CostCenter);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CostCenterService.SaveUpdateCostCenter(CostCenter);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Cost center updated successfully!" : "Cost center created successfully!");
                NavigationManager.NavigateTo("/CostCenterList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save cost center: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/CostCenterList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadCostCenter();
        else InitializeCostCenter();
        StateHasChanged();
    }
}