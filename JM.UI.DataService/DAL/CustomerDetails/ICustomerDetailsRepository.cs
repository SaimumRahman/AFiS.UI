using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CustomerDetails;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.CustomerDetails
{
    public interface ICustomerDetailsRepository
    {
        Task<IEnumerable<CustomerDetailsDTO>> GetAllCustomers();
        Task<CustomerDetailsDTO?> GetCustomerById(int id);
        Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer);
        Task<ResponseResult> DeleteCustomer(int id);
    }
}
