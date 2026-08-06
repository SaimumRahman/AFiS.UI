using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.SalesPOS
{
    public class SaleRepository : BaseRepository, ISaleRepository
    {
        public SaleRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<SaleRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetSales()
        {
            try
            {
                _logger.LogInformation("Fetching all sales");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/SalePOS/getall");
                response.EnsureSuccessStatusCode();

                var sales = await response.Content.ReadFromJsonAsync<List<SaleSummaryDTO>>();
                return sales ?? new List<SaleSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get sales");
                throw new Exception("Failed to fetch sales: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get sales");
                throw new Exception("Unexpected error fetching sales: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetDraftSales()
        {
            try
            {
                _logger.LogInformation("Fetching draft sales");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/SalePOS/getall-draft");
                response.EnsureSuccessStatusCode();

                var sales = await response.Content.ReadFromJsonAsync<List<SaleSummaryDTO>>();
                return sales ?? new List<SaleSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get draft sales");
                throw new Exception("Failed to fetch draft sales: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get draft sales");
                throw new Exception("Unexpected error fetching draft sales: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetBookingSales()
        {
            try
            {
                _logger.LogInformation("Fetching booking sales");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/SalePOS/getall-booking");
                response.EnsureSuccessStatusCode();

                var sales = await response.Content.ReadFromJsonAsync<List<SaleSummaryDTO>>();
                return sales ?? new List<SaleSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get booking sales");
                throw new Exception("Failed to fetch booking sales: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get booking sales");
                throw new Exception("Unexpected error fetching booking sales: " + ex.Message, ex);
            }
        }

        public async Task<SaleMasterDTO?> GetSaleById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching sale: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Sale not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<SaleMasterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get sale by ID: {Id}", id);
                throw new Exception($"Failed to fetch sale: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get sale by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching sale: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveSale(SaleMasterDTO sale)
        {
            try
            {
                _logger.LogInformation("Saving sale");

                var httpClient = GetAuthenticatedClient("MainApi");
                var content = JsonContent.Create(new { Sale = sale });
                var response = await httpClient.PostAsync("api/SalePOS/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save sale");
                throw new Exception("Failed to save sale: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save sale");
                throw new Exception("Unexpected error saving sale: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> SaveDuePayment(int saleMasterId, int storeId, List<PaymentTransactionDTO> payments, int createdBy)
        {
            try
            {
                _logger.LogInformation("Saving due payment for sale: {SaleMasterId}", saleMasterId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var content = JsonContent.Create(new
                {
                    SaleMasterId = saleMasterId,
                    StoreId = storeId,
                    CreatedBy = createdBy,
                    Payments = payments
                });
                var response = await httpClient.PostAsync("api/SalePOS/save-due-payment", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save due payment");
                throw new Exception("Failed to save due payment: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save due payment");
                throw new Exception("Unexpected error saving due payment: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> UnmarkDraftSale(int saleMasterId)
        {
            try
            {
                _logger.LogInformation("Unmarking draft for sale: {SaleMasterId}", saleMasterId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsync($"api/SalePOS/unmark-draft/{saleMasterId}", null);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during unmark draft");
                throw new Exception("Failed to unmark draft: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during unmark draft");
                throw new Exception("Unexpected error unmarking draft: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> DeleteSale(int id)
        {
            try
            {
                _logger.LogInformation("Deleting sale: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/SalePOS/delete/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete sale: {Id}", id);
                throw new Exception($"Failed to delete sale: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete sale: {Id}", id);
                throw new Exception($"Unexpected error deleting sale: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetSalesByDateRange(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation("Fetching sales from {FromDate} to {ToDate}", fromDate, toDate);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/by-date-range?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No sales found for date range");
                    return new List<SaleSummaryDTO>();
                }

                var sales = await response.Content.ReadFromJsonAsync<List<SaleSummaryDTO>>();
                return sales ?? new List<SaleSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get sales by date range");
                throw new Exception($"Failed to fetch sales by date range: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get sales by date range");
                throw new Exception($"Unexpected error fetching sales by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetSalesByCustomerId(int customerId)
        {
            try
            {
                _logger.LogInformation("Fetching sales for customer: {CustomerId}", customerId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/by-customer/{customerId}");

                if (!response.IsSuccessStatusCode)
                    return new List<SaleSummaryDTO>();

                var sales = await response.Content.ReadFromJsonAsync<List<SaleSummaryDTO>>();
                return sales ?? new List<SaleSummaryDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sales by customer ID");
                throw;
            }
        }

        public async Task<SaleMasterDTO?> GetSaleByInvoiceNo(string invoiceNo)
        {
            try
            {
                _logger.LogInformation("Fetching sale by invoice: {InvoiceNo}", invoiceNo);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/by-invoice/{invoiceNo}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<SaleMasterDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sale by invoice no");
                throw;
            }
        }

        public async Task<string> GetNewInvoiceNo()
        {
            try
            {
                _logger.LogInformation("Requesting new invoice number");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/SalePOS/new-invoice-no");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<InvoiceNoResponse>();
                return result?.InvoiceNo ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching new invoice number");
                throw new Exception("Failed to generate invoice number: " + ex.Message, ex);
            }
        }
     
        public async Task<ProductSearchDTO?> SearchByBarcode(string returnRefNo, int storeId)
        {
            try
            {
                _logger.LogInformation("Searching by barcode: {Barcode} in store: {StoreId}", returnRefNo, storeId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/search-barcode/{Uri.EscapeDataString(returnRefNo)}/{storeId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ProductSearchDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by barcode");
                return null;
            }
        }

        public async Task<IEnumerable<ProductSearchDTO>> SearchProducts(string term)
        {
            try
            {
                _logger.LogInformation("Searching products: {Term}", term);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/SalePOS/search-products?term={Uri.EscapeDataString(term)}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ProductSearchDTO>>();
                return result ?? new List<ProductSearchDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return new List<ProductSearchDTO>();
            }
        }
    }

    internal class InvoiceNoResponse
    {
        public string InvoiceNo { get; set; } = string.Empty;
    }
}
