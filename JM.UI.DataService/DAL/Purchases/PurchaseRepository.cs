using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public class PurchaseRepository : BaseRepository, IPurchaseRepository
    {
        public PurchaseRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<PurchaseModelDTO>> GetPurchases()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Purchases/GetAllPurchases");
                response.EnsureSuccessStatusCode();

                var purchases = await response.Content.ReadFromJsonAsync<List<PurchaseModelDTO>>();
                return purchases ?? new List<PurchaseModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all purchases");
                throw;
            }
        }

        public async Task<PurchaseModelDTO?> GetPurchaseById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Purchases/GetPurchaseById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<PurchaseModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchase(PurchaseModelDTO purchase)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { PurchaseDTO = purchase };
                var response = await httpClient.PostAsJsonAsync("Purchases/InsertUpdatePurchase", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase");
                throw;
            }
        }

        public async Task DeletePurchase(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Purchases/DeletePurchase/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase: {Id}", id);
                throw;
            }
        }
    }
}
