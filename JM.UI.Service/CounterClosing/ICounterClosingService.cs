using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CounterClosing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.CounterClosing
{
    public interface ICounterClosingService
    {
        Task<IEnumerable<CounterDTO>> GetCounters(int? storeId);
        Task<IEnumerable<CounterDTO>> GetCountersForManagement(int storeId);
        Task<bool> CanManageCounters(int userId, string? userName);
        Task<ResponseResult> SaveCounter(SaveCounterDTO counter);
        Task<CounterClosingPreviewDTO?> GetPreview(int storeId, int counterId, DateTime? businessDate);
        Task<IEnumerable<CounterClosingDTO>> GetHistory(int? counterId, int? storeId, int take);
        Task<ResponseResult> CloseCounter(SubmitCounterCloseDTO closing);
        Task<ResponseResult> ReopenCounter(ReopenCounterCloseDTO reopen);
    }
}
