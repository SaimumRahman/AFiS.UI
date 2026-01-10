using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Company
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDTO>> GetCompanies();
        Task<CompanyDTO?> GetCompanyById(int id);
        Task<ResponseResult> SaveUpdateCompany(CompanyDTO company);
        Task<ResponseResult> DeleteCompany(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateCompany(CompanyDTO company);
        CompanyDTO CreateNewCompany();
        string Truncate(string? value, int maxChars);
    }
}
