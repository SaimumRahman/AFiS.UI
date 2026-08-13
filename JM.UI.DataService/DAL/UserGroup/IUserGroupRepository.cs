using JM.Infrastructure.Models;
using JM.UI.Entities.Model.UserGroup;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.UserGroup
{
    public interface IUserGroupRepository
    {
        Task<IEnumerable<UserGroupDTO>> GetAllUserGroups();
        Task<IEnumerable<UserGroupDTO>> GetUserGroupsByGroupId(int groupId);
        Task<GroupUsersDTO> GetGroupUsersDetail(int groupId);
        Task<ResponseResult> AssignUsersToGroup(int groupId, List<int> userIds);
        Task<ResponseResult> RemoveUserFromGroup(int userId, int groupId);
        Task<ResponseResult> UpdateGroupUsers(int groupId, List<int> userIds);
        Task<int> GetAdminGroupCountByUserId(int userId);
    }
}
