using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Accounting_D;
using System;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounting
{
    public class ManualJournalService : IManualJournalService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ManualJournalService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<ManualJournalContextDTO> GetContext(string companyCode, int? storeId, DateTime? entryDate)
            => await _repositoryUnitOfWork.ManualJournalRepository.GetContext(companyCode, storeId, entryDate);

        public async Task<ManualJournalListResultDTO> GetList(ManualJournalListRequestDTO request)
            => await _repositoryUnitOfWork.ManualJournalRepository.GetList(request);

        public async Task<ManualJournalDTO?> Get(long entryId)
            => await _repositoryUnitOfWork.ManualJournalRepository.Get(entryId);

        public async Task<ManualJournalSaveResultDTO> Save(ManualJournalSaveRequestDTO request)
            => await _repositoryUnitOfWork.ManualJournalRepository.Save(request);

        public async Task<ManualJournalSaveResultDTO> Post(long entryId, int userId)
            => await _repositoryUnitOfWork.ManualJournalRepository.Post(entryId, userId);

        public async Task<ManualJournalSaveResultDTO> Reverse(long entryId, int userId)
            => await _repositoryUnitOfWork.ManualJournalRepository.Reverse(entryId, userId);

        public async Task<ManualJournalSaveResultDTO> DeleteDraft(long entryId, int userId)
            => await _repositoryUnitOfWork.ManualJournalRepository.DeleteDraft(entryId, userId);
    }
}
