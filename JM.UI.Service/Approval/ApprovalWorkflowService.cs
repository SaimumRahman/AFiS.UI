using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Approval
{
    public class ApprovalWorkflowService : IApprovalWorkflowService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ApprovalWorkflowService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ApprovalWorkflowModelDTO>> GetApprovalWorkflows()
        {
            return await _repositoryUnitOfWork.ApprovalWorkflowRepository.GetApprovalWorkflows();
        }

        public async Task<ApprovalWorkflowModelDTO?> GetApprovalWorkflowById(int id)
        {
            return await _repositoryUnitOfWork.ApprovalWorkflowRepository.GetApprovalWorkflowById(id);
        }

        public async Task<ResponseResult> SaveUpdateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow)
        {
            var validation = await ValidateApprovalWorkflow(approvalWorkflow);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (approvalWorkflow.Id == 0)
            {
                approvalWorkflow.CreatedDate = DateTime.Now;
            }
            else
            {
                approvalWorkflow.LastModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork.ApprovalWorkflowRepository.SaveUpdateApprovalWorkflow(approvalWorkflow);
        }

        public async Task<ResponseResult> DeleteApprovalWorkflow(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalWorkflowRepository.DeleteApprovalWorkflow(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approval workflow deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete approval workflow: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> ToggleApprovalWorkflowStatus(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalWorkflowRepository.ToggleApprovalWorkflowStatus(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approval workflow status updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to update status: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow)
        {
            if (string.IsNullOrWhiteSpace(approvalWorkflow.WorkflowName))
                return Task.FromResult((false, "Workflow name is required."));

            if (approvalWorkflow.WorkflowName.Length > 200)
                return Task.FromResult((false, "Workflow name cannot exceed 200 characters."));

            if (!string.IsNullOrWhiteSpace(approvalWorkflow.EntityType) && approvalWorkflow.EntityType.Length > 100)
                return Task.FromResult((false, "Entity type cannot exceed 100 characters."));

            if (approvalWorkflow.TotalLevels <= 0)
                return Task.FromResult((false, "Total levels must be greater than 0."));

            return Task.FromResult((true, string.Empty));
        }

        public ApprovalWorkflowModelDTO CreateNewApprovalWorkflow()
        {
            return new ApprovalWorkflowModelDTO
            {
                IsActive = true,
                CreatedDate = DateTime.Now,
                EntityType = "Profile",
                TotalLevels = 1
            };
        }

        public string GetStatusBadgeStyle(bool isActive)
        {
            return isActive
                ? "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;"
                : "background-color: #f44336; color: white; padding: 4px 8px; border-radius: 4px;";
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
