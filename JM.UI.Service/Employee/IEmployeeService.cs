using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Employees;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Employee
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeModelDTO>> GetEmployees();
        Task<EmployeeModelDTO?> GetEmployeeById(int id);
        Task<EmployeeModelDTO?> GetEmployeeCode();
        Task<ResponseResult> SaveUpdateEmployee(EmployeeModelDTO employee);
        Task<ResponseResult> DeleteEmployee(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateEmployee(EmployeeModelDTO employee);
        Task<EmployeeModelDTO> CreateNewEmployee();
        Task<EmployeeModelDTO?> GetEmployeeBySurname(string surname);
        string Truncate(string? value, int maxChars);
        string GetStatusText(int status);
        string GetStatusBadgeStyle(int status);
    }
}
