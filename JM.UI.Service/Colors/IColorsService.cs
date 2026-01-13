using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Colors;

namespace JM.UI.Service.Colors;

public interface IColorsService
{
    Task<IEnumerable<ColorsDTO>> GetColorss();
    Task<ColorsDTO?> GetColorsById(int id);
    Task<ResponseResult> SaveUpdateColors(ColorsDTO Colors);
    Task<ResponseResult> DeleteColors(int id);
    Task<(bool IsValid, string ErrorMessage)> ValidateColors(ColorsDTO Colors);
    string Truncate(string? value, int maxChars);
}
