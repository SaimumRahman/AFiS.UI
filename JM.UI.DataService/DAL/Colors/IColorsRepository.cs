using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Colors;

namespace JM.UI.DataService.DAL.Colors;

public interface IColorsRepository
{
    Task<IEnumerable<ColorsDTO>> GetColors();
    Task<ColorsDTO?> GetColorsById(int id);
    Task<ResponseResult> SaveUpdateColors(ColorsDTO Colors);
    Task DeleteColors(int id);
}
