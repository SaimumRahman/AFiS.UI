using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Employees;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Employees
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        public EmployeeRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<EmployeeRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<EmployeeModelDTO>> GetEmployees()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all employees");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Employees/getall");
                response.EnsureSuccessStatusCode();

                var employees = await response.Content.ReadFromJsonAsync<List<EmployeeModelDTO>>();

                return employees ?? new List<EmployeeModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get employees");
                throw new Exception("Failed to fetch employees: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get employees");
                throw new Exception("Unexpected error fetching employees: " + ex.Message, ex);
            }
        }

        public async Task<EmployeeModelDTO?> GetEmployeeById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch employee: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Employees/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Employee not found: {Id}", id);
                    return null;
                }

                var employee = await response.Content.ReadFromJsonAsync<EmployeeModelDTO>();
                return employee;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get employee by ID: {Id}", id);
                throw new Exception($"Failed to fetch employee: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get employee by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching employee: {ex.Message}", ex);
            }
        }

        public async Task DeleteEmployee(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete employee: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Employees/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Employee deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete employee: {Id}", id);
                throw new Exception($"Failed to delete employee: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete employee: {Id}", id);
                throw new Exception($"Unexpected error deletingemployee: employee: {ex.Message}", ex);
            }
        }
        public async Task<ResponseResult> SaveUpdateEmployee(EmployeeModelDTO employee)
        {
            try
            {
                _logger.LogInformation("Starting to save employee");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    EmployeeDTO = employee
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Employees/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save employee");
                throw new Exception("Failed to save employee: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save employee");
                throw new Exception("Unexpected error saving employee: " + ex.Message, ex);
            }
        }
    }
}
