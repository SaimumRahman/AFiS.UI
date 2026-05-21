using JM.Infrastructure.Models; 
using JM.UI.Entities.Model.Barcodes;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace JM.UI.DataService.DAL.Barcode
{
    public class BarcodePrintConfigRepository : BaseRepository, IBarcodePrintConfigRepository
    {
        public BarcodePrintConfigRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<BarcodePrintConfigRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<BarcodePrintConfigDTO>> GetAllBarcodePrintConfigs()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all barcode print configs");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("BarcodePrintConfig/getall");
                response.EnsureSuccessStatusCode();

                var configs = await response.Content.ReadFromJsonAsync<List<BarcodePrintConfigDTO>>();

                return configs ?? new List<BarcodePrintConfigDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get all barcode print configs");
                throw new Exception("Failed to fetch barcode print configs: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get all barcode print configs");
                throw new Exception("Unexpected error fetching barcode print configs: " + ex.Message, ex);
            }
        }

        public async Task<BarcodePrintConfigDTO?> GetTopBarcodePrintConfig()
        {
            try
            {
                _logger.LogInformation("Starting to fetch top barcode print config");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("BarcodePrintConfig/gettop");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Top barcode print config not found");
                    return null;
                }

                var config = await response.Content.ReadFromJsonAsync<BarcodePrintConfigDTO>();
                return config;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get top barcode print config");
                throw new Exception($"Failed to fetch top barcode print config: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get top barcode print config");
                throw new Exception($"Unexpected error fetching top barcode print config: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<BarcodeItemDTO>> GetBarcodeItemsByPurchaseId(int purchaseId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch barcode items for purchase: {PurchaseId}", purchaseId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"BarcodePrintConfig/{purchaseId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Barcode items not found for purchase: {PurchaseId}", purchaseId);
                    return new List<BarcodeItemDTO>();
                }

                var items = await response.Content.ReadFromJsonAsync<List<BarcodeItemDTO>>();
                return items ?? new List<BarcodeItemDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get barcode items by purchase ID: {PurchaseId}", purchaseId);
                throw new Exception($"Failed to fetch barcode items: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get barcode items by purchase ID: {PurchaseId}", purchaseId);
                throw new Exception($"Unexpected error fetching barcode items: {ex.Message}", ex);
            }
        }
    }
}