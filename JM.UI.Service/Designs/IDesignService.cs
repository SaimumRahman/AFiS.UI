using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Designs
{
    public interface IDesignService
    {
        Task<IEnumerable<DesignModelDTO>> GetDesigns();
        Task<DesignModelDTO?> GetDesignById(int id);
        Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId);
        Task<ResponseResult> SaveUpdateDesign(DesignModelDTO design);
        Task<ResponseResult> DeleteDesign(int id);
    }
}
