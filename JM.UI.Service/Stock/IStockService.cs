using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.StockReport_D;

namespace JM.UI.Service.Stock;

public interface IStockService
{
    Task<IEnumerable<StockLedgerDTO>> GetStockLedger(
     DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId);
}