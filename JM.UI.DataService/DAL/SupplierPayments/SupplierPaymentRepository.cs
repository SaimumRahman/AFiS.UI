using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ResponseResult { IsSuccessStatus = false, Message = $"Server Error: {response.StatusCode} - {errorContent}" };
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
    }
}
