using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.CustomerDetails
{
    public class CustomerDetailsRepository : BaseRepository, ICustomerDetailsRepository
    {
        public CustomerDetailsRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CustomerDetailsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CustomerDetailsDTO>> GetAllCustomers()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all customers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Customers/GetAllCustomers");
                response.EnsureSuccessStatusCode();

                var customers = await response.Content.ReadFromJsonAsync<List<CustomerDetailsDTO>>();

                return customers ?? new List<CustomerDetailsDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all customers");
                throw new Exception("Failed to fetch customers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all customers");
                throw new Exception("Unexpected error fetching customers: " + ex.Message, ex);
            }
        }

        public async Task<CustomerDetailsDTO?> GetCustomerById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch customer: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/GetCustomerById/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Customer not found: {Id}", id);
                    return null;
                }

                var customer = await response.Content.ReadFromJsonAsync<CustomerDetailsDTO>();
                return customer;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get customer by ID: {Id}", id);
                throw new Exception($"Failed to fetch customer: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get customer by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching customer: {ex.Message}", ex);
            }
        }

        public async Task<CustomerDetailsDTO?> GetCustomerByPhone(string phone)
        {
            try
            {
                _logger.LogInformation("Starting to fetch customer by phone: {Phone}", phone);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/GetCustomerByPhone/{phone}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Customer not found by phone: {Phone}", phone);
                    return null;
                }

                var customer = await response.Content.ReadFromJsonAsync<CustomerDetailsDTO>();
                return customer;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get customer by phone: {Phone}", phone);
                throw new Exception($"Failed to fetch customer by phone: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get customer by phone: {Phone}", phone);
                throw new Exception($"Unexpected error fetching customer by phone: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> InsertUpdateCustomer(CustomerDetailsDTO customer)
        {
            try
            {
                _logger.LogInformation("Starting to save customer");
                var command = new
                {
                    CustomerDTO = new
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        WhatsApp = customer.WhatsApp,
                        DateOfBirth = customer.DateOfBirth,
                        Anniversary = customer.Anniversary,
                        CompanyId = customer.CompanyId,
                        Designation = customer.Designation,
                        EmployeeId = customer.EmployeeId,
                        AddressHome = customer.AddressHome,
                        AddressOffice = customer.AddressOffice,
                        AddressOther = customer.AddressOther,
                        DefaultAddressType = customer.DefaultAddressType,
                        MemberTypeId = customer.MemberTypeId,
                        Remarks = customer.Remarks,
                        Status = customer.Status,
                        CreatedDate = customer.CreatedDate,
                        LastModifiedDate = customer.LastModifiedDate,
                        IsForceAdd = customer.IsForceAdd
                    }
                };
                        var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("Customers/InsertUpdateCustomer", command);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save customer");
                throw new Exception("Failed to save customer: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save customer");
                throw new Exception("Unexpected error saving customer: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> DeleteCustomer(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete customer: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Customers/DeleteCustomer/{id}");

                ResponseResult? result = null;
                try { result = await response.Content.ReadFromJsonAsync<ResponseResult>(); } catch { }

                if (!response.IsSuccessStatusCode)
                {
                    return result ?? new ResponseResult { IsSuccessStatus = false, Message = "Failed to delete customer." };
                }

                _logger.LogInformation("Customer deleted successfully: {Id}", id);
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting customer: {Id}", id);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<CustomerDetailsDTO>> GetCustomersByMobile(string mobile)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/GetCustomersByMobile/{Uri.EscapeDataString(mobile)}");
                if (!response.IsSuccessStatusCode) return new List<CustomerDetailsDTO>();
                return await response.Content.ReadFromJsonAsync<List<CustomerDetailsDTO>>() ?? new List<CustomerDetailsDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers by mobile");
                return new List<CustomerDetailsDTO>();
            }
        }

        public async Task<IEnumerable<CustomerCompanyDTO>> SearchCompanies(string term)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/SearchCompanies?term={Uri.EscapeDataString(term ?? string.Empty)}");
                if (!response.IsSuccessStatusCode) return new List<CustomerCompanyDTO>();
                return await response.Content.ReadFromJsonAsync<List<CustomerCompanyDTO>>() ?? new List<CustomerCompanyDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching companies");
                return new List<CustomerCompanyDTO>();
            }
        }

        public async Task<CustomerCompanyDTO?> GetCompanyById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/GetCompanyById/{id}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<CustomerCompanyDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company by id");
                return null;
            }
        }

        public async Task<ResponseResult> CreateCompany(CustomerCompanyDTO company)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("Customers/CreateCompany", new { Company = company });
                if (!response.IsSuccessStatusCode)
                {
                    ResponseResult? r = null;
                    try { r = await response.Content.ReadFromJsonAsync<ResponseResult>(); } catch { }
                    return r ?? new ResponseResult { IsSuccessStatus = false, Message = "Failed to create company." };
                }
                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Company saved." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
