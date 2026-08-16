using JM.UI.DataService.DAL;
using JM.UI.Entities.Model.Ecommerce;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Ecommerce
{
    public class EcommerceRepository : BaseRepository, IEcommerceRepository
    {
        public EcommerceRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<EcommerceRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = storeId.HasValue
                    ? $"Ecommerce/Store?storeId={storeId.Value}"
                    : "Ecommerce/Store";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<EcommerceStoreDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ecommerce store");
                throw;
            }
        }

        public async Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = BuildQueryString(filter);
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var items = await response.Content.ReadFromJsonAsync<List<EcommerceItemDTO>>();
                return items ?? new List<EcommerceItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ecommerce items");
                throw;
            }
        }

        private static string BuildQueryString(EcommerceFilterRequestDTO filter)
        {
            var sb = new StringBuilder("Ecommerce/Items?");
            if (filter.StoreId.HasValue)
                sb.Append($"storeId={filter.StoreId.Value}&");
            if (filter.GroupId.HasValue)
                sb.Append($"groupId={filter.GroupId.Value}&");
            if (filter.SubGroupId.HasValue)
                sb.Append($"subGroupId={filter.SubGroupId.Value}&");
            if (filter.DesignId.HasValue)
                sb.Append($"designId={filter.DesignId.Value}&");
            if (filter.BrandId.HasValue)
                sb.Append($"brandId={filter.BrandId.Value}&");
            if (filter.ColorId.HasValue)
                sb.Append($"colorId={filter.ColorId.Value}&");
            if (filter.SizeId.HasValue)
                sb.Append($"sizeId={filter.SizeId.Value}&");
            if (!string.IsNullOrWhiteSpace(filter.ReturnRefNo))
                sb.Append($"returnRefNo={Uri.EscapeDataString(filter.ReturnRefNo)}&");
            if (!string.IsNullOrWhiteSpace(filter.Barcode))
                sb.Append($"barcode={Uri.EscapeDataString(filter.Barcode)}&");

            return sb.ToString().TrimEnd('&');
        }
    }
}