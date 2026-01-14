using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Accounts;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Accounts
{
    public class AccountsRepository : BaseRepository, IAccountsRepository
    {
        public AccountsRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<AccountsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<AccountModelDTO>> GetAccounts()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Accounts/GetAllAccounts");
                response.EnsureSuccessStatusCode();

                var accounts = await response.Content.ReadFromJsonAsync<List<AccountModelDTO>>();
                return accounts ?? new List<AccountModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts");
                throw;
            }
        }

        public async Task<AccountModelDTO?> GetAccountById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Accounts/GetAccountById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<AccountModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateAccount(AccountModelDTO account)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { AccountDTO = account };
                var response = await httpClient.PostAsJsonAsync("Accounts/InsertUpdateAccount", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving account");
                throw;
            }
        }

        public async Task DeleteAccount(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Accounts/DeleteAccount/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account: {Id}", id);
                throw;
            }
        }
    }
}
