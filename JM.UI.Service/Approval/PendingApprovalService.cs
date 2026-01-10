using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Approval
{
    public class PendingApprovalService : IPendingApprovalService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public PendingApprovalService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<PendingApprovalDTO>> GetAll(int currentUser)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetAll(currentUser);
        }
        public async Task<IEnumerable<PendingApprovalDTO>> GetAllPendingDelivery(int currentUser)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetAllPendingDelivery(currentUser);
        }
        public async Task<IEnumerable<PendingApprovalDTO>> GetAllPendingPickup(int currentUser)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetAllPendingPickup(currentUser);
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByWorkflowId(int workflowId)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetByWorkflowId(workflowId);
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByEntityId(int entityId)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetByEntityId(entityId);
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByApproverUserId(int approverUserId)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetByApproverUserId(approverUserId);
        }

        public async Task<PendingApprovalDTO?> GetById(int id)
        {
            return await _repositoryUnitOfWork.PendingApprovalRepository.GetById(id);
        }

        public async Task<ResponseResult> SaveUpdate(PendingApprovalDTO pendingApproval)
        {
            var validation = await ValidatePendingApproval(pendingApproval);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (pendingApproval.PendingApprovalID == 0)
            {
                pendingApproval.CreatedDate = DateTime.Now;
            }
            else
            {
                pendingApproval.LastModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork
                .PendingApprovalRepository
                .SaveUpdate(pendingApproval);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.PendingApprovalRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete pending approval: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> Approve(PendingApprovalDTO pendingApproval)
        {
            try
            {
                return await _repositoryUnitOfWork
                    .PendingApprovalRepository
                    .SaveUpdate(pendingApproval);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Approval failed: {ex.Message}"
                };
            }
        }



        public async Task<ResponseResult> MarkReminderSent(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.PendingApprovalRepository.MarkReminderSent(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to mark reminder sent: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> Deactivate(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.PendingApprovalRepository.Deactivate(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to deactivate pending approval: {ex.Message}"
                };
            }
        }

        #region Validation

        private Task<(bool IsValid, string ErrorMessage)> ValidatePendingApproval(PendingApprovalDTO pendingApproval)
        {
            if (pendingApproval.WorkflowID <= 0)
                return Task.FromResult((false, "Workflow is required."));

            if (pendingApproval.EntityId <= 0)
                return Task.FromResult((false, "Entity is required."));

            if (pendingApproval.ApproverUserID <= 0)
                return Task.FromResult((false, "Approver user is required."));

            if (pendingApproval.CurrentLevel <= 0)
                return Task.FromResult((false, "Approval level must be greater than zero."));

            return Task.FromResult((true, string.Empty));
        }

        #endregion

        #region UI Helpers (Optional)

        public string GetStatusBadgeStyle(bool isActive)
        {
            return isActive
                ? "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;"
                : "background-color: #f44336; color: white; padding: 4px 8px; border-radius: 4px;";
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars
                ? value.Substring(0, maxChars) + "..."
                : value ?? string.Empty;
        }

        #endregion
    }
}
