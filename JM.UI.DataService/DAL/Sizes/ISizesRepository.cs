using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Sizes;

namespace JM.UI.DataService.DAL.Sizes;

public interface ISizesRepository
{
    Task<IEnumerable<SizesDTO>> GetSizess();
    Task<SizesDTO?> GetSizesById(int id);
    Task<ResponseResult> SaveUpdateSizes(SizesDTO Sizes);
    Task DeleteSizes(int id);
}
