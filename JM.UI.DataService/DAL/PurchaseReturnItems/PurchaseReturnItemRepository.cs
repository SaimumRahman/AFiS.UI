using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseReturnItems;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.PurchaseReturnItems
{
    public class PurchaseReturnItemRepository : BaseRepository, IPurchaseReturnItemRepository
    {
        public PurchaseReturnItemRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseReturnItemRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<PurchaseReturnItemModelDTO>> GetAllReturnItems()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("PurchaseReturnItems/GetAllReturnItems");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<PurchaseReturnItemModelDTO>>() ?? new List<PurchaseReturnItemModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all purchase return items");
                throw;
            }
        }

        public async Task<IEnumerable<PurchaseReturnItemModelDTO>> GetItemsByReturnId(int returnId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PurchaseReturnItems/GetItemsByReturnId/{returnId}");
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<PurchaseReturnItemModelDTO>>();
                return items ?? new List<PurchaseReturnItemModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items for PurchaseReturnId: {ReturnId}", returnId);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateItem(PurchaseReturnItemModelDTO item)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("PurchaseReturnItems/SaveUpdateItem", item);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase return item");
                throw;
            }
        }

        public async Task DeleteItem(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"PurchaseReturnItems/DeleteItem/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase return item: {Id}", id);
                throw;
            }
        }
    }
}
