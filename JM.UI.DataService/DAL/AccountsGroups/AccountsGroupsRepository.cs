using JM.Infrastructure.Models;
using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.AccountsGroups
{
    public class AccountsGroupsRepository : BaseRepository, IAccountsGroupsRepository
    {
        public AccountsGroupsRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<AccountsGroupsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<AccountsGroupsModelDTO>> GetAccountsGroups()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("AccountsGroups/GetAllAccountsGroups");
                response.EnsureSuccessStatusCode();

                var groups = await response.Content.ReadFromJsonAsync<List<AccountsGroupsModelDTO>>();
                return groups ?? new List<AccountsGroupsModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts groups");
                throw;
            }
        }

        public async Task<AccountsGroupsModelDTO?> GetAccountsGroupsById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"AccountsGroups/GetAccountsGroupsById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<AccountsGroupsModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accounts group by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsModelDTO accountsGroups)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { AccountsGroupsDTO = accountsGroups };
                var response = await httpClient.PostAsJsonAsync("AccountsGroups/InsertUpdateAccountsGroups", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving accounts group");
                throw;
            }
        }

        public async Task DeleteAccountsGroups(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"AccountsGroups/DeleteAccountsGroups/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accounts group: {Id}", id);
                throw;
            }
        }
    }
}
