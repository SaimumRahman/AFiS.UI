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

        public async Task<IEnumerable<AccountsGroupsDTO>> GetAccountsGroups()
        {
            try
            {
                _logger.LogInformation("Service: Starting to fetch all AccountsGroups");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("AccountsGroups/getall");
                response.EnsureSuccessStatusCode();

                var groups = await response.Content.ReadFromJsonAsync<List<AccountsGroupsDTO>>();
                return groups ?? new List<AccountsGroupsDTO>();
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all accounts groups");
                throw;
            }
        }

        public async Task<AccountsGroupsDTO?> GetAccountsGroupsById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch AccountsGroups: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"AccountsGroups/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AccountsGroups not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<AccountsGroupsDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get AccountsGroups by ID: {Id}", id);
                throw new Exception($"Failed to fetch AccountsGroups: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accounts group by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsDTO accountsGroups)
        {
            try
            {

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { AccountsGroupsDTO = accountsGroups };
                var response = await httpClient.PostAsJsonAsync("AccountsGroups/insert-update", requestBody);
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
                _logger.LogInformation("Starting to save AccountsGroups");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"AccountsGroups/delete/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accounts group: {Id}", id);
                throw;
            }
        }
    }
}
