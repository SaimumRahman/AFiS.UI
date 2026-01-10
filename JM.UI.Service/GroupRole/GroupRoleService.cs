using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.GroupRole;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.GroupRole
{
    public class GroupRoleService : IGroupRoleService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public GroupRoleService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<GroupRoleDTO>> GetGroupRoles()
        {
            var groupRoles = await _repositoryUnitOfWork.GroupRoleRepository.GetGroupRoles();
            return groupRoles.Select(g => new GroupRoleDTO
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                Description = g.Description,
                IsSystem = g.IsSystem,
                CreatedAt = g.CreatedAt
            }).ToList();
        }

        public async Task<GroupRoleDTO?> GetGroupRoleById(int id)
        {
            return await _repositoryUnitOfWork.GroupRoleRepository.GetGroupRoleById(id);
        }

        public async Task<ResponseResult> SaveUpdateGroupRole(GroupRoleDTO groupRole)
        {
            var validation = await ValidateGroupRole(groupRole);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (groupRole.GroupId == 0)
            {
                groupRole.CreatedAt = DateTime.Now;
            }

            return await _repositoryUnitOfWork.GroupRoleRepository.SaveUpdateGroupRole(groupRole);
        }

        public async Task<ResponseResult> DeleteGroupRole(int id)
        {
            try
            {
                await _repositoryUnitOfWork.GroupRoleRepository.DeleteGroupRole(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Group role deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete group role: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateGroupRole(GroupRoleDTO groupRole)
        {
            if (string.IsNullOrWhiteSpace(groupRole.GroupName))
                return Task.FromResult((false, "Group name is required."));

            if (groupRole.GroupName.Length > 100)
                return Task.FromResult((false, "Group name cannot exceed 100 characters."));

            if (!string.IsNullOrWhiteSpace(groupRole.Description) && groupRole.Description.Length > 250)
                return Task.FromResult((false, "Description cannot exceed 250 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public GroupRoleDTO CreateNewGroupRole()
        {
            return new GroupRoleDTO
            {
                CreatedAt = DateTime.Now,
                IsSystem = false
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
