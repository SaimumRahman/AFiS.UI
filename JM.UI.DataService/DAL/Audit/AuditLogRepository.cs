using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Audit;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Audit
{
    public interface IAuditLogRepository
    {
        Task<AuditLogPageResult> GetAsync(string? tableName, long? rowId, string? businessKey, int? storeId, DateTime? fromDate, DateTime? toDate, string? searchTerm, int page, int pageSize);
        Task<AuditLogDTO> GetByIdAsync(long id);
    }

    public class AuditLogRepository : BaseRepository, IAuditLogRepository
    {
        public AuditLogRepository(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<AuditLogRepository> logger)
            : base(httpClientFactory, tokenProvider, logger) { }

        public async Task<AuditLogPageResult> GetAsync(string? tableName, long? rowId, string? businessKey, int? storeId, DateTime? fromDate, DateTime? toDate, string? searchTerm, int page, int pageSize)
        {
            var http = GetAuthenticatedClient("MainApi");
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(tableName)) qs.Add($"tableName={Uri.EscapeDataString(tableName)}");
            if (rowId.HasValue) qs.Add($"rowId={rowId.Value}");
            if (!string.IsNullOrWhiteSpace(businessKey)) qs.Add($"businessKey={Uri.EscapeDataString(businessKey)}");
            if (storeId.HasValue) qs.Add($"storeId={storeId.Value}");
            if (fromDate.HasValue) qs.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) qs.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrWhiteSpace(searchTerm)) qs.Add($"searchTerm={Uri.EscapeDataString(searchTerm.Trim())}");
            qs.Add($"page={page}");
            qs.Add($"pageSize={pageSize}");
            var url = "api/AuditLog" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
            var res = await http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AuditLogPageResult>() ?? new AuditLogPageResult();
        }

        public async Task<AuditLogDTO> GetByIdAsync(long id)
        {
            var http = GetAuthenticatedClient("MainApi");
            var res = await http.GetAsync($"api/AuditLog/{id}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<AuditLogDTO>() ?? new AuditLogDTO();
        }
    }
}
