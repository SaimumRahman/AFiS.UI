using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Store;

public partial class StoreAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected StoreDTO Store { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Store" : "Add New Store";
    protected string PageIcon => IsEditMode ? "edit" : "store";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadStore();
        }
        else
        {
            InitializeStore();
        }
    }

    private async Task LoadStore()
    {
        try
        {
            IsLoading = true;
            var store = await _serviceUnitOfWork.StoreService.GetStoreById(Id!.Value);

            if (store == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Store not found.");
                NavigationManager.NavigateTo("/StoreList");
                return;
            }

            Store = store;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load store: {ex.Message}");
            NavigationManager.NavigateTo("/StoreList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeStore()
    {
        Store = _serviceUnitOfWork.StoreService.CreateNewStore();
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
            Store.ModifiedBy = userId.ToString();
        }
        else
        {
            Store.CreatedBy = userId.ToString();
        }

        var validation = await _serviceUnitOfWork.StoreService.ValidateStore(Store);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.StoreService.SaveUpdateStore(Store);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Store updated successfully!" : "Store created successfully!");
                NavigationManager.NavigateTo("/StoreList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save store: {ex.Message}");
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

        Store.CreatedBy = userId.ToString();

        var validation = await _serviceUnitOfWork.StoreService.ValidateStore(Store);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.StoreService.SaveUpdateStore(Store);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Store created successfully!");
                InitializeStore();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save store: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/StoreList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadStore();
        }
        else
        {
            InitializeStore();
        }
        StateHasChanged();
    }
}