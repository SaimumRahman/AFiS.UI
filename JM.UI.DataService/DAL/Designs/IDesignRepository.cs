using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Designs
{
    public interface IDesignRepository
    {
        Task<IEnumerable<DesignModelDTO>> GetDesigns();
        Task<DesignModelDTO?> GetDesignById(int id);
        Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId);
        Task<ResponseResult> SaveUpdateDesign(DesignModelDTO design);
        Task DeleteDesign(int id);
        Task<DesignModelDTO> GetDesignCode();
    }
}
