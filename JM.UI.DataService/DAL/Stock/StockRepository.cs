using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.UI.DataService.DAL.Stores;
using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.StockReport_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Stock;

public class StockRepository : BaseRepository, IStockRepository
{
    public StockRepository(
        IHttpClientFactory httpClientFactory,
        ITokenProvider tokenProvider,
        ILogger<StockRepository> logger)
        : base(httpClientFactory, tokenProvider, logger)
    {
    }

    public async Task<IEnumerable<StockLedgerDTO>> GetStockLedger(
        DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId)
    {
        try
        {
            var httpClient = GetAuthenticatedClient("MainApi");

            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            if (itemId.HasValue) queryParams.Add($"itemId={itemId.Value}");
            if (storeId.HasValue) queryParams.Add($"storeId={storeId.Value}");

            var url = "Stock/ledger";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<StockLedgerDTO>>();
            return result ?? new List<StockLedgerDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stock ledger");
            throw;
        }
    }
}