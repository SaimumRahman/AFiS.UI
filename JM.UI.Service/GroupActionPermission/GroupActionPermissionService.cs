using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.GroupActionPermission;
using JM.UI.Service.Banks;

namespace JM.UI.Service.GroupActionPermission;

public class GroupActionPermissionService : IGroupActionPermissionService
{
    private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

    public GroupActionPermissionService(IRepositoryUnitOfWork repositoryUnitOfWork)
        => _repositoryUnitOfWork = repositoryUnitOfWork;

    public async Task<IEnumerable<GroupActionPermissionDTO>> GetGroupActionPermissions(int groupId)
        => await _repositoryUnitOfWork.GroupActionPermissionRepository.GetGroupActionPermissions(groupId);
    public Task<(bool IsValid, string ErrorMessage)> ValidateGroupActionPermissions(int groupId, List<GroupActionPermissionDTO> permissions)
    {
        if (groupId <= 0)
            return Task.FromResult((false, "Invalid group selected."));

        if (permissions == null || !permissions.Any())
            return Task.FromResult((false, "No permissions provided."));

        foreach (var permission in permissions)
        {
            if (permission.RouteId <= 0)
                return Task.FromResult((false, "Invalid route ID found in permissions."));

            if (permission.ActionId <= 0)
                return Task.FromResult((false, "Invalid action ID found in permissions."));
        }

        return Task.FromResult((true, string.Empty));
    }
    public async Task<ResponseResult> InsertUpdateGroupActionPermissions(int groupId, List<GroupActionPermissionDTO> permissions)
    {
        var validation = await ValidateGroupActionPermissions(groupId, permissions);
        if (!validation.IsValid)
        {
            return new ResponseResult
            {
                IsSuccessStatus = false,
                Message = validation.ErrorMessage
            };
        }

        return await _repositoryUnitOfWork.GroupActionPermissionRepository.InsertUpdateGroupActionPermissions(groupId, permissions);
    }
}