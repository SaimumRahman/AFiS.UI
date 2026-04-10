using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Groups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Groups
{
    public interface IGroupRepository
    {
        Task<IEnumerable<GroupModelDTO>> GetGroups();
        Task<GroupModelDTO?> GetGroupById(int id);
        Task<ResponseResult> SaveUpdateGroup(GroupModelDTO group);
        Task DeleteGroup(int id);
        Task<string> GetNextGroupCode();

    }
}
