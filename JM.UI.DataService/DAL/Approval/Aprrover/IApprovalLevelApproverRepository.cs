using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Approval.Aprrover
{
    public interface IApprovalLevelApproverRepository
    {
        Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApprovers();
        Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApproversByLevelId(int levelId);
        Task<ApprovalLevelApproverModelDTO?> GetApprovalLevelApproverById(int id);
        Task<ResponseResult> SaveUpdateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver);
        Task DeleteApprovalLevelApprover(int id);
        Task ToggleApprovalLevelApproverStatus(int id);
        Task<ApprovalLevelApproverModelDTO> IsExistApproval();
    }
}
