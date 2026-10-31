using JM.UI.Entities.Model.Accounting_D;
using System;
using System.Threading.Tasks;

namespace JM.UI.Service.Accounting
{
    public interface IManualJournalService
    {
        Task<ManualJournalContextDTO> GetContext(string companyCode, int? storeId, DateTime? entryDate);
        Task<ManualJournalListResultDTO> GetList(ManualJournalListRequestDTO request);
        Task<ManualJournalDTO?> Get(long entryId);
        Task<ManualJournalSaveResultDTO> Save(ManualJournalSaveRequestDTO request);
        Task<ManualJournalSaveResultDTO> Post(long entryId, int userId);
        Task<ManualJournalSaveResultDTO> Reverse(long entryId, int userId);
        Task<ManualJournalSaveResultDTO> DeleteDraft(long entryId, int userId);
    }
}
