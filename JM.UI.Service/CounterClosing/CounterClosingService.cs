using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.CounterClosing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.CounterClosing
{
    public class CounterClosingService : ICounterClosingService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public CounterClosingService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CounterDTO>> GetCounters(int? storeId)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.GetCounters(storeId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CounterDTO>> GetCountersForManagement(int storeId)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.GetCountersForManagement(storeId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CanManageCounters(int userId, string? userName)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.CanManageCounters(userId, userName);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ResponseResult> SaveCounter(SaveCounterDTO counter)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.SaveCounter(counter);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<CounterClosingPreviewDTO?> GetPreview(int storeId, int counterId, DateTime? businessDate)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.GetPreview(storeId, counterId, businessDate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CounterClosingDTO>> GetHistory(int? counterId, int? storeId, int take)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.GetHistory(counterId, storeId, take);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> CloseCounter(SubmitCounterCloseDTO closing)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.CloseCounter(closing);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> ReopenCounter(ReopenCounterCloseDTO reopen)
        {
            try
            {
                return await _unitOfWork.CounterClosingRepository.ReopenCounter(reopen);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
