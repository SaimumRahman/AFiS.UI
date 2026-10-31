using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.SupplierPayments
{
    public class SupplierPaymentRepository : BaseRepository, ISupplierPaymentRepository
    {
        public SupplierPaymentRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<SupplierPaymentRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<SupplierPaymentDTO>> GetSupplierPayments()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("SupplierPayments");
                response.EnsureSuccessStatusCode();

                var payments = await response.Content.ReadFromJsonAsync<List<SupplierPaymentDTO>>();
                return payments ?? new List<SupplierPaymentDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all supplier payments");
                throw;
            }
        }

        public async Task<SupplierPaymentDTO?> GetSupplierPaymentById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SupplierPayments/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<SupplierPaymentDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier payment by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSupplierPayment(SupplierPaymentDTO payment)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { SupplierPayment = payment };
                var response = await httpClient.PostAsJsonAsync("SupplierPayments", requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var resultId = await response.Content.ReadFromJsonAsync<int>();
                    return new ResponseResult { IsSuccessStatus = true, Message = "Saved Successfully", Data = resultId };
                }
                else
                {
                    var message = await ReadErrorMessageAsync(response);
                    return new ResponseResult { IsSuccessStatus = false, Message = message };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving supplier payment");
                 return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task DeleteSupplierPayment(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"SupplierPayments/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier payment: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<SupplierLedgerDTO>> GetSupplierLedger(int supplierId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SupplierPayments/Ledger/{supplierId}");
                response.EnsureSuccessStatusCode();

                var ledger = await response.Content.ReadFromJsonAsync<List<SupplierLedgerDTO>>();
                return ledger ?? new List<SupplierLedgerDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier ledger");
                throw;
            }
        }

        public async Task<IEnumerable<SupplierOutstandingDTO>> GetSupplierOutstanding()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("SupplierPayments/Outstanding");
                response.EnsureSuccessStatusCode();

                var outstanding = await response.Content.ReadFromJsonAsync<List<SupplierOutstandingDTO>>();
                return outstanding ?? new List<SupplierOutstandingDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier outstanding balances");
                throw;
            }
        }

        public async Task<SupplierPaymentSourcesDTO?> GetPaymentSources(int storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SupplierPayments/payment-sources?storeId={storeId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SupplierPaymentSourcesDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier payment sources for store {StoreId}", storeId);
                throw;
            }
        }

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return $"Server error ({(int)response.StatusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("message", out var message)
                    && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? content;
                }

                if (doc.RootElement.TryGetProperty("errors", out var errors)
                    && errors.ValueKind == JsonValueKind.Array)
                {
                    var messages = new List<string>();
                    foreach (var error in errors.EnumerateArray())
                    {
                        if (error.TryGetProperty("errorMessage", out var em)
                            && em.ValueKind == JsonValueKind.String)
                        {
                            messages.Add(em.GetString()!);
                        }
                    }

                    if (messages.Count > 0)
                        return string.Join(" ", messages);
                }
            }
            catch (JsonException)
            {
                // Not JSON - return the raw body below.
            }

            return content;
        }
    }
}
