using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designations;

namespace JM.UI.Service.Designations;

public interface IDesignationService
{
    Task<IEnumerable<DesignationDTO>> GetDesignations();
    Task<DesignationDTO?> GetDesignationById(int id);
    Task<ResponseResult> SaveUpdateDesignation(DesignationDTO designation);
    Task<ResponseResult> DeleteDesignation(int id);
    Task<(bool IsValid, string ErrorMessage)> ValidateDesignation(DesignationDTO designation);
    string Truncate(string? value, int maxChars);
}
