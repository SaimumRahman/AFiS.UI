using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemFeatures;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.ItemFeatures
{
    public class ItemFeatureRepository : BaseRepository, IItemFeatureRepository
    {
        public ItemFeatureRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ItemFeatureRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ItemFeatureDTO>> GetItemFeatures()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ItemFeature/getall");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<ItemFeatureDTO>>() ?? new List<ItemFeatureDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item features");
                throw;
            }
        }

        public async Task<ResponseResult> SaveItemFeature(ItemFeatureDTO feature)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");

                var requestBody = new { ItemFeatureDTO = feature };
                var content = JsonContent.Create(requestBody);

                var endpoint = feature.FeatureId == 0
                    ? "ItemFeature/insert"
                    : "ItemFeature/update";

                var response = feature.FeatureId == 0
                    ? await httpClient.PostAsync(endpoint, content)
                    : await httpClient.PutAsync(endpoint, content);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item feature");
                throw;
            }
        }
    }
}
