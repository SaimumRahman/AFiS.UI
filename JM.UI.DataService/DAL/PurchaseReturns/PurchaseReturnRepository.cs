using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseReturns;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.PurchaseReturns
{
    public class PurchaseReturnRepository : BaseRepository, IPurchaseReturnRepository
    {
        public PurchaseReturnRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseReturnRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<PurchaseReturnModelDTO>> GetPurchaseReturns()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("PurchaseReturns/GetAllPurchaseReturns");
                response.EnsureSuccessStatusCode();

                var purchaseReturns = await response.Content.ReadFromJsonAsync<List<PurchaseReturnModelDTO>>();
                return purchaseReturns ?? new List<PurchaseReturnModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all purchase returns");
                throw;
            }
        }

        public async Task<PurchaseReturnModelDTO?> GetPurchaseReturnById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PurchaseReturns/GetPurchaseReturnById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<PurchaseReturnModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase return by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchaseReturn(PurchaseReturnModelDTO purchaseReturn)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                // Note: PurchaseOrderRepository wrapped it in an object, but my Controller expects PurchaseReturnDTO directly.
                // Checking PurchaseReturnsController: [FromBody] PurchaseReturnDTO purchaseReturn
                // So passing purchaseReturn directly is correct unless the API model binding was different.
                // Wait, PurchaseOrderRepository did: var requestBody = new { PurchaseOrderDTO = purchaseOrder }; 
                // That implies the API endpoint signature might be wrapped or check logic.
                // My controller: public async Task<ResponseResult> SaveUpdatePurchaseReturn([FromBody] PurchaseReturnDTO purchaseReturn)
                // So direct passing is correct.
                
                var response = await httpClient.PostAsJsonAsync("PurchaseReturns/SaveUpdatePurchaseReturn", purchaseReturn);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase return");
                throw;
            }
        }

        public async Task DeletePurchaseReturn(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"PurchaseReturns/DeletePurchaseReturn/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase return: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ReturnRefStockDetailDTO>> GetReturnRefStockDetails(string returnRefNo, int storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"items/GetReturnRefStockDetails?returnRefNo={System.Uri.EscapeDataString(returnRefNo)}&storeId={storeId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ReturnRefStockDetailDTO>>();
                return result ?? new List<ReturnRefStockDetailDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching return ref stock details for: {ReturnRefNo}", returnRefNo);
                throw;
            }
        }
    }
}
