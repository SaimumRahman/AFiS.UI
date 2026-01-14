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
                _logger.LogInformation("Service: Starting to fetch all AccountsGroups");
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("AccountsGroups/GetAllAccountsGroups");
                response.EnsureSuccessStatusCode();

                var groups = await response.Content.ReadFromJsonAsync<List<AccountsGroupsModelDTO>>();
                return groups ?? new List<AccountsGroupsModelDTO>();
            }

                var AccountsGroupss = await response.Content.ReadFromJsonAsync<List<AccountsGroupsDTO>>();
                _logger.LogInformation($"Service: Retrieved {AccountsGroupss?.Count ?? 0} AccountsGroupss");

                return AccountsGroupss ?? new List<AccountsGroupsDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Service: HTTP request failed during get AccountsGroups");
                throw new Exception("Failed to fetch AccountsGroups: " + ex.Message, ex);
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
                _logger.LogInformation("Starting to fetch AccountsGroups: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"AccountsGroups/GetAccountsGroupsById/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AccountsGroups not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<AccountsGroupsModelDTO>();
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

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsModelDTO accountsGroups)
        {
            try
            {
                _logger.LogInformation("Starting to delete AccountsGroups: {Id}", id);

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
                _logger.LogInformation("Starting to save AccountsGroups");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"AccountsGroups/DeleteAccountsGroups/{id}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save AccountsGroups");
                throw new Exception("Failed to save AccountsGroups: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accounts group: {Id}", id);
                throw;
            }
        }

    }
}
