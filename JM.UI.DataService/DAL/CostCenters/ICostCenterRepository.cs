using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CostCenters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.CostCenters
{
    public interface ICostCenterRepository
    {
        Task<IEnumerable<CostCenterDTO>> GetCostCenters(bool? onlyActive = null);
        Task<CostCenterDTO?> GetCostCenterById(int id);
        Task<ResponseResult> SaveUpdateCostCenter(CostCenterDTO costCenter);
        Task DeleteCostCenter(int id);
    }
}