using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MesurementUnits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.MesurementUnits
{
    public interface IMesurementUnitService
    {
        Task<IEnumerable<MesurementUnitModelDTO>> GetMesurementUnits();
        Task<MesurementUnitModelDTO?> GetMesurementUnitById(int id);
        Task<ResponseResult> SaveUpdateMesurementUnit(MesurementUnitModelDTO unit);
        Task<ResponseResult> DeleteMesurementUnit(int id);
    }
}
