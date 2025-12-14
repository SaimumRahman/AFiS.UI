using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Customer;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.Customer
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CustomerRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CustomerModelDTO>> GetCustomers()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all customers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Customers/getall");
                response.EnsureSuccessStatusCode();

                var customers = await response.Content.ReadFromJsonAsync<List<CustomerModelDTO>>();

                return customers ?? new List<CustomerModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get customers");
                throw new Exception("Failed to fetch customers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get customers");
                throw new Exception("Unexpected error fetching customers: " + ex.Message, ex);
            }
        }

        public async Task<CustomerModelDTO?> GetCustomerById(int customerId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch customer: {CustomerID}", customerId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Customers/get/{customerId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Customer not found: {CustomerID}", customerId);
                    return null;
                }

                var customer = await response.Content.ReadFromJsonAsync<CustomerModelDTO>();
                return customer;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get customer by ID: {CustomerID}", customerId);
                throw new Exception($"Failed to fetch customer: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get customer by ID: {CustomerID}", customerId);
                throw new Exception($"Unexpected error fetching customer: {ex.Message}", ex);
            }
        }

        public async Task DeleteCustomer(int customerId)
        {
            try
            {
                _logger.LogInformation("Starting to delete customer: {CustomerID}", customerId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Customers/delete/{customerId}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Customer deleted successfully: {CustomerID}", customerId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete customer: {CustomerID}", customerId);
                throw new Exception($"Failed to delete customer: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete customer: {CustomerID}", customerId);
                throw new Exception($"Unexpected error deleting customer: {ex.Message}", ex);
            }
        }

        public async Task ToggleCustomerStatus(int customerId)
        {
            try
            {
                _logger.LogInformation("Starting to toggle customer status: {CustomerID}", customerId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync($"Customers/toggle-status/{customerId}", null);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Customer status toggled successfully: {CustomerID}", customerId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during toggle customer status: {CustomerID}", customerId);
                throw new Exception($"Failed to toggle customer status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during toggle customer status: {CustomerID}", customerId);
                throw new Exception($"Unexpected error toggling customer status: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateCustomer(CustomerModelDTO customer)
        {
            try
            {
                _logger.LogInformation("Starting to save customer");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    CustomerDTO = customer
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Customers/insert-update", content);
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
    }
}