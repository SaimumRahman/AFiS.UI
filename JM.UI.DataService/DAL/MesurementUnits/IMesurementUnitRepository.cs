using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MesurementUnits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.MesurementUnits
{
    public interface IMesurementUnitRepository
    {
        Task<IEnumerable<MesurementUnitModelDTO>> GetMesurementUnits();
        Task<MesurementUnitModelDTO?> GetMesurementUnitById(int id);
        Task<ResponseResult> SaveUpdateMesurementUnit(MesurementUnitModelDTO unit);
        Task DeleteMesurementUnit(int id);
    }
}
