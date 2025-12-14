using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.Customer;
using JM.UI.Entities.Model.Customer;

namespace JM.UI.Service.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        private readonly List<string> _customerTypes = new()
        {
            "Individual",
            "Corporate",
            "Retailer",
            "Wholesaler"
        };

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerModelDTO>> GetCustomers()
        {
            var customers = await _customerRepository.GetCustomers();
            return customers.Select(c => new CustomerModelDTO
            {
                CustomerID = c.CustomerID,
                CustomerName = c.CustomerName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                Address = c.Address,
                CreatedDate = c.CreatedDate,
                IsActive = c.IsActive,
                CustomerCode = c.CustomerCode,
                CustomerType = c.CustomerType,
                CurrentBalance = c.CurrentBalance,
                CreatedBy = c.CreatedBy
            }).ToList();
        }

        public async Task<CustomerModelDTO?> GetCustomerById(int customerId)
        {
            return await _customerRepository.GetCustomerById(customerId);
        }

        public async Task<ResponseResult> SaveUpdateCustomer(CustomerModelDTO customer)
        {
            var validation = await ValidateCustomer(customer);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (customer.CustomerID == 0)
            {
                customer.CreatedDate = DateTime.Now;
            }

            return await _customerRepository.SaveUpdateCustomer(customer);
        }

        public async Task<ResponseResult> DeleteCustomer(int customerId)
        {
            try
            {
                await _customerRepository.DeleteCustomer(customerId);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Customer deleted successfully!"
                };
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

        public async Task<ResponseResult> ToggleCustomerStatus(int customerId)
        {
            try
            {
                await _customerRepository.ToggleCustomerStatus(customerId);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Customer status updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to update status: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateCustomer(CustomerModelDTO customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                return Task.FromResult((false, "Customer name is required."));

            if (!string.IsNullOrWhiteSpace(customer.Email) && !IsValidEmail(customer.Email))
                return Task.FromResult((false, "Please enter a valid email address."));

            return Task.FromResult((true, string.Empty));
        }

        public CustomerModelDTO CreateNewCustomer()
        {
            return new CustomerModelDTO
            {
                IsActive = true,
                CreatedDate = DateTime.Now,
                CurrentBalance = 0,
                CustomerType = "Individual"
            };
        }

        public List<string> GetCustomerTypes()
        {
            return _customerTypes;
        }

        public string GetStatusBadgeStyle(bool isActive)
        {
            return isActive
                ? "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;"
                : "background-color: #f44336; color: white; padding: 4px 8px; border-radius: 4px;";
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
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
    }
}