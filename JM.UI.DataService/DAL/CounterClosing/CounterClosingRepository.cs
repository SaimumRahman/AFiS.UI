using JM.Infrastructure.Models;
using JM.UI.Entities.Model.CounterClosing;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.CounterClosing
{
    public class CounterClosingRepository : BaseRepository, ICounterClosingRepository
    {
        public CounterClosingRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<CounterClosingRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<CounterDTO>> GetCounters(int? storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = storeId.HasValue
                    ? $"CounterClosing/counters?storeId={storeId.Value}"
                    : "CounterClosing/counters";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var counters = await response.Content.ReadFromJsonAsync<List<CounterDTO>>();
                return counters ?? new List<CounterDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sales counters");
                throw;
            }
        }

        public async Task<IEnumerable<CounterDTO>> GetCountersForManagement(int storeId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"CounterClosing/counters/manage?storeId={storeId}");
                response.EnsureSuccessStatusCode();

                var counters = await response.Content.ReadFromJsonAsync<List<CounterDTO>>();
                return counters ?? new List<CounterDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching counters for management");
                throw;
            }
        }

        public async Task<bool> CanManageCounters(int userId, string? userName)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"CounterClosing/canmanage?userId={userId}";
                if (!string.IsNullOrWhiteSpace(userName))
                    url += $"&userName={Uri.EscapeDataString(userName)}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking counter management permission");
                return false;
            }
        }

        public async Task<ResponseResult> SaveCounter(SaveCounterDTO counter)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("CounterClosing/counter",
                    new { Counter = counter });

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "Empty server response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving counter");
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<CounterClosingPreviewDTO?> GetPreview(int storeId, int counterId, DateTime? businessDate)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"CounterClosing/preview?storeId={storeId}&counterId={counterId}";
                if (businessDate.HasValue)
                    url += $"&businessDate={businessDate.Value:yyyy-MM-dd}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<CounterClosingPreviewDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching closing preview for counter {CounterId}", counterId);
                throw;
            }
        }

        public async Task<IEnumerable<CounterClosingDTO>> GetHistory(int? counterId, int? storeId, int take)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"CounterClosing/history?take={take}";
                if (counterId.HasValue)
                    url += $"&counterId={counterId.Value}";
                if (storeId.HasValue)
                    url += $"&storeId={storeId.Value}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var history = await response.Content.ReadFromJsonAsync<List<CounterClosingDTO>>();
                return history ?? new List<CounterClosingDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching closing history");
                throw;
            }
        }

        public async Task<ResponseResult> CloseCounter(SubmitCounterCloseDTO closing)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("CounterClosing/close",
                    new { Closing = closing });

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "Empty server response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing counter {CounterId}", closing.CounterId);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> ReopenCounter(ReopenCounterCloseDTO reopen)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("CounterClosing/reopen",
                    new { Reopen = reopen });

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "Empty server response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening closing {ClosingId}", reopen.ClosingId);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
