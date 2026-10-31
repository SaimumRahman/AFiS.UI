using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CustomerDetails;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.CustomerDetails
{
    public interface ICustomerDetailsService
    {
        Task<IEnumerable<CustomerDetailsDTO>> GetAllCustomers();
        Task<CustomerDetailsDTO?> GetCustomerById(int id);
        Task<CustomerDetailsDTO?> GetCustomerByPhone(string phone);
        Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer);
        Task<ResponseResult> DeleteCustomer(int id);
        Task<(bool IsValid, string ErrorMessage)> Validate(CustomerDetailsDTO customer);
        CustomerDetailsDTO CreateNew();
        string Truncate(string? value, int maxChars);
    }
}
