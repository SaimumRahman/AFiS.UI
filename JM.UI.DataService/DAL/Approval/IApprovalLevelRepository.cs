using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Approval
{
    public interface IApprovalLevelRepository
    {
        Task<IEnumerable<ApprovalLevelModelDTO>> GetApprovalLevels();
        Task<IEnumerable<UserAuthDetailsDAO>> GetUser();
        Task<ApprovalLevelModelDTO?> GetApprovalLevelById(int id);
        Task<ResponseResult> SaveUpdateApprovalLevel(ApprovalLevelModelDTO approvalLevel);
        Task DeleteApprovalLevel(int id);
        Task ToggleApprovalLevelStatus(int id);

    }
}
