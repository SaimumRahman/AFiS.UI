using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Sms;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Sms
{
    public interface ISmsLogRepository
    {
        Task<SmsLogPageResult> GetAsync(DateTime? fromDate, DateTime? toDate, string storeIds, string searchTerm, int page, int pageSize);
        Task<SmsLogDTO> GetByIdAsync(long id);
    }

    public class SmsLogRepository : BaseRepository, ISmsLogRepository
    {
        public SmsLogRepository(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<SmsLogRepository> logger)
            : base(httpClientFactory, tokenProvider, logger) { }

        public async Task<SmsLogPageResult> GetAsync(DateTime? fromDate, DateTime? toDate, string storeIds, string searchTerm, int page, int pageSize)
        {
            var http = GetAuthenticatedClient("MainApi");
            var qs = new List<string>();
            if (fromDate.HasValue) qs.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) qs.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(storeIds)) qs.Add($"storeIds={Uri.EscapeDataString(storeIds)}");
            if (!string.IsNullOrWhiteSpace(searchTerm)) qs.Add($"searchTerm={Uri.EscapeDataString(searchTerm.Trim())}");
            qs.Add($"page={page}");
            qs.Add($"pageSize={pageSize}");
            var url = "api/SmsLog" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
            var res = await http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SmsLogPageResult>() ?? new SmsLogPageResult();
        }

        public async Task<SmsLogDTO> GetByIdAsync(long id)
        {
            var http = GetAuthenticatedClient("MainApi");
            var res = await http.GetAsync($"api/SmsLog/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SmsLogDTO>() ?? new SmsLogDTO();
        }
    }
}
