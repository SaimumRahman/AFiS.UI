using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRoutePermission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.GroupRoutePermission
{
    public interface IGroupRoutePermissionService
    {
        GroupRoutePermissionModelDTO CreateNewGroupRoutePermission();
        Task<ResponseResult> DeleteGroupRoutePermission(int id);
        Task<GroupRoutePermissionModelDTO?> GetGroupRoutePermissionById(int id);
        Task<List<GroupRoutePermissionModelDTO?>> GetGroupRoutePermissionByGroupId(int groupId);
        Task<IEnumerable<GroupRoutePermissionModelDTO>> GetGroupRoutePermissions();
        Task<ResponseResult> SaveUpdateGroupRoutePermission(GroupRoutePermissionModelDTO permission);
        Task<(bool IsValid, string ErrorMessage)> ValidateGroupRoutePermission(GroupRoutePermissionModelDTO permission);
    }
}