using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.ViewModel;

namespace JM.UI.Service.Approval
{
    public interface IApprovalLevelService
    {
        Task<IEnumerable<ApprovalLevelModelDTO>> GetApprovalLevels();
        Task<IEnumerable<UserAuthDetailsDAO>> GetUser();
        Task<ApprovalLevelModelDTO?> GetApprovalLevelById(int id);
        Task<ResponseResult> SaveUpdateApprovalLevel(ApprovalLevelModelDTO approvalLevel);
        Task<ResponseResult> DeleteApprovalLevel(int id);
        Task<ResponseResult> ToggleApprovalLevelStatus(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateApprovalLevel(ApprovalLevelModelDTO approvalLevel);
        ApprovalLevelModelDTO CreateNewApprovalLevel();
        string GetStatusBadgeStyle(bool isActive);
        string Truncate(string? value, int maxChars);
    }
}
