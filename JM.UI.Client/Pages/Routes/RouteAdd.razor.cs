using JM.UI.Entities.Model.Routes;
using JM.UI.Service.Routes;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Components.Pages.Route
{
    public partial class RouteAddComponent : ComponentBase
    {
        [Inject] private IRouteService RouteService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected RouteModelDTO Route { get; set; } = new();
        protected RouteModelDTO OriginalRoute { get; set; } = new();

        protected bool IsLoading { get; set; } = false;
        protected bool IsProcessing { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;

        protected string PageTitle => IsEditMode ? "Edit Route" : "Add New Route";
        protected string PageIcon => IsEditMode ? "edit" : "add_circle_outline";

        protected override async Task OnInitializedAsync()
        {
            await LoadRoute();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id.HasValue && Id.Value > 0)
            {
                await LoadRoute();
            }
        }

        private async Task LoadRoute()
        {
            if (!Id.HasValue || Id.Value <= 0)
            {
                Route = new RouteModelDTO { IsActive = true };
                OriginalRoute = new RouteModelDTO { IsActive = true };
                return;
            }

            try
            {
                IsLoading = true;
                var result = await RouteService.GetRouteById(Id.Value);

                if (result != null)
                {
                    Route = result;
                    OriginalRoute = new RouteModelDTO
                    {
                        RouteId = result.RouteId,
                        RouteName = result.RouteName,
                        RoutePath = result.RoutePath,
                        IsActive = result.IsActive
                    };
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Route not found",
                        Duration = 4000
                    });
                    Cancel();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed to load route: {ex.Message}",
                    Duration = 4000
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;

                if (IsEditMode)
                {
                    var result = await RouteService.SaveUpdateRoute(Route);
                    if (result.IsSuccessStatus)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Success",
                            Detail = "Route updated successfully",
                            Duration = 4000
                        });
                        NavigationManager.NavigateTo("/RouteList");
                    }
                    else
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error",
                            Detail = "Failed to update route",
                            Duration = 4000
                        });
                    }
                }
                else
                {
                    var result = await RouteService.SaveUpdateRoute(Route);
                    if (result.IsSuccessStatus)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Success",
                            Detail = "Route created successfully",
                            Duration = 4000
                        });
                        NavigationManager.NavigateTo("/RouteList");
                    }
                    else
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error",
                            Detail = "Failed to create route",
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
            finally
            {
                IsProcessing = false;
            }
        }

        protected async Task SaveAndNew()
        {
            try
            {
                IsProcessing = true;

                var result = await RouteService.SaveUpdateRoute(Route);
                if (result.IsSuccessStatus)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Route created successfully",
                        Duration = 4000
                    });

                    // Reset form for new entry
                    Route = new RouteModelDTO { IsActive = true };
                    OriginalRoute = new RouteModelDTO { IsActive = true };
                    StateHasChanged();
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Failed to create route",
                        Duration = 4000
                    });
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
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Reset()
        {
            if (IsEditMode)
            {
                Route.RouteName = OriginalRoute.RouteName;
                Route.RoutePath = OriginalRoute.RoutePath;
                Route.IsActive = OriginalRoute.IsActive;
            }
            else
            {
                Route = new RouteModelDTO { IsActive = true };
            }

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Reset",
                Detail = "Form has been reset",
                Duration = 3000
            });
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/RouteList");
        }
    }
}