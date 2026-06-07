using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public async Task<IEnumerable<ItemDTO>> GetItems()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Items/GetAllItems");
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<ItemDTO>>();
                return items ?? new List<ItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all items");
                throw;
            }
        }
        public async Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Items/GetItemsByStoreId/{storeId}");
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<ItemDTO>>();
                return items ?? new List<ItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items for store: {StoreId}", storeId);
                throw;
            }
        }

        public async Task<ItemDTO?> GetItemById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Items/GetItemById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> CreateItem(CreateItemRequestDTO createItemRequest)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var request = new
                {
                    CreateItemRequest = createItemRequest
                };
                var response = await httpClient.PostAsJsonAsync("Items/SaveUpdateItem", request);
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

        public async Task<ResponseResult> DeleteItem(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Items/DeleteItem/{id}");
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ResponseResult>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {Id}", id);
                throw;
            }
        }
        public async Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Items/GetItemsBySubGroupId/{subGroupId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var subGroups = await response.Content.ReadFromJsonAsync<List<ItemDTO>>();
                return subGroups ?? new List<ItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sub-group by ID: {Id}", subGroupId);
                throw;
            }
        }

    }
}
