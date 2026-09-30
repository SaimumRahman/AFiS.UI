using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CostCenters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.CostCenters
{
    public interface ICostCenterService
    {
        Task<IEnumerable<CostCenterDTO>> GetCostCenters(bool? onlyActive = null);
        Task<CostCenterDTO?> GetCostCenterById(int id);
        Task<ResponseResult> SaveUpdateCostCenter(CostCenterDTO costCenter);
        Task<ResponseResult> DeleteCostCenter(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateCostCenter(CostCenterDTO costCenter);
        CostCenterDTO CreateNewCostCenter();
    }
}