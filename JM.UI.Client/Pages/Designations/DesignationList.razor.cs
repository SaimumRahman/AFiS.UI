using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Designations;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Designations;

public partial class DesignationListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<DesignationDTO> DesignationsGrid = default!;
    protected IEnumerable<DesignationDTO> Designations { get; set; } = new List<DesignationDTO>();
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadDesignations();
    }

    private async Task LoadDesignations()
    {
        IsLoading = true;
        try
        {
            Designations = await _serviceUnitOfWork.DesignationService.GetDesignations();
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

    protected void AddDesignation() => NavigationManager.NavigateTo("/DesignationAdd");

    protected void EditDesignation(DesignationDTO d)
        => NavigationManager.NavigateTo($"/DesignationAdd/{d.Id}");

    protected async Task DeleteDesignation(DesignationDTO d)
    {
        var confirm = await dialogService.Confirm(
            $"Delete designation '{d.Name}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel"
            });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.DesignationService.DeleteDesignation(d.Id);
            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus) await LoadDesignations();
        }
    }

    protected string Truncate(string? value, int maxChars)
        => _serviceUnitOfWork.DesignationService.Truncate(value, maxChars);

    protected void ShowTooltip(ElementReference el, string text)
        => TooltipService.Open(el, text);

    public void Dispose() => DesignationsGrid?.Dispose();
}