using JM.UI.DataService.DAL.Sms;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Sms;
using JM.Infrastructure.Models;
using System.Threading.Tasks;

namespace JM.UI.Service.Sms
{
    public interface ISmsSettingsService
    {
        Task<SmsSettingsDTO> GetAsync();
        Task<ResponseResult> SaveAsync(SmsSettingsDTO dto, int modifiedBy);
        Task<SmsSendResultDTO> TestSendAsync(SmsSettingsDTO settings, string mobile, string message, int modifiedBy);
    }

    public class SmsSettingsService : ISmsSettingsService
    {
        private readonly IRepositoryUnitOfWork _uow;
        public SmsSettingsService(IRepositoryUnitOfWork uow) { _uow = uow; }

        public Task<SmsSettingsDTO> GetAsync() => _uow.SmsSettingsRepository.GetAsync();
        public Task<ResponseResult> SaveAsync(SmsSettingsDTO dto, int modifiedBy) => _uow.SmsSettingsRepository.SaveAsync(dto, modifiedBy);
        public Task<SmsSendResultDTO> TestSendAsync(SmsSettingsDTO settings, string mobile, string message, int modifiedBy)
            => _uow.SmsSettingsRepository.TestSendAsync(settings, mobile, message, modifiedBy);
    }
}
