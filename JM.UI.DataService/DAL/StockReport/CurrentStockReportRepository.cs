using JM.UI.Entities.Model.StockReport_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.StockReport
{
    public class CurrentStockReportRepository : BaseRepository, ICurrentStockReportRepository
    {
        public CurrentStockReportRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CurrentStockReportRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<CurrentStockReportResponseDTO> GetCurrentStockReport(CurrentStockReportFilterDTO filter)
        {
            try
            {
                _logger.LogInformation("Fetching current stock report");

                var queryParams = new List<string>();
                if (filter.StoreId.HasValue) queryParams.Add($"storeId={filter.StoreId}");
                if (filter.GroupId.HasValue) queryParams.Add($"groupId={filter.GroupId}");
                if (filter.SubGroupId.HasValue) queryParams.Add($"subGroupId={filter.SubGroupId}");
                if (!string.IsNullOrWhiteSpace(filter.ProductType)) queryParams.Add($"productType={filter.ProductType}");

                var url = "StockReport/current-stock";
                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CurrentStockReportResponseDTO>();
                return result ?? new CurrentStockReportResponseDTO();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get current stock report");
                throw new Exception("Failed to fetch current stock report: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get current stock report");
                throw new Exception("Unexpected error fetching current stock report: " + ex.Message, ex);
            }
        }
    }
}
