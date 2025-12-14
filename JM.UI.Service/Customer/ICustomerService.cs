using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Customer
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerModelDTO>> GetCustomers();
        Task<CustomerModelDTO?> GetCustomerById(int customerId);
        Task<ResponseResult> SaveUpdateCustomer(CustomerModelDTO customer);
        Task<ResponseResult> DeleteCustomer(int customerId);
        Task<ResponseResult> ToggleCustomerStatus(int customerId);
        Task<(bool IsValid, string ErrorMessage)> ValidateCustomer(CustomerModelDTO customer);
        CustomerModelDTO CreateNewCustomer();
        List<string> GetCustomerTypes();
        string GetStatusBadgeStyle(bool isActive);
        string Truncate(string? value, int maxChars);
    }
}