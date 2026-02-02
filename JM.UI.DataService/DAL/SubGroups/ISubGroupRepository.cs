using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SubGroups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.SubGroups
{
    public interface ISubGroupRepository
    {
        Task<IEnumerable<SubGroupModelDTO>> GetSubGroups();
        Task<SubGroupModelDTO?> GetSubGroupById(int id);
        Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId);
        Task<ResponseResult> SaveUpdateSubGroup(SubGroupModelDTO subGroup);
        Task DeleteSubGroup(int id);
    }
}
