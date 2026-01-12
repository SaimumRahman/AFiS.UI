using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Sizes;

namespace JM.UI.Service.Sizes;

public interface ISizesService
{
    Task<IEnumerable<SizesDTO>> GetSizess();
    Task<SizesDTO?> GetSizesById(int id);
    Task<ResponseResult> SaveUpdateSizes(SizesDTO Sizes);
    Task<ResponseResult> DeleteSizes(int id);
    Task<(bool IsValid, string ErrorMessage)> ValidateSizes(SizesDTO Sizes);
    string Truncate(string? value, int maxChars);
}
