using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Reporting
{
    public class ReportingRepository : BaseRepository, IReportingRepository
    {
        public ReportingRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ReportingRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId)
        {
            try
            {
                var url = "api/Reporting/profit-loss";
                if (storeId.HasValue && storeId.Value > 0)
                    url += $"?storeId={storeId.Value}";

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ProfitLossReportDTO>>();
                return result ?? new List<ProfitLossReportDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching profit loss report");
                throw new Exception("Failed to fetch profit loss report: " + ex.Message, ex);
            }
        }
    }
}
