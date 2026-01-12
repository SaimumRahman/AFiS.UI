// ShiftListComponent.razor.cs
using JM.UI.Entities.Model.Shift;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Shift
{
    public partial class ShiftListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ShiftDTO> ShiftGrid = default!;
        protected IEnumerable<ShiftDTO> Shift { get; set; } = new List<ShiftDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadShift();
        }

        private async Task LoadShift()
        {
            try
            {
                IsLoading = true;
                StateHasChanged();

                Shift = await _serviceUnitOfWork.ShiftService.GetShift();

                if (Shift == null || !Shift.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "No Data", "No shifts found");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", $"Loaded {Shift.Count()} shifts");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Shift: {ex.Message}");
                Console.WriteLine($"UI Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddShift()
        {
            NavigationManager.NavigateTo("/ShiftAdd");
        }

        protected void EditShift(ShiftDTO Shift)
        {
            NavigationManager.NavigateTo($"/ShiftAdd/{Shift.Id}");
        }

        protected async Task DeleteShift(ShiftDTO Shift)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete Shift '{Shift.Name}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ShiftService.DeleteShift(Shift.Id);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Shift deleted successfully.");
                    await LoadShift();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete Shift.");
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

        public void Dispose()
        {
            ShiftGrid?.Dispose();
        }
    }
}