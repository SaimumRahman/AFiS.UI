using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.StockOpening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.StockOpenings
{
    public class StockOpeningService : IStockOpeningService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public StockOpeningService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseResult> InsertStockOpening(StockOpeningEntryDTO stockOpening)
        {
            try
            {
                return await _unitOfWork.StockOpeningRepository.InsertStockOpening(stockOpening);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<StockOpeningEntryDTO>> GetStockOpeningsList()
        {
            try
            {
                return await _unitOfWork.StockOpeningRepository.GetStockOpeningsList();
            }
            catch (Exception)
            {
                return new List<StockOpeningEntryDTO>();
            }
        }

        public async Task<int> GetNextReferenceNo(int storeId)
        {
            try
            {
                return await _unitOfWork.StockOpeningRepository.GetNextReferenceNo(storeId);
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}
