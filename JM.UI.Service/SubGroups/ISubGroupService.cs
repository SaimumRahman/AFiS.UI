using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SubGroups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.SubGroups
{
    public interface ISubGroupService
    {
        Task<IEnumerable<SubGroupModelDTO>> GetSubGroups();
        Task<SubGroupModelDTO?> GetSubGroupById(int id);
        Task<ResponseResult> SaveUpdateSubGroup(SubGroupModelDTO subGroup);
        Task<ResponseResult> DeleteSubGroup(int id);
    }
}
