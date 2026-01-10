using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designations;

namespace JM.UI.DataService.DAL.Designations;

public interface IDesignationRepository
{
    Task<IEnumerable<DesignationDTO>> GetDesignations();
    Task<DesignationDTO?> GetDesignationById(int id);
    Task<ResponseResult> SaveUpdateDesignation(DesignationDTO designation);
    Task DeleteDesignation(int id);
}
