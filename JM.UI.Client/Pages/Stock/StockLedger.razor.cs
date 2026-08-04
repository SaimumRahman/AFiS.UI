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
    protected bool IsStoreDropdownDisabled => CurrentUserId != 1;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        await LoadStores();
        await LoadStockLedger();
    }

    private async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores();

            if (CurrentUserId == 1)
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
