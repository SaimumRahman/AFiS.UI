using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Colors;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Colors;

public partial class ColorsListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<ColorsDTO> ColorssGrid = default!;
    protected IEnumerable<ColorsDTO> Colorss { get; set; } = new List<ColorsDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadColorss();
    }

    private async Task LoadColorss()
    {
        IsLoading = true;
        try
        {
            Colorss = await _serviceUnitOfWork.ColorsService.GetColorss();
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

    protected void AddColors() => NavigationManager.NavigateTo("/ColorsAdd");

    protected void EditColors(ColorsDTO d)
        => NavigationManager.NavigateTo($"/ColorsAdd/{d.Id}");

    protected async Task DeleteColors(ColorsDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete Colors '{d.Name}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.ColorsService.DeleteColors(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadColorss();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.ColorsService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => ColorssGrid?.Dispose();
}