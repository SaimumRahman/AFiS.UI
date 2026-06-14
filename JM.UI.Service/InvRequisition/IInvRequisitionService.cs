using JM.Infrastructure.Models;
using JM.UI.Entities.Model.InvRequisition;

namespace JM.UI.Service.InvRequisition
{
    public interface IInvRequisitionService
    {
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAll();
        Task<InvRequisitionMasterDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(InvRequisitionMasterDTO requisition);
        Task<ResponseResult> Delete(int id);
        Task<(bool IsValid, string ErrorMessage)> Validate(InvRequisitionMasterDTO requisition);
        InvRequisitionMasterDTO CreateNew();
        string Truncate(string? value, int maxChars);
    }
}
