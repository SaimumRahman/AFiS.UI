using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.ViewModel;

namespace JM.UI.Service.Approval
{
    public class ApprovalLevelService : IApprovalLevelService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;
        public ApprovalLevelService(IRepositoryUnitOfWork repositoryUnitOfWork) => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ApprovalLevelModelDTO>> GetApprovalLevels()
        {
            var approvalLevels = await _repositoryUnitOfWork.ApprovalLevelRepository.GetApprovalLevels();
            return approvalLevels.Select(al => new ApprovalLevelModelDTO
            {
                Id = al.Id,
                WorkflowID = al.WorkflowID,
                LevelNumber = al.LevelNumber,
                LevelName = al.LevelName,
                LevelDescription = al.LevelDescription,
                IsParallelApproval = al.IsParallelApproval,
                RequiredApprovers = al.RequiredApprovers,
                IsActive = al.IsActive,
                CreatedBy = al.CreatedBy,
                CreatedDate = al.CreatedDate,
                LastModifiedDate = al.LastModifiedDate,
                LastModifiedBy = al.LastModifiedBy,
                WorkflowName = al.WorkflowName
            }).ToList();
        }
        public async Task<IEnumerable<UserAuthDetailsDAO>> GetUser()
        {
            var approvalLevels = await _repositoryUnitOfWork.ApprovalLevelRepository.GetUser();
            return approvalLevels.Select(al => new UserAuthDetailsDAO
            {
                UserId = al.UserId,
                UserName = al.UserName
            }).ToList();
        }

        public async Task<ApprovalLevelModelDTO?> GetApprovalLevelById(int id)
        {
            return await _repositoryUnitOfWork.ApprovalLevelRepository.GetApprovalLevelById(id);
        }

        public async Task<ResponseResult> SaveUpdateApprovalLevel(ApprovalLevelModelDTO approvalLevel)
        {
            var validation = await ValidateApprovalLevel(approvalLevel);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (approvalLevel.Id == 0)
            {
                approvalLevel.CreatedDate = DateTime.Now;
            }
            else
            {
                approvalLevel.LastModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork.ApprovalLevelRepository.SaveUpdateApprovalLevel(approvalLevel);
        }

        public async Task<ResponseResult> DeleteApprovalLevel(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalLevelRepository.DeleteApprovalLevel(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approval level deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete approval level: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> ToggleApprovalLevelStatus(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ApprovalLevelRepository.ToggleApprovalLevelStatus(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Approval level status updated successfully!"
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

        public Task<(bool IsValid, string ErrorMessage)> ValidateApprovalLevel(ApprovalLevelModelDTO approvalLevel)
        {
            if (approvalLevel.WorkflowID <= 0)
                return Task.FromResult((false, "Workflow is required."));

            if (approvalLevel.LevelNumber <= 0)
                return Task.FromResult((false, "Level number must be greater than 0."));

            if (string.IsNullOrWhiteSpace(approvalLevel.LevelName))
                return Task.FromResult((false, "Level name is required."));

            if (approvalLevel.LevelName.Length > 100)
                return Task.FromResult((false, "Level name cannot exceed 100 characters."));

            if (!string.IsNullOrWhiteSpace(approvalLevel.LevelDescription) && approvalLevel.LevelDescription.Length > 500)
                return Task.FromResult((false, "Level description cannot exceed 500 characters."));

            if (approvalLevel.RequiredApprovers <= 0)
                return Task.FromResult((false, "Required approvers must be greater than 0."));

            return Task.FromResult((true, string.Empty));
        }

        public ApprovalLevelModelDTO CreateNewApprovalLevel()
        {
            return new ApprovalLevelModelDTO
            {
                IsActive = true,
                CreatedDate = DateTime.Now,
                IsParallelApproval = false,
                RequiredApprovers = 1,
                LevelNumber = 1
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
