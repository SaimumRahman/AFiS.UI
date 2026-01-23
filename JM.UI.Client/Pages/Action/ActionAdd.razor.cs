using JM.UI.Entities.Model.Actions;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Action;

public partial class ActionAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int ActionId { get; set; }

    protected ActionDTO ActionModel { get; set; } = new ActionDTO();
    protected bool IsSaving { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (ActionId > 0)
        {
            await LoadAction();
        }
    }

    private async Task LoadAction()
    {
        try
        {
            var action = await _serviceUnitOfWork.ActionService.GetActionById(ActionId);

            if (action != null)
            {
                ActionModel = action;
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "Action not found");
                GoBack();
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load action: {ex.Message}");
            GoBack();
        }
    }

    protected async Task HandleSubmit(ActionDTO model)
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            var result = ActionId > 0
                ? await _serviceUnitOfWork.ActionService.UpdateAction(model)
                : await _serviceUnitOfWork.ActionService.CreateAction(model);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(
                    NotificationSeverity.Success,
                    "Success",
                    result.Message ?? $"Action {(ActionId > 0 ? "updated" : "created")} successfully."
                );

                GoBack();
            }
            else
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    result.Message ?? $"Failed to {(ActionId > 0 ? "update" : "create")} action."
                );
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"An error occurred: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    protected void GoBack()
    {
        NavigationManager.NavigateTo("/ActionList");
    }
}