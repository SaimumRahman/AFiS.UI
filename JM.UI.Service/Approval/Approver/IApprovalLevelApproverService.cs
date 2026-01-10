using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Approval.Approver
{
    public interface IApprovalLevelApproverService
    {
        Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApprovers();
        Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApproversByLevelId(int levelId);
        Task<ApprovalLevelApproverModelDTO?> GetApprovalLevelApproverById(int id);
        Task<ResponseResult> SaveUpdateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver);
        Task<ResponseResult> DeleteApprovalLevelApprover(int id);
        Task<ResponseResult> ToggleApprovalLevelApproverStatus(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver);
        ApprovalLevelApproverModelDTO CreateNewApprovalLevelApprover();
        string GetStatusBadgeStyle(bool isActive);
        string Truncate(string? value, int maxChars);
        Task<ApprovalLevelApproverModelDTO> IsExistApproval();
    }
}
