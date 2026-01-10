using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Bank;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Banks;

public partial class BanksListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<BanksDTO> BankssGrid = default!;
    protected IEnumerable<BanksDTO> Bankss { get; set; } = new List<BanksDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadBankss();
    }

    private async Task LoadBankss()
    {
        IsLoading = true;
        try
        {
            Bankss = await _serviceUnitOfWork.BanksService.GetBankss();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void AddBanks() => NavigationManager.NavigateTo("/BanksAdd");

    protected void EditBanks(BanksDTO d)
        => NavigationManager.NavigateTo($"/BanksAdd/{d.Id}");

    protected async Task DeleteBanks(BanksDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete Banks '{d.Name}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.BanksService.DeleteBanks(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadBankss();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.BanksService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => BankssGrid?.Dispose();
}