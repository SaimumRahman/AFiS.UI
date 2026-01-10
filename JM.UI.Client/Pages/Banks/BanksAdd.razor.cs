using JM.UI.Entities.Model.Bank;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Banks;

public partial class BanksAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Parameter] public int? BanksID { get; set; }

    protected BanksDTO Banks { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;

    protected bool IsEditMode => BanksID.HasValue && BanksID.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Banks" : "Add New Banks";
    protected string PageIcon => IsEditMode ? "edit" : "work";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
            await LoadBanks();
        //else
        //    InitializeBanks();
    }

    private async Task LoadBanks()
    {
        try
        {
            IsLoading = true;
            var Banks = await _serviceUnitOfWork.BanksService.GetBanksById(BanksID!.Value);
            if (Banks == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Banks not found.");
                NavigationManager.NavigateTo("/BanksList");
                return;
            }
            Banks = Banks;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Banks: {ex.Message}");
            NavigationManager.NavigateTo("/BanksList");
        }
        finally { IsLoading = false; }
    }

    //private void InitializeBanks()
    //{
    //    Banks = _serviceUnitOfWork.BanksService.CreateNewBanks();
    //}

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.BanksService.ValidateBanks(Banks);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.BanksService.SaveUpdateBanks(Banks);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Banks updated successfully!" : "Banks created successfully!");
                NavigationManager.NavigateTo("/BanksList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Banks: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var validation = await _serviceUnitOfWork.BanksService.ValidateBanks(Banks);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.BanksService.SaveUpdateBanks(Banks);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Banks created successfully!");
                //InitializeBanks();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save Banks: {ex.Message}");
        }
        finally { IsProcessing = false; }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/BanksList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadBanks();
        //else InitializeBanks();
        StateHasChanged();
    }
}