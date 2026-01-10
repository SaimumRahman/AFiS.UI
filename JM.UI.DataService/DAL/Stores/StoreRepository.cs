using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Stores
{
    public class StoreRepository : BaseRepository, IStoreRepository
    {
        public StoreRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<StoreRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<StoreDTO>> GetStores()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all stores");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Stores/getall");
                response.EnsureSuccessStatusCode();

                var stores = await response.Content.ReadFromJsonAsync<List<StoreDTO>>();

                return stores ?? new List<StoreDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get stores");
                throw new Exception("Failed to fetch stores: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get stores");
                throw new Exception("Unexpected error fetching stores: " + ex.Message, ex);
            }
        }

        public async Task<StoreDTO?> GetStoreById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch store: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Stores/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Store not found: {Id}", id);
                    return null;
                }

                var store = await response.Content.ReadFromJsonAsync<StoreDTO>();
                return store;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get store by ID: {Id}", id);
                throw new Exception($"Failed to fetch store: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get store by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching store: {ex.Message}", ex);
            }
        }

        public async Task DeleteStore(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete store: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Stores/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Store deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete store: {Id}", id);
                throw new Exception($"Failed to delete store: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete store: {Id}", id);
                throw new Exception($"Unexpected error deleting store: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateStore(StoreDTO store)
        {
            try
            {
                _logger.LogInformation("Starting to save store");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    StoreDTO = store
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Stores/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save store");
                throw new Exception("Failed to save store: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save store");
                throw new Exception("Unexpected error saving store: " + ex.Message, ex);
            }
        }
    }
}
