using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Approval.Approver
{
    public class ApprovalLevelApproverService : IApprovalLevelApproverService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ApprovalLevelApproverService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApprovers()
        {
            return await _repositoryUnitOfWork.ApprovalLevelApproverRepository.GetApprovalLevelApprovers();
        }
        public async Task<ApprovalLevelApproverModelDTO> IsExistApproval()
        {
            return await _repositoryUnitOfWork.ApprovalLevelApproverRepository.IsExistApproval();
        }

        public async Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApproversByLevelId(int levelId)
        {
            return await _repositoryUnitOfWork.ApprovalLevelApproverRepository.GetApprovalLevelApproversByLevelId(levelId);
        }

        public async Task<ApprovalLevelApproverModelDTO?> GetApprovalLevelApproverById(int id)
        {
            return await _repositoryUnitOfWork.ApprovalLevelApproverRepository.GetApprovalLevelApproverById(id);
        }

        public async Task<ResponseResult> SaveUpdateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver)
        {
            var validation = await ValidateApprovalLevelApprover(approver);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (approver.Id == 0)
            {
                approver.CreatedDate = DateTime.Now;
                approver.AssignedDate = DateTime.Now;
            }
            else
            {
                approver.LastModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork.ApprovalLevelApproverRepository.SaveUpdateApprovalLevelApprover(approver);
        }

        public async Task<ResponseResult> DeleteApprovalLevelApprover(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalLevelApproverRepository.DeleteApprovalLevelApprover(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approver deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete approver: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> ToggleApprovalLevelApproverStatus(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalLevelApproverRepository.ToggleApprovalLevelApproverStatus(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approver status updated successfully!"
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

        public Task<(bool IsValid, string ErrorMessage)> ValidateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver)
        {
            if (approver.ApprovalLevelID <= 0)
                return Task.FromResult((false, "Approval level is required."));

            //if (string.IsNullOrWhiteSpace(approver.UserID))
            //    return Task.FromResult((false, "User is required."));

            //if (approver.UserID.Length > 500)
            //    return Task.FromResult((false, "User ID cannot exceed 500 characters."));

            if (approver.ApproverOrder <= 0)
                return Task.FromResult((false, "Approver order must be greater than 0."));

            return Task.FromResult((true, string.Empty));
        }

        public ApprovalLevelApproverModelDTO CreateNewApprovalLevelApprover()
        {
            return new ApprovalLevelApproverModelDTO
            {
                IsActive = true,
                AssignedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ApproverOrder = 1
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
