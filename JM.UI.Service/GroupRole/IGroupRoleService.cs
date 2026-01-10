using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRole;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.GroupRole
{
    public interface IGroupRoleService
    {
        GroupRoleDTO CreateNewGroupRole();
        Task<ResponseResult> DeleteGroupRole(int id);
        Task<GroupRoleDTO?> GetGroupRoleById(int id);
        Task<IEnumerable<GroupRoleDTO>> GetGroupRoles();
        Task<ResponseResult> SaveUpdateGroupRole(GroupRoleDTO groupRole);
        string Truncate(string? value, int maxChars);
        Task<(bool IsValid, string ErrorMessage)> ValidateGroupRole(GroupRoleDTO groupRole);
    }
}