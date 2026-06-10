using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfer
{
    public partial class TransferListComponent : PosComponentBase
    {
        [Inject]
        public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject]
        public ProtectedLocalStorage _localStorage { get; set; } = default!;

        protected RadzenDataGrid<TransferMasterDTO> TransferGrid = default!;
        protected IEnumerable<TransferMasterDTO> Transfers { get; set; } = new List<TransferMasterDTO>();
        protected bool IsLoading { get; set; } = false;
        protected int CurrentUserId { get; set; } = 0;
        protected int CurrentStoreId { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadUserContext();
            await LoadTransfers();
        }

        // ── Read UserId and StoreId from localStorage ─────────────────
        private async Task LoadUserContext()
        {
            try
            {
                var userIdResult = await _localStorage.GetAsync<string>("UserId");
                var storeIdResult = await _localStorage.GetAsync<string>("StoreId");

                if (userIdResult.Success && !string.IsNullOrEmpty(userIdResult.Value))
                    int.TryParse(userIdResult.Value, out var uid);
                CurrentUserId = int.TryParse(userIdResult.Value, out var parsedUid) ? parsedUid : 0;

                CurrentStoreId = int.TryParse(storeIdResult.Success ? storeIdResult.Value : "0",
                                     out var parsedSid) ? parsedSid : 0;

                Console.WriteLine($"[TransferList] UserId={CurrentUserId}, StoreId={CurrentStoreId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransferList] LoadUserContext error: {ex.Message}");
            }
        }

        private async Task LoadTransfers()
        {
            try
            {
                IsLoading = true;

                // ✅ UserId = 1 → Head Office admin → load ALL transfers
                // ✅ Other users → load only their store's transfers
                if (CurrentUserId == 1 || CurrentStoreId == 4)
                {
                    Transfers = await _serviceUnitOfWork.TransferService.GetTransfers();
                }
                else
                {
                    Transfers = await _serviceUnitOfWork.TransferService.GetTransfersByStoreId(CurrentStoreId);
                }
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
                    var details = await _serviceUnitOfWork.TransferService
                                      .GetDetailsByTransferId(master.TransferId);
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
            NavigationManager.NavigateTo("/ItemTransferEntry");
        }

        protected void EditTransfer(TransferMasterDTO transfer)
        {
            NavigationManager.NavigateTo($"/ItemTransferEntry/{transfer.TransferId}");
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