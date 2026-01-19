using JM.UI.Entities.Model.Routes;
using JM.UI.Service.Routes;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Components.Pages.Route
{
    public partial class RouteListComponent : ComponentBase
    {
        [Inject] private IRouteService RouteService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private TooltipService TooltipService { get; set; } = default!;

        protected RadzenDataGrid<RouteModelDTO> RoutesGrid { get; set; } = default!;
        protected IEnumerable<RouteModelDTO> Routes { get; set; } = new List<RouteModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadRoutes();
        }

        private async Task LoadRoutes()
        {
            try
            {
                IsLoading = true;
                var result = await RouteService.GetRoutes();
                Routes = result ?? new List<RouteModelDTO>();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed to load routes: {ex.Message}",
                    Duration = 4000
                });
                Routes = new List<RouteModelDTO>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void AddRoute()
        {
            NavigationManager.NavigateTo("/RouteAdd");
        }

        protected void EditRoute(RouteModelDTO route)
        {
            NavigationManager.NavigateTo($"/RouteAdd/{route.RouteId}");
        }

        protected async Task ToggleStatus(RouteModelDTO route)
        {
            try
            {
                var confirmResult = await DialogService.Confirm(
                    $"Are you sure you want to {(route.IsActive ? "deactivate" : "activate")} the route '{route.RouteName}'?",
                    "Confirm Status Change",
                    new ConfirmOptions
                    {
                        OkButtonText = "Yes",
                        CancelButtonText = "No",
                        AutoFocusFirstElement = true
                    }
                );

                if (confirmResult == true)
                {
                    route.IsActive = !route.IsActive;
                    var result = await RouteService.SaveUpdateRoute(route);

                    if (result.IsSuccessStatus)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Success",
                            Detail = $"Route '{route.RouteName}' has been {(route.IsActive ? "activated" : "deactivated")} successfully",
                            Duration = 4000
                        });
                        await LoadRoutes();
                        await RoutesGrid.Reload();
                    }
                    else
                    {
                        // Revert the change if update failed
                        route.IsActive = !route.IsActive;
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error",
                            Detail = "Failed to update route status",
                            Duration = 4000
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"An error occurred: {ex.Message}",
                    Duration = 4000
                });
            }
        }

        protected async Task DeleteRoute(RouteModelDTO route)
        {
            try
            {
                var confirmResult = await DialogService.Confirm(
                    $"Are you sure you want to delete the route '{route.RouteName}'? This action cannot be undone.",
                    "Confirm Delete",
                    new ConfirmOptions
                    {
                        OkButtonText = "Yes, Delete",
                        CancelButtonText = "Cancel",
                        AutoFocusFirstElement = true
                    }
                );

                if (confirmResult == true)
                {
                    var result = await RouteService.DeleteRoute(route.RouteId);

                    if (result.IsSuccessStatus)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Success",
                            Detail = $"Route '{route.RouteName}' has been deleted successfully",
                            Duration = 4000
                        });
                        await LoadRoutes();
                        await RoutesGrid.Reload();
                    }
                    else
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error",
                            Detail = "Failed to delete route",
                            Duration = 4000
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"An error occurred: {ex.Message}",
                    Duration = 4000
                });
            }
        }

        protected void ShowTooltip(ElementReference elementReference, string message)
        {
            TooltipService.Open(elementReference, message, new TooltipOptions
            {
                Position = TooltipPosition.Top,
                Duration = 2000
            });
        }
    }
}