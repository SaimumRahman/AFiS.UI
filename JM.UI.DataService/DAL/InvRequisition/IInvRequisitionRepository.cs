using JM.Infrastructure.Models;
using JM.UI.Entities.Model.InvRequisition;

namespace JM.UI.DataService.DAL.InvRequisition
{
    public interface IInvRequisitionRepository
    {
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAll();
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAllByStoreId(int storeId);
        Task<InvRequisitionMasterDTO?> GetById(int id);
        Task<ResponseResult> InsertUpdate(InvRequisitionMasterDTO requisition);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<RequisitionStatusDTO>> GetRequisitionStatuses();
        Task<ResponseResult> UpdateRequisitionStatus(int RequisitionId, int StatusId, int StatusBy, string StatusComments);
    }
}
