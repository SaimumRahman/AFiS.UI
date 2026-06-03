using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Employees;
using JM.UI.Entities.Model.Users;
using JM.UI.Service.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JM.UI.Service.Employee
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public EmployeeService(IRepositoryUnitOfWork repositoryUnitOfWork)
        {
            _repositoryUnitOfWork = repositoryUnitOfWork;
        }

        public async Task<IEnumerable<EmployeeModelDTO>> GetEmployees()
        {
            var employees = await _repositoryUnitOfWork
                .EmployeeRepository
                .GetEmployees();

            return employees.Select(e => new EmployeeModelDTO
            {
                Id = e.Id,
                Name = e.Name,
                Surname = e.Surname,
                EmployeeCode = e.EmployeeCode,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                BloodGroup = e.BloodGroup,
                Religion = e.Religion,
                FatherName = e.FatherName,
                MotherName = e.MotherName,
                MaritalStatus = e.MaritalStatus,
                SpouseName = e.SpouseName,
                PresentAddress = e.PresentAddress,
                PermanentAddress = e.PermanentAddress,
                Contact = e.Contact,
                Email = e.Email,
                EmergencyContact = e.EmergencyContact,
                EmergencyContactPerson = e.EmergencyContactPerson,
                EmergencyContactPersonAddress = e.EmergencyContactPersonAddress,
                Relation = e.Relation,
                BankId = e.BankId,
                BankAccountNumber = e.BankAccountNumber,
                Picture = e.Picture,
                DateJoined = e.DateJoined,
                DateReleased = e.DateReleased,
                NID = e.NID,
                ReferredBy = e.ReferredBy,
                AccountId = e.AccountId,
                StoreId = e.StoreId,
                DesignationId = e.DesignationId,
                BasicSalary = e.BasicSalary,
                DutyType = e.DutyType,
                ShiftId = e.ShiftId,
                Status = e.Status,
                CreatedBy = e.CreatedBy,
                CreatedOn = e.CreatedOn,
                ModifiedBy = e.ModifiedBy,
                ModifiedOn = e.ModifiedOn,
                BankName = e.BankName,
                StoreName = e.StoreName,
                DesignationName = e.DesignationName,
                ShiftName = e.ShiftName
            });
        }

        public async Task<EmployeeModelDTO?> GetEmployeeById(int id)
        {
            return await _repositoryUnitOfWork
                .EmployeeRepository
                .GetEmployeeById(id);
        }
        public async Task<EmployeeModelDTO?> GetEmployeeBySurname(string surname)
        {
            return await _repositoryUnitOfWork
                .EmployeeRepository
                .GetEmployeeBySurname(surname);
        }
        public async Task<ResponseResult> SaveUpdateEmployee(EmployeeModelDTO employee)
        {
            var validation = await ValidateEmployee(employee);

            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (employee.Id == 0)
            {
                employee.CreatedOn = DateTime.Now;
                employee.Status = 1;
            }
            else
            {
                employee.ModifiedOn = DateTime.Now;
            }
            return await _repositoryUnitOfWork
                .EmployeeRepository
                .SaveUpdateEmployee(employee);
        }

        public async Task<ResponseResult> DeleteEmployee(int id)
        {
            try
            {
                await _repositoryUnitOfWork
                    .EmployeeRepository
                    .DeleteEmployee(id);

                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Employee deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete employee: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateEmployee(EmployeeModelDTO employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
                return Task.FromResult((false, "Employee name is required."));

            if (!Regex.IsMatch(employee.Surname, "^[A-Za-z0-9]+$"))
                return Task.FromResult((false, "Surname must contain only letters and no whitespace."));

            if (employee.Name.Length > 200)
                return Task.FromResult((false, "Employee name cannot exceed 200 characters."));

            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                return Task.FromResult((false, "Employee code is required."));

            if (employee.EmployeeCode.Length > 50)
                return Task.FromResult((false, "Employee code cannot exceed 50 characters."));
            if (employee.StoreId > 0)
                return Task.FromResult((false, "Store ID cannot exceed 50 characters."));

            //if (!string.IsNullOrWhiteSpace(employee.Contact) &&
            //    employee.Contact.Length > 50)
            //    return Task.FromResult((false, "Contact cannot exceed 50 characters."));

            //if (!string.IsNullOrWhiteSpace(employee.Email))
            //{
            //    if (employee.Email.Length > 100)
            //        return Task.FromResult((false, "Email cannot exceed 100 characters."));

            //    if (!IsValidEmail(employee.Email))
            //        return Task.FromResult((false, "Invalid email format."));
            //}

            //if (employee.BasicSalary.HasValue && employee.BasicSalary < 0)
            //    return Task.FromResult((false, "Basic salary cannot be negative."));

            return Task.FromResult((true, string.Empty));
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

        public async Task<EmployeeModelDTO> CreateNewEmployee()
        {
            var code = await GetEmployeeCode();
            return new EmployeeModelDTO
            {
                CreatedOn = DateTime.Now,
                Status = 1,
                DateJoined = DateTime.Now,
                EmployeeCode = code?.EmployeeCode ?? string.Empty
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars
                ? value.Substring(0, maxChars) + "..."
                : value ?? string.Empty;
        }

        public string GetStatusText(int status) =>
            status switch
            {
                1 => "Active",
                0 => "Inactive",
                2 => "On Leave",
                3 => "Terminated",
                _ => "Unknown"
            };

        public string GetStatusBadgeStyle(int status) =>
            status switch
            {
                1 => "background-color:#4caf50;color:white;",
                0 => "background-color:#f44336;color:white;",
                2 => "background-color:#ff9800;color:white;",
                3 => "background-color:#9e9e9e;color:white;",
                _ => "background-color:#607d8b;color:white;"
            };

        public async Task<EmployeeModelDTO?> GetEmployeeCode()
        {
            return await _repositoryUnitOfWork
                 .EmployeeRepository
                 .GetEmployeeCode();
        }
    }
}
