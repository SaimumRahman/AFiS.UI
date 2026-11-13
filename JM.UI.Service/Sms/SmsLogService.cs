using JM.UI.DataService.DAL.Sms;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Sms;
using System.Threading.Tasks;

namespace JM.UI.Service.Sms
{
    public interface ISmsLogService
    {
        Task<SmsLogPageResult> GetAsync(DateTime? fromDate, DateTime? toDate, string storeIds, string searchTerm, int page, int pageSize);
        Task<SmsLogDTO> GetByIdAsync(long id);
    }

    public class SmsLogService : ISmsLogService
    {
        private readonly IRepositoryUnitOfWork _uow;
        public SmsLogService(IRepositoryUnitOfWork uow) { _uow = uow; }
        public Task<SmsLogPageResult> GetAsync(DateTime? fromDate, DateTime? toDate, string storeIds, string searchTerm, int page, int pageSize)
            => _uow.SmsLogRepository.GetAsync(fromDate, toDate, storeIds, searchTerm, page, pageSize);
        public Task<SmsLogDTO> GetByIdAsync(long id) => _uow.SmsLogRepository.GetByIdAsync(id);
    }
}
