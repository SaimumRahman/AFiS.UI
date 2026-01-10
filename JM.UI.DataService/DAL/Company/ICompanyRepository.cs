using JM;
using JM.Infrastructure.Models;
using JM.UI;
using JM.UI.DataService;
using JM.UI.DataService.DAL;
using JM.UI.DataService.DAL.Approval;
using JM.UI.DataService.DAL.Company;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Company
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<CompanyDTO>> GetCompanies();
        Task<CompanyDTO?> GetCompanyById(int id);
        Task DeleteCompany(int id);
        Task<ResponseResult> SaveUpdateCompany(CompanyDTO company);
    }
}
