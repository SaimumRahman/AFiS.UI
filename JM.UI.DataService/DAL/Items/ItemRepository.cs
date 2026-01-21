using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public class ItemRepository : BaseRepository, IItemRepository
    {
        public ItemRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ItemRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ItemModelDTO>> GetItems()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Items/GetAllItems");
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<ItemModelDTO>>();
                return items ?? new List<ItemModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all items");
                throw;
            }
        }

        public async Task<ItemModelDTO?> GetItemById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Items/GetItemById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ItemModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateItem(ItemModelDTO item)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("Items/SaveUpdateItem", item);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item");
                throw;
            }
        }

        public async Task DeleteItem(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Items/DeleteItem/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {Id}", id);
                throw;
            }
        }
    }
}
