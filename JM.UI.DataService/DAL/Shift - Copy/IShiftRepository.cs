using JM;
using JM.Infrastructure.Models;
using JM.UI;
using JM.UI.DataService;
using JM.UI.DataService.DAL;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Shift;
using JM.UI.Entities.Model.Shift;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Shift
{
    public interface IShiftRepository
    {
        Task<IEnumerable<ShiftDTO>> GetShift();
        Task<ShiftDTO?> GetShiftById(int id);
        Task DeleteShift(int id);
        Task<ResponseResult> SaveUpdateShift(ShiftDTO Shift);
    }
}
