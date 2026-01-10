using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Shift;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Shift
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftDTO>> GetShift();
        Task<ShiftDTO?> GetShiftById(int id);
        Task<ResponseResult> SaveUpdateShift(ShiftDTO Shift);
        Task<ResponseResult> DeleteShift(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateShift(ShiftDTO Shift);
        ShiftDTO CreateNewShift();
        string Truncate(string? value, int maxChars);
    }
}
