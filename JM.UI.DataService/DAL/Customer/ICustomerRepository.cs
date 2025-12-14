using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Customer;

namespace JM.UI.DataService.DAL.Customer
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerModelDTO>> GetCustomers();
        Task<CustomerModelDTO?> GetCustomerById(int customerId);
        Task<ResponseResult> SaveUpdateCustomer(CustomerModelDTO customer);
        Task DeleteCustomer(int customerId);
        Task ToggleCustomerStatus(int customerId);
    }
}