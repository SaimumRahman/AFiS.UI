using JM.UI.Entities.Model.Actions;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Action;

public partial class ActionListComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<ActionDTO> ActionsGrid = default!;
    protected IEnumerable<ActionDTO> Actions { get; set; } = new List<ActionDTO>();
    protected bool IsLoading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadActions();
    }

    private async Task LoadActions()
    {
        try
        {
            IsLoading = true;
            Actions = await _serviceUnitOfWork.ActionService.GetAllActions();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load actions: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void AddAction()
    {
        NavigationManager.NavigateTo("/ActionAdd");
    }

    protected void EditAction(ActionDTO action)
    {
        NavigationManager.NavigateTo($"/ActionAdd/{action.ActionId}");
    }

    protected async Task DeleteAction(ActionDTO action)
    {
        var confirm = await dialogService.Confirm(
            $"Are you sure you want to delete action '{action.ActionKey}'?",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

        if (confirm == true)
        {
            var result = await _serviceUnitOfWork.ActionService.DeleteAction(action.ActionId);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Action deleted successfully.");
                await LoadActions();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete action.");
            }
        }
    }

    protected string Truncate(string? value, int maxChars)
        => string.IsNullOrWhiteSpace(value)
            ? ""
            : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

    protected void ShowTooltip(ElementReference elementReference, string text)
    {
        TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
    }

    protected string GetActionIcon(string? actionKey)
    {
        return actionKey?.ToUpper() switch
        {
            "CREATE" => "add_circle",
            "EDIT" => "edit",
            "DELETE" => "delete",
            "APPROVE" => "check_circle",
            "VIEW" => "visibility",
            "EXPORT" => "file_download",
            "IMPORT" => "file_upload",
            _ => "play_circle"
        };
    }

    protected string GetActionColor(string? actionKey)
    {
        return actionKey?.ToUpper() switch
        {
            "CREATE" => "var(--rz-success)",
            "EDIT" => "var(--rz-info)",
            "DELETE" => "var(--rz-danger)",
            "APPROVE" => "var(--rz-success)",
            "VIEW" => "var(--rz-primary)",
            "EXPORT" => "var(--rz-warning)",
            "IMPORT" => "var(--rz-warning)",
            _ => "var(--rz-primary)"
        };
    }

    protected BadgeStyle GetActionBadgeStyle(string? actionKey)
    {
        return actionKey?.ToUpper() switch
        {
            "CREATE" => BadgeStyle.Success,
            "EDIT" => BadgeStyle.Info,
            "DELETE" => BadgeStyle.Danger,
            "APPROVE" => BadgeStyle.Success,
            "VIEW" => BadgeStyle.Primary,
            "EXPORT" => BadgeStyle.Warning,
            "IMPORT" => BadgeStyle.Warning,
            _ => BadgeStyle.Light
        };
    }

    public void Dispose()
    {
        ActionsGrid?.Dispose();
    }
}