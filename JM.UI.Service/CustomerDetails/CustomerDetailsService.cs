using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.CustomerDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.CustomerDetails
{
    public class CustomerDetailsService : ICustomerDetailsService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CustomerDetailsService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<CustomerDetailsDTO>> GetAllCustomers()
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetAllCustomers();
        }

        public async Task<IEnumerable<CustomerDetailsDTO>> GetCustomersByMobile(string mobile)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetCustomersByMobile(mobile);
        }

        public async Task<CustomerDetailsDTO?> GetCustomerById(int id)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetCustomerById(id);
        }

        public async Task<CustomerDetailsDTO?> GetCustomerByPhone(string phone)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetCustomerByPhone(phone);
        }

        public async Task<IEnumerable<CustomerCompanyDTO>> SearchCompanies(string term)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.SearchCompanies(term);
        }

        public async Task<CustomerCompanyDTO?> GetCompanyById(int id)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetCompanyById(id);
        }

        public async Task<ResponseResult> CreateCompany(CustomerCompanyDTO company)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.CreateCompany(company);
        }

        public async Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer)
        {
            var validation = await Validate(customer);
            if (!validation.IsValid)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = validation.ErrorMessage };
            }

            if (customer.Id == 0)
            {
                customer.CreatedDate = DateTime.Now;
                if (customer.MemberTypeId == null) customer.MemberTypeId = 2; // General
                if (string.IsNullOrWhiteSpace(customer.Status)) customer.Status = "Active";
                if (string.IsNullOrWhiteSpace(customer.DefaultAddressType)) customer.DefaultAddressType = "Home";
            }
            else
            {
                customer.LastModifiedDate = DateTime.Now;
            }

            // Build the single Address field from the default address for backward compat
            if (string.IsNullOrWhiteSpace(customer.Address))
            {
                customer.Address = customer.DefaultAddressType switch
                {
                    "Office" => customer.AddressOffice,
                    "Other" => customer.AddressOther,
                    _ => customer.AddressHome
                };
            }

            return await _repositoryUnitOfWork.CustomerDetailsRepository.InsertUpdateCustomer(customer);
        }

        public async Task<ResponseResult> DeleteCustomer(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.CustomerDetailsRepository.DeleteCustomer(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete customer: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> Validate(CustomerDetailsDTO customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return Task.FromResult((false, "Customer name is required."));

            if (customer.Name.Length > 100)
                return Task.FromResult((false, "Customer name cannot exceed 100 characters."));

            if (!string.IsNullOrWhiteSpace(customer.Email) && customer.Email.Length > 100)
                return Task.FromResult((false, "Email cannot exceed 100 characters."));

            if (string.IsNullOrWhiteSpace(customer.Phone))
                return Task.FromResult((false, "Phone number is required."));

            if (customer.Phone.Length > 20)
                return Task.FromResult((false, "Phone number cannot exceed 20 characters."));

            // Address rule: if any address is filled, the default must be filled.
            var home = (customer.AddressHome ?? string.Empty).Trim();
            var office = (customer.AddressOffice ?? string.Empty).Trim();
            var other = (customer.AddressOther ?? string.Empty).Trim();
            var anyAddr = !string.IsNullOrEmpty(home) || !string.IsNullOrEmpty(office) || !string.IsNullOrEmpty(other);
            if (anyAddr)
            {
                var def = customer.DefaultAddressType ?? "Home";
                var defVal = def == "Office" ? office : (def == "Other" ? other : home);
                if (string.IsNullOrEmpty(defVal))
                    return Task.FromResult((false, "The default address must be filled in."));
            }

            // Company: if text was typed it must resolve (handled in UI); here just length.
            if (!string.IsNullOrWhiteSpace(customer.Designation) && customer.Designation.Length > 100)
                return Task.FromResult((false, "Designation cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public CustomerDetailsDTO CreateNew()
        {
            return new CustomerDetailsDTO
            {
                CreatedDate = DateTime.Now,
                Status = "Active",
                DefaultAddressType = "Home",
                MemberTypeId = 2
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
