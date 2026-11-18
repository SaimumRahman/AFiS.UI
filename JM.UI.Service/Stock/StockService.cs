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
        DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId,
        int? groupId, int? subGroupId, int? designId, int? brandId,
        string? searchTerm = null)
    {
        try
        {
            return await _unitOfWork.StockRepository
                .GetStockLedger(fromDate, toDate, itemId, storeId,
                                groupId, subGroupId, designId, brandId, searchTerm);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ItemStatementDTO> GetItemStatement(
        string baseBarcode, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            return await _unitOfWork.StockRepository
                .GetItemStatement(baseBarcode, fromDate, toDate);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<StockAgingReportDTO> GetStockAging(int? groupId = null, int? brandId = null, string? agingBucket = null, int? sourceStoreId = null, string? search = null, bool includeHealthy = false)
    {
        try
        {
            return await _unitOfWork.StockRepository
                .GetStockAging(groupId, brandId, agingBucket, sourceStoreId, search, includeHealthy);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
