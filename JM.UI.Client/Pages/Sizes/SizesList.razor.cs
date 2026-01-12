
using JM.UI.Entities.Model.Sizes;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Sizes;

public partial class SizesListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<SizesDTO> SizessGrid = default!;
    protected IEnumerable<SizesDTO> Sizess { get; set; } = new List<SizesDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadSizess();
    }

    private async Task LoadSizess()
    {
        IsLoading = true;
        try
        {
            Sizess = await _serviceUnitOfWork.SizesService.GetSizess();
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

    protected void AddSizes() => NavigationManager.NavigateTo("/SizesAdd");

    protected void EditSizes(SizesDTO d)
        => NavigationManager.NavigateTo($"/SizesAdd/{d.Id}");

    protected async Task DeleteSizes(SizesDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete Sizes '{d.Name}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.SizesService.DeleteSizes(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadSizess();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.SizesService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => SizessGrid?.Dispose();
}