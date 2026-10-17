using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Stock;

public partial class StockLedgerComponent : PosComponentBase, IDisposable
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<StockLedgerDTO> StockLedgerGrid = default!;
    protected IEnumerable<StockLedgerDTO> StockLedgerList = new List<StockLedgerDTO>();
    protected IEnumerable<StoreDTO> StoreList = new List<StoreDTO>();
    protected bool IsLoading;

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected int? SelectedStoreId { get; set; }
    protected int CurrentUserId { get; set; } = 0;

    // Any user whose group is flagged IsSystem (GroupsRole.IsSystem = 1) - plus the
    // built-in super admin - may pick any store here. Everyone else is pinned to the
    // store they are signed in to.
    protected bool CanSelectAnyStore { get; set; }
    protected bool IsStoreDropdownDisabled => !CanSelectAnyStore;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");

        // Must be resolved before LoadStores(), which branches on it.
        await ResolveStoreSelectionPermission();

        await LoadStores();
        await LoadStockLedger();
    }

    private async Task ResolveStoreSelectionPermission()
    {
        CanSelectAnyStore = CurrentUserId == 1;
        if (CanSelectAnyStore) return;

        try
        {
            CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                                    .GetSystemGroupCountByUserId(CurrentUserId) > 0;
        }
        catch (Exception ex)
        {
            // Fail closed: a failed lookup must not hand out cross-store visibility.
            CanSelectAnyStore = false;
            Console.WriteLine($"[DEBUG] ResolveStoreSelectionPermission failed: {ex.Message}");
        }
    }

    private async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores();

            if (CanSelectAnyStore)
            {
                StoreList = stores;
            }
            else
            {
                var currentStoreId = await GetLocalStorageInt("StoreId");
                StoreList = stores.Where(s => s.Id == currentStoreId).ToList();
                SelectedStoreId = StoreList.FirstOrDefault()?.Id;
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stores: {ex.Message}");
        }
    }

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(key);
            if (result.Success && int.TryParse(result.Value, out int value))
                return value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
        }

        return 0;
    }

    protected async Task LoadStockLedger()
    {
        try
        {
            IsLoading = true;
            StockLedgerList = await _serviceUnitOfWork.StockService
                .GetStockLedger(FromDate, ToDate, null, SelectedStoreId);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stock ledger: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        StockLedgerGrid?.Dispose();
    }
}
