using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupActionPermission;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.GroupActionPermission
{
    public interface IGroupActionPermissionRepository
    {
        Task<IEnumerable<GroupActionPermissionDTO>> GetGroupActionPermissions(int groupId);
        Task<ResponseResult> InsertUpdateGroupActionPermissions(int groupId, List<GroupActionPermissionDTO> permissions);
    }
}