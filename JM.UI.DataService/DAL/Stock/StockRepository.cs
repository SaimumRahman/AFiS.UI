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
        DateTime? fromDate, DateTime? toDate, int? itemId, int? storeId,
        int? groupId, int? subGroupId, int? designId, int? brandId,
        string? searchTerm = null)
    {
        try
        {
            var httpClient = GetAuthenticatedClient("MainApi");

            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            if (itemId.HasValue) queryParams.Add($"itemId={itemId.Value}");
            if (storeId.HasValue) queryParams.Add($"storeId={storeId.Value}");
            if (groupId.HasValue) queryParams.Add($"groupId={groupId.Value}");
            if (subGroupId.HasValue) queryParams.Add($"subGroupId={subGroupId.Value}");
            if (designId.HasValue) queryParams.Add($"designId={designId.Value}");
            if (brandId.HasValue) queryParams.Add($"brandId={brandId.Value}");
            if (!string.IsNullOrWhiteSpace(searchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm.Trim())}");

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

    public async Task<ItemStatementDTO> GetItemStatement(
        string baseBarcode, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var httpClient = GetAuthenticatedClient("MainApi");

            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(baseBarcode)) queryParams.Add($"baseBarcode={Uri.EscapeDataString(baseBarcode)}");
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var url = "Stock/item-statement";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ItemStatementDTO>();
            return result ?? new ItemStatementDTO();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching item statement");
            throw;
        }
    }
}