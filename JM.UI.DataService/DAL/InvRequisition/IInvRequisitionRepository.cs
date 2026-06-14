using JM.Infrastructure.Models;
using JM.UI.Entities.Model.InvRequisition;

namespace JM.UI.DataService.DAL.InvRequisition
{
    public interface IInvRequisitionRepository
    {
        Task<IEnumerable<InvRequisitionMasterDTO>> GetAll();
        Task<InvRequisitionMasterDTO?> GetById(int id);
        Task<ResponseResult> InsertUpdate(InvRequisitionMasterDTO requisition);
        Task<ResponseResult> Delete(int id);
    }
}
