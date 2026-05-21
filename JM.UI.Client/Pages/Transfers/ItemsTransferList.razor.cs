using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfer
{
    public partial class TransferListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<TransferMasterDTO> TransferGrid = default!;
        protected IEnumerable<TransferMasterDTO> Transfers { get; set; } = new List<TransferMasterDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadTransfers();
        }

        private async Task LoadTransfers()
        {
            try
            {
                IsLoading = true;
                Transfers = await _serviceUnitOfWork.TransferService.GetTransfers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load transfers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OnRowExpand(TransferMasterDTO master)
        {
            try
            {
                if (master.Details == null || !master.Details.Any())
                {
                    var details = await _serviceUnitOfWork.TransferService.GetDetailsByTransferId(master.TransferId);

                    master.Details = details.ToList();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load transfer details: {ex.Message}");
            }
        }

        protected void AddTransfer()
        {
            NavigationManager.NavigateTo("/TransferAdd");
        }

        protected void EditTransfer(TransferMasterDTO transfer)
        {
            NavigationManager.NavigateTo($"/TransferAdd/{transfer.TransferId}");
        }

        protected string GetStatusLabel(int? statusId) => statusId switch
        {
            1 => "Pending",
            2 => "In Transit",
            3 => "Received",
            4 => "Cancelled",
            _ => "Unknown"
        };

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? "—"
                : value.Length <= maxChars ? value : value[..maxChars] + "...";

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text,
                new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            TransferGrid?.Dispose();
        }
    }
}