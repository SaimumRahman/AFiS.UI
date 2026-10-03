using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Accounting
{
    public class ChartOfAccountRepository : BaseRepository, IChartOfAccountRepository
    {
        private readonly ILogger<ChartOfAccountRepository> _logger;

        public ChartOfAccountRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ChartOfAccountRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ChartOfAccountDTO>> GetChartOfAccounts()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/ChartOfAccount/GetChartOfAccounts");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ChartOfAccountDTO>>();
                return result ?? new List<ChartOfAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chart of accounts");
                throw;
            }
        }

        public async Task<ChartOfAccountDTO?> GetChartOfAccountById(int accountId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/ChartOfAccount/GetChartOfAccountById/{accountId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ChartOfAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chart of account by ID: {AccountId}", accountId);
                throw;
            }
        }

        public async Task<IEnumerable<ChartOfAccountDTO>> GetGroupAccounts()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/ChartOfAccount/GetGroupAccounts");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ChartOfAccountDTO>>();
                return result ?? new List<ChartOfAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching group accounts");
                throw;
            }
        }

        public async Task<IEnumerable<ChartOfAccountDTO>> GetExpenseAccounts()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/ChartOfAccount/GetExpenseAccounts");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ChartOfAccountDTO>>();
                return result ?? new List<ChartOfAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expense accounts");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetPartyTypes()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/ChartOfAccount/GetPartyTypes");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<string>>();
                return result ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching party types");
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateChartOfAccount(ChartOfAccountDTO account)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { Account = account };
                var response = await httpClient.PostAsJsonAsync("api/ChartOfAccount/InsertUpdateChartOfAccount", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chart of account");
                throw;
            }
        }

        public async Task<ResponseResult> DeleteChartOfAccount(int accountId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/ChartOfAccount/DeleteChartOfAccount/{accountId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chart of account: {AccountId}", accountId);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
