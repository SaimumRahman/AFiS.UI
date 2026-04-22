using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.StockReport_D;
using JM.UI.Service.Stores;

namespace JM.UI.Service.Stock;

public class StockService : IStockService
{
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public StockService(IRepositoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<StockLedgerDTO>> GetStockLedger(
        DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId)
    {
        try
        {
            return await _unitOfWork.StockRepository
                .GetStockLedger(fromDate, toDate, itemId, storeId);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
