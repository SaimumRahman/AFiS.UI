using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Approval
{
    public interface IPendingApprovalRepository
    {
        Task<ResponseResult> Deactivate(int id);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<PendingApprovalDTO>> GetAll(int currentUser);
        Task<IEnumerable<PendingApprovalDTO>> GetAllPendingPickup(int currentUser);
        Task<IEnumerable<PendingApprovalDTO>> GetAllPendingDelivery(int currentUser);
        Task<IEnumerable<PendingApprovalDTO>> GetByApproverUserId(int approverUserId);
        Task<IEnumerable<PendingApprovalDTO>> GetByEntityId(int entityId);
        Task<PendingApprovalDTO?> GetById(int id);
        Task<IEnumerable<PendingApprovalDTO>> GetByWorkflowId(int workflowId);
        Task<ResponseResult> MarkReminderSent(int id);
        Task<ResponseResult> SaveUpdate(PendingApprovalDTO command);
    }
}