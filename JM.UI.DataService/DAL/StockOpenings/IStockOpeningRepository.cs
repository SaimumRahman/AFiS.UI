using JM.Infrastructure.Models;
using JM.UI.Entities.Model.StockOpening;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.StockOpenings
{
    public interface IStockOpeningRepository
    {
        Task<ResponseResult> InsertStockOpening(StockOpeningEntryDTO stockOpening);
        Task<IEnumerable<StockOpeningEntryDTO>> GetStockOpeningsList();
        Task<int> GetNextReferenceNo(int storeId);
    }
}
