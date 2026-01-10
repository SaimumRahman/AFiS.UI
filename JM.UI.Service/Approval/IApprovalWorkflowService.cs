using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Approval
{
    public interface IApprovalWorkflowService
    {
        Task<IEnumerable<ApprovalWorkflowModelDTO>> GetApprovalWorkflows();
        Task<ApprovalWorkflowModelDTO?> GetApprovalWorkflowById(int id);
        Task<ResponseResult> SaveUpdateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow);
        Task<ResponseResult> DeleteApprovalWorkflow(int id);
        Task<ResponseResult> ToggleApprovalWorkflowStatus(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow);
        ApprovalWorkflowModelDTO CreateNewApprovalWorkflow();
        string GetStatusBadgeStyle(bool isActive);
        string Truncate(string? value, int maxChars);
    }
}
