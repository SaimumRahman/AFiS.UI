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

        public async Task<CustomerDetailsDTO?> GetCustomerById(int id)
        {
            return await _repositoryUnitOfWork.CustomerDetailsRepository.GetCustomerById(id);
        }

        public async Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer)
        {
            var validation = await Validate(customer);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (customer.Id == 0)
            {
                customer.CreatedDate = DateTime.Now;
            }
            else
            {
                customer.LastModifiedDate = DateTime.Now;
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

            if (!string.IsNullOrWhiteSpace(customer.Address) && customer.Address.Length > 200)
                return Task.FromResult((false, "Address cannot exceed 200 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public CustomerDetailsDTO CreateNew()
        {
            return new CustomerDetailsDTO
            {
                CreatedDate = DateTime.Now
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
