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

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        StoreList = await _serviceUnitOfWork.StoreService.GetStores();
        await LoadStockLedger();
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
