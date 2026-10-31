using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Accounting
{
    public class ManualJournalRepository : BaseRepository, IManualJournalRepository
    {
        private readonly ILogger<ManualJournalRepository> _logger;

        public ManualJournalRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ManualJournalRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<ManualJournalContextDTO> GetContext(string companyCode, int? storeId, DateTime? entryDate)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"api/ManualJournal/GetContext?companyCode={Uri.EscapeDataString(companyCode)}";
                if (storeId.HasValue && storeId.Value > 0)
                    url += $"&storeId={storeId.Value}";
                if (entryDate.HasValue)
                    url += $"&entryDate={entryDate.Value:yyyy-MM-dd}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalContextDTO>()
                    ?? new ManualJournalContextDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching manual journal context");
                throw;
            }
        }

        public async Task<ManualJournalListResultDTO> GetList(ManualJournalListRequestDTO request)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"api/ManualJournal/GetList?companyCode={Uri.EscapeDataString(request.CompanyCode)}" +
                          $"&fromDate={request.FromDate:yyyy-MM-dd}&toDate={request.ToDate:yyyy-MM-dd}" +
                          $"&page={request.Page}&pageSize={request.PageSize}";

                if (!string.IsNullOrWhiteSpace(request.BranchCode))
                    url += $"&branchCode={Uri.EscapeDataString(request.BranchCode)}";
                if (!string.IsNullOrWhiteSpace(request.Status))
                    url += $"&status={Uri.EscapeDataString(request.Status)}";
                if (!string.IsNullOrWhiteSpace(request.Search))
                    url += $"&search={Uri.EscapeDataString(request.Search)}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalListResultDTO>()
                    ?? new ManualJournalListResultDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing manual journals");
                throw;
            }
        }

        public async Task<ManualJournalDTO?> Get(long entryId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/ManualJournal/Get/{entryId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ManualJournalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching manual journal {EntryId}", entryId);
                throw;
            }
        }

        public async Task<ManualJournalSaveResultDTO> Save(ManualJournalSaveRequestDTO request)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/ManualJournal/Save", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalSaveResultDTO>()
                    ?? new ManualJournalSaveResultDTO { Success = false, Message = "No response from server." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving manual journal {EntryId}", request.EntryId);
                throw;
            }
        }

        public async Task<ManualJournalSaveResultDTO> Post(long entryId, int userId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/ManualJournal/Post",
                    new { EntryId = entryId, UserId = userId });
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalSaveResultDTO>()
                    ?? new ManualJournalSaveResultDTO { Success = false, Message = "No response from server." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting manual journal {EntryId}", entryId);
                throw;
            }
        }

        public async Task<ManualJournalSaveResultDTO> Reverse(long entryId, int userId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/ManualJournal/Reverse",
                    new { EntryId = entryId, UserId = userId });
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalSaveResultDTO>()
                    ?? new ManualJournalSaveResultDTO { Success = false, Message = "No response from server." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reversing manual journal {EntryId}", entryId);
                throw;
            }
        }

        public async Task<ManualJournalSaveResultDTO> DeleteDraft(long entryId, int userId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/ManualJournal/DeleteDraft/{entryId}?userId={userId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ManualJournalSaveResultDTO>()
                    ?? new ManualJournalSaveResultDTO { Success = false, Message = "No response from server." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting manual journal draft {EntryId}", entryId);
                throw;
            }
        }
    }
}
