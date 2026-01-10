using JM.Infrastructure.Models;
using JM.UI.Entities.Model.UserGroup;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.UserGroup
{
    public interface IUserGroupService
    {
        Task<ResponseResult> AssignUsersToGroup(int groupId, List<int> userIds);
        Task<IEnumerable<UserGroupDTO>> GetAllUserGroups();
        Task<GroupUsersDTO> GetGroupUsersDetail(int groupId);
        Task<IEnumerable<UserGroupDTO>> GetUserGroupsByGroupId(int groupId);
        Task<ResponseResult> RemoveUserFromGroup(int userId, int groupId);
    }
}