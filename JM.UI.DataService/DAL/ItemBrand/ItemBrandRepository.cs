using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.ItemBrand
{
    public class ItemBrandRepository : BaseRepository, IItemBrandRepository
    {
        public ItemBrandRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ItemBrandRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ItemBrandDTO>> GetItemBrands()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ItemBrand/getall");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<ItemBrandDTO>>() ?? new List<ItemBrandDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item brands");
                throw;
            }
        }

        public async Task<ResponseResult> SaveItemBrand(ItemBrandDTO brand)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");

                var requestBody = new { ItemBrandDTO = brand };
                var content = JsonContent.Create(requestBody);

                var endpoint = brand.BrandId == 0
                    ? "ItemBrand/insert"
                    : "ItemBrand/update";

                var response = brand.BrandId == 0
                    ? await httpClient.PostAsync(endpoint, content)
                    : await httpClient.PutAsync(endpoint, content);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item brand");
                throw;
            }
        }
    }
}
