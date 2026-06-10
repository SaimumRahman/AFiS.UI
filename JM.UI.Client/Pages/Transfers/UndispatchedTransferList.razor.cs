using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Transfer;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Transfer
{
    public partial class UndispatchedTransferListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<TransferMasterDTO> TransferGrid = default!;
        protected IEnumerable<TransferMasterDTO> Transfers { get; set; } = new List<TransferMasterDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected List<int> SelectedTransferIds { get; set; } = new();
        protected bool HasSelection => SelectedTransferIds.Any();
        protected bool IsLoading { get; set; } = false;
        protected int? StoreId { get; set; } = null;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            if(await GetUserInfoAsync() == 1)
            {
                await LoadStores();
                await LoadTransfers(0);
            }
            else
            {
                await LoadStores();
                int storeId = await GetLoggedInUserStoreId();
                Stores = Stores.Where(x => x.Id == storeId).ToList();
                StoreId = Stores.FirstOrDefault(x => x.Id == storeId)?.Id;
                await LoadTransfers(await GetLoggedInUserStoreId());
            }
        }
        // Add checkbox toggle method
        protected void ToggleSelection(int transferId, bool isChecked)
        {
            if (isChecked)
            {
                if (!SelectedTransferIds.Contains(transferId))
                    SelectedTransferIds.Add(transferId);
            }
            else
            {
                SelectedTransferIds.Remove(transferId);
            }
        }
        // Add select all toggle
        protected void ToggleSelectAll(bool isChecked)
        {
            SelectedTransferIds = isChecked
                ? Transfers.Select(t => t.TransferId).ToList()
                : new List<int>();
        }
        protected async Task UpdateDispatchStatus()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.TransferService.UpdateDispatchStatus(SelectedTransferIds, 1);

                if (result.IsSuccessStatus)
                {
                    SelectedTransferIds.Clear();
                    await LoadTransfers(StoreId ?? 0);
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message);
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Warning", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to update dispatch status: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }
        protected async Task OnStoreChanged(int storeId)
        {
            await LoadTransfers(storeId);
            StateHasChanged();
        }
        private async Task LoadStores()
        {
            try
            {
                Stores = await _serviceUnitOfWork.StoreService.GetStores();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load stores: {ex.Message}");
            }
        }

        private async Task LoadTransfers(int? storeId)
        {
            try
            {
                IsLoading = true;
                Transfers = await _serviceUnitOfWork.TransferService.GetUndispatchedTransfers(storeId ?? 0);
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

        protected BadgeStyle GetStatusBadgeStyle(int? statusId) => statusId switch
        {
            1 => BadgeStyle.Warning,   // Pending
            2 => BadgeStyle.Info,      // In Transit
            3 => BadgeStyle.Success,   // Received
            4 => BadgeStyle.Danger,    // Cancelled
            _ => BadgeStyle.Secondary
        };

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
        private async Task<int> GetUserInfoAsync()
        {
            var userIdResult = await _localStorage.GetAsync<string>("UserId");
            if (userIdResult.Success && !string.IsNullOrEmpty(userIdResult.Value))
                int.TryParse(userIdResult.Value, out var uid);
            return int.TryParse(userIdResult.Value, out var parsedUid) ? parsedUid : 0;
           
        }
        private async Task<int> GetLoggedInUserStoreId()
        {
            var storeIdResult = await _localStorage.GetAsync<string>("StoreId");
            return int.TryParse(storeIdResult.Success ? storeIdResult.Value : "0",
                                     out var parsedSid) ? parsedSid : 0;
        }
    }
}