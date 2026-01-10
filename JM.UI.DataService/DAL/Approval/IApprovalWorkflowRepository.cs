using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Approval
{
    public interface IApprovalWorkflowRepository
    {
        Task<IEnumerable<ApprovalWorkflowModelDTO>> GetApprovalWorkflows();
        Task<ApprovalWorkflowModelDTO?> GetApprovalWorkflowById(int id);
        Task<ResponseResult> SaveUpdateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow);
        Task DeleteApprovalWorkflow(int id);
        Task ToggleApprovalWorkflowStatus(int id);
    }
}
