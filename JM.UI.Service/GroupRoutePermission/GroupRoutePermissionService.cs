using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.GroupRoutePermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.UI.Service.GroupRoutePermission
{
    public class GroupRoutePermissionService : IGroupRoutePermissionService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public GroupRoutePermissionService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<GroupRoutePermissionModelDTO>> GetGroupRoutePermissions()
        {
            return await _repositoryUnitOfWork.GroupRoutePermissionRepository.GetGroupRoutePermissions();
        }

        public async Task<GroupRoutePermissionModelDTO?> GetGroupRoutePermissionById(int id)
        {
            return await _repositoryUnitOfWork.GroupRoutePermissionRepository.GetGroupRoutePermissionById(id);
        }
        public async Task<List<GroupRoutePermissionModelDTO?>> GetGroupRoutePermissionByGroupId(int groupId)
        {
            return await _repositoryUnitOfWork.GroupRoutePermissionRepository.GetGroupRoutePermissionByGroupId(groupId);
        }
        public async Task<List<GroupRoutePermissionModelDTO?>> GetRouteListByGroupId(int groupId)
        {
            return await _repositoryUnitOfWork.GroupRoutePermissionRepository.GetRouteListByGroupId(groupId);
        }

        public async Task<ResponseResult> SaveUpdateGroupRoutePermission(GroupRoutePermissionModelDTO permission)
        {
            var validation = await ValidateGroupRoutePermission(permission);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            return await _repositoryUnitOfWork.GroupRoutePermissionRepository.SaveUpdateGroupRoutePermission(permission);
        }

        public async Task<ResponseResult> DeleteGroupRoutePermission(int id)
        {
            try
            {
                await _repositoryUnitOfWork.GroupRoutePermissionRepository.DeleteGroupRoutePermission(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Group route permission deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete group route permission: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateGroupRoutePermission(GroupRoutePermissionModelDTO permission)
        {
            if (permission.GroupId <= 0)
                return Task.FromResult((false, "Group ID must be greater than 0."));

            if (permission.RouteId <= 0)
                return Task.FromResult((false, "Route ID must be greater than 0."));

            return Task.FromResult((true, string.Empty));
        }

        public GroupRoutePermissionModelDTO CreateNewGroupRoutePermission()
        {
            return new GroupRoutePermissionModelDTO
            {
                GroupId = 0,
                RouteId = 0
            };
        }
    }
}
