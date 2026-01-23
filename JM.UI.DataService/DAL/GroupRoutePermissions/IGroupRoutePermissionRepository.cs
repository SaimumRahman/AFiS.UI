using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRoutePermission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.GroupRoutePermissions
{
    public interface IGroupRoutePermissionRepository
    {
        Task DeleteGroupRoutePermission(int id);
        Task<GroupRoutePermissionModelDTO?> GetGroupRoutePermissionById(int id);
        Task<List<GroupRoutePermissionModelDTO?>> GetGroupRoutePermissionByGroupId(int groupId);
        Task<IEnumerable<GroupRoutePermissionModelDTO>> GetGroupRoutePermissions();
        Task<ResponseResult> SaveUpdateGroupRoutePermission(GroupRoutePermissionModelDTO permission);
        Task<List<GroupRoutePermissionModelDTO?>> GetRouteListByGroupId(int groupId);
        Task<List<GroupRoutePermissionModelDTO?>> GetRouteListByUserId(int userId);
        Task<GroupRoutePermissionModelDTO?> GetRoutePermittedForUser(int userId, string routePath);
    }
}