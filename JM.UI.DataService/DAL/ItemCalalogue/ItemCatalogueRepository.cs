using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.ItemCalalogue
{
    public class ItemCatalogueRepository : BaseRepository, IItemCatalogueRepository
    {
        public ItemCatalogueRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ItemCatalogueRepository> logger)
            : base(httpClientFactory, tokenProvider, logger) { }

        public async Task<IEnumerable<ItemCatalogueDTO>> GetItemCatalogues()
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync("ItemCatalogues/getall");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ItemCatalogueDTO>>()
                       ?? new List<ItemCatalogueDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching catalogues");
                throw;
            }
        }

        public async Task<ItemCatalogueDTO?> GetItemCatalogueById(int id)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync($"ItemCatalogues/get/{id}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<ItemCatalogueDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching catalogue {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveItemCatalogue(ItemCatalogueDTO dto)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var content = JsonContent.Create(new { ItemCatalogueDTO = dto });
                var response = await client.PostAsync("ItemCatalogues/insert-update", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving catalogue");
                throw;
            }
        }

        public async Task DeleteItemCatalogue(int id)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.DeleteAsync($"ItemCatalogues/delete/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting catalogue {Id}", id);
                throw;
            }
        }
    }
}
