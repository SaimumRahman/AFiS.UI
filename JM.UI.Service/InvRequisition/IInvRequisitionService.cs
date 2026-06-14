using JM.Infrastructure.Models;
using JM.UI.Entities.Model.InvRequisition;

namespace JM.UI.Service.InvRequisition
{
    public interface IInvRequisitionService
    {
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAll();
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAllByStoreId(int storeId);
        Task<InvRequisitionMasterDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(InvRequisitionMasterDTO requisition);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<RequisitionStatusDTO>> GetRequisitionStatuses();
        Task<(bool IsValid, string ErrorMessage)> Validate(InvRequisitionMasterDTO requisition);
        InvRequisitionMasterDTO CreateNew();
        Task<ResponseResult> UpdateRequisitionStatus(int RequisitionId, int StatusId, int StatusBy, string StatusComments);
        string Truncate(string? value, int maxChars);
    }
}
