using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.ItemOrigin
{
    public class ItemOriginRepository : BaseRepository, IItemOriginRepository
    {
        public ItemOriginRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ItemOriginRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ItemOriginDTO>> GetItemOrigins()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ItemOrigin/getall");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<ItemOriginDTO>>() ?? new List<ItemOriginDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item origins");
                throw;
            }
        }

        public async Task<ResponseResult> SaveItemOrigin(ItemOriginDTO origin)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");

                var requestBody = new { ItemOriginDTO = origin };
                var content = JsonContent.Create(requestBody);

                var endpoint = origin.OriginId == 0
                    ? "ItemOrigin/insert"
                    : "ItemOrigin/update";

                var response = origin.OriginId == 0
                    ? await httpClient.PostAsync(endpoint, content)
                    : await httpClient.PutAsync(endpoint, content);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item origin");
                throw;
            }
        }
    }
}
