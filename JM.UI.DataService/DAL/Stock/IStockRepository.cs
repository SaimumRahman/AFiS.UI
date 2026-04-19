using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.StockReport_D;

namespace JM.UI.DataService.DAL.Stock;

public interface IStockRepository
{
    Task<IEnumerable<StockLedgerDTO>> GetStockLedger(
        DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId);
}