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
        Task<IEnumerable<CustomerDetailsDTO>> GetCustomersByMobile(string mobile);
        Task<CustomerDetailsDTO?> GetCustomerById(int id);
        Task<CustomerDetailsDTO?> GetCustomerByPhone(string phone);
        Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer);
        Task<ResponseResult> DeleteCustomer(int id);
        Task<IEnumerable<CustomerCompanyDTO>> SearchCompanies(string term);
        Task<CustomerCompanyDTO?> GetCompanyById(int id);
        Task<ResponseResult> CreateCompany(CustomerCompanyDTO company);
    }
}
