using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRole;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.GroupRole
{
    // Client Repository Interface
    public interface IGroupRoleRepository
    {
        Task<IEnumerable<GroupRoleDTO>> GetGroupRoles();
        Task<GroupRoleDTO?> GetGroupRoleById(int id);
        Task DeleteGroupRole(int id);
        Task<ResponseResult> SaveUpdateGroupRole(GroupRoleDTO groupRole);
    }
}
