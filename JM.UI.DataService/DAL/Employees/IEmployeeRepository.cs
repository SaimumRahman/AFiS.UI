using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Employees;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Employees
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeModelDTO>> GetEmployees();
        Task<IEnumerable<EmployeeModelDTO>> GetEmployeesByStoreId(int storeId);
        Task<EmployeeModelDTO?> GetEmployeeById(int id);
        Task<EmployeeModelDTO?> GetEmployeeCode();
        Task DeleteEmployee(int id);
        Task<ResponseResult> SaveUpdateEmployee(EmployeeModelDTO employee);
        Task<EmployeeModelDTO?> GetEmployeeBySurname(string surname);
    }
}
