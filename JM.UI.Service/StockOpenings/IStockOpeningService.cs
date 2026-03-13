using JM.Infrastructure.Models;
using JM.UI.Entities.Model.StockOpening;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.StockOpenings
{
    public interface IStockOpeningService
    {
        Task<ResponseResult> InsertStockOpening(StockOpeningEntryDTO stockOpening);
        Task<IEnumerable<StockOpeningEntryDTO>> GetStockOpeningsList();
        Task<int> GetNextReferenceNo(int storeId);
    }
}
