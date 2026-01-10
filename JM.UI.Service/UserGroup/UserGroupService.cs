using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.UserGroup;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.UserGroup
{
    // Service
    public class UserGroupService : IUserGroupService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public UserGroupService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<UserGroupDTO>> GetAllUserGroups()
        {
            return await _repositoryUnitOfWork.UserGroupRepository.GetAllUserGroups();
        }

        public async Task<IEnumerable<UserGroupDTO>> GetUserGroupsByGroupId(int groupId)
        {
            return await _repositoryUnitOfWork.UserGroupRepository.GetUserGroupsByGroupId(groupId);
        }

        public async Task<GroupUsersDTO> GetGroupUsersDetail(int groupId)
        {
            return await _repositoryUnitOfWork.UserGroupRepository.GetGroupUsersDetail(groupId);
        }

        public async Task<ResponseResult> AssignUsersToGroup(int groupId, List<int> userIds)
        {
            if (groupId <= 0)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Invalid group ID"
                };
            }

            if (userIds == null || !userIds.Any())
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Please select at least one user"
                };
            }

            return await _repositoryUnitOfWork.UserGroupRepository.AssignUsersToGroup(groupId, userIds);
        }

        public async Task<ResponseResult> RemoveUserFromGroup(int userId, int groupId)
        {
            if (userId <= 0 || groupId <= 0)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Invalid user ID or group ID"
                };
            }

            return await _repositoryUnitOfWork.UserGroupRepository.RemoveUserFromGroup(userId, groupId);
        }
    }
}
