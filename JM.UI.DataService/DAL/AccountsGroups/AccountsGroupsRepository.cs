using JM.Infrastructure.Models;
using JM.UI.Entities.Model.AccountsGroups;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Service: API returned {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"API returned {response.StatusCode}");
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
                _logger.LogError(ex, "Service: Unexpected error during get AccountsGroups");
                throw new Exception("Unexpected error fetching AccountsGroups: " + ex.Message, ex);
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

                var AccountsGroups = await response.Content.ReadFromJsonAsync<AccountsGroupsDTO>();
                return AccountsGroups;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get AccountsGroups by ID: {Id}", id);
                throw new Exception($"Failed to fetch AccountsGroups: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get AccountsGroups by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching AccountsGroups: {ex.Message}", ex);
            }
        }

        public async Task DeleteAccountsGroups(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete AccountsGroups: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"AccountsGroups/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("AccountsGroups deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete AccountsGroups: {Id}", id);
                throw new Exception($"Failed to delete AccountsGroups: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete AccountsGroups: {Id}", id);
                throw new Exception($"Unexpected error deleting AccountsGroups: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateAccountsGroups(AccountsGroupsDTO AccountsGroups)
        {
            try
            {
                _logger.LogInformation("Starting to save AccountsGroups");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    AccountsGroupsDTO = AccountsGroups
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("AccountsGroups/insert-update", content);
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
                _logger.LogError(ex, "Unexpected error during save AccountsGroups");
                throw new Exception("Unexpected error saving AccountsGroups: " + ex.Message, ex);
            }
        }

    }
}
