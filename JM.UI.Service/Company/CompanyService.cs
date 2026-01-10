using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Company
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CompanyService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<CompanyDTO>> GetCompanies()
        {
            var companies = await _repositoryUnitOfWork.CompanyRepository.GetCompanies();
            return companies.Select(c => new CompanyDTO
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                Contact = c.Contact,
                Email = c.Email,
                VAT = c.VAT,
                TIN = c.TIN,
                CreatedOn = c.CreatedOn,
                CreatedBy = c.CreatedBy,
                ModifiedOn = c.ModifiedOn,
                ModifiedBy = c.ModifiedBy
            }).ToList();
        }

        public async Task<CompanyDTO?> GetCompanyById(int id)
        {
            return await _repositoryUnitOfWork.CompanyRepository.GetCompanyById(id);
        }

        public async Task<ResponseResult> SaveUpdateCompany(CompanyDTO company)
        {
            var validation = await ValidateCompany(company);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (company.Id == 0)
            {
                company.CreatedOn = DateTime.Now;
            }
            else
            {
                company.ModifiedOn = DateTime.Now;
            }

            return await _repositoryUnitOfWork.CompanyRepository.SaveUpdateCompany(company);
        }

        public async Task<ResponseResult> DeleteCompany(int id)
        {
            try
            {
                await _repositoryUnitOfWork.CompanyRepository.DeleteCompany(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Company deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete company: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateCompany(CompanyDTO company)
        {
            if (string.IsNullOrWhiteSpace(company.Name))
                return Task.FromResult((false, "Company name is required."));

            if (company.Name.Length > 100)
                return Task.FromResult((false, "Company name cannot exceed 100 characters."));

            if (!string.IsNullOrWhiteSpace(company.Address) && company.Address.Length > 500)
                return Task.FromResult((false, "Address cannot exceed 500 characters."));

            if (!string.IsNullOrWhiteSpace(company.Contact) && company.Contact.Length > 50)
                return Task.FromResult((false, "Contact cannot exceed 50 characters."));

            if (!string.IsNullOrWhiteSpace(company.Email))
            {
                if (company.Email.Length > 100)
                    return Task.FromResult((false, "Email cannot exceed 100 characters."));

                if (!IsValidEmail(company.Email))
                    return Task.FromResult((false, "Invalid email format."));
            }

            if (!string.IsNullOrWhiteSpace(company.VAT) && company.VAT.Length > 50)
                return Task.FromResult((false, "VAT cannot exceed 50 characters."));

            if (!string.IsNullOrWhiteSpace(company.TIN) && company.TIN.Length > 50)
                return Task.FromResult((false, "TIN cannot exceed 50 characters."));

            return Task.FromResult((true, string.Empty));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public CompanyDTO CreateNewCompany()
        {
            return new CompanyDTO
            {
                CreatedOn = DateTime.Now
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
