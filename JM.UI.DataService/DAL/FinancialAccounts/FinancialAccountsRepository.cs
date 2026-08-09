using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.FinancialAccounts;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.FinancialAccounts
{
    public class FinancialAccountsRepository : BaseRepository, IFinancialAccountsRepository
    {
        private readonly ILogger<FinancialAccountsRepository> _logger;

        public FinancialAccountsRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<FinancialAccountsRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<FinancialAccountDTO>> GetFinancialAccounts()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("FinancialAccounts/GetAllFinancialAccounts");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<FinancialAccountDTO>>();
                return result ?? new List<FinancialAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all financial accounts");
                throw;
            }
        }

        public async Task<FinancialAccountDTO?> GetFinancialAccountById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"FinancialAccounts/GetFinancialAccountById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<FinancialAccountDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching financial account by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateFinancialAccount(FinancialAccountDTO financialAccount)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { FinancialAccountDTO = financialAccount };
                var response = await httpClient.PostAsJsonAsync("FinancialAccounts/InsertUpdateFinancialAccount", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving financial account");
                throw;
            }
        }

        public async Task<ResponseResult> DeleteFinancialAccount(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"FinancialAccounts/DeleteFinancialAccount/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting financial account: {Id}", id);
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<IEnumerable<FinancialAccountTypeDTO>> GetFinancialAccountTypes()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("FinancialAccounts/GetFinancialAccountTypes");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<FinancialAccountTypeDTO>>();
                return result ?? new List<FinancialAccountTypeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching financial account types");
                throw;
            }
        }

        public async Task<IEnumerable<MFSTypeDTO>> GetMFSTypes()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("FinancialAccounts/GetMFSTypes");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<MFSTypeDTO>>();
                return result ?? new List<MFSTypeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MFS types");
                throw;
            }
        }

        public async Task<IEnumerable<BanksDTO>> GetBanks()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("FinancialAccounts/GetBanks");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<BanksDTO>>();
                return result ?? new List<BanksDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching banks for financial accounts");
                throw;
            }
        }

        public async Task<IEnumerable<FinancialAccountDropdownDTO>> GetFinancialAccountsForDropdown()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("FinancialAccounts/GetFinancialAccountsForDropdown");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<FinancialAccountDropdownDTO>>();
                return result ?? new List<FinancialAccountDropdownDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching financial accounts for dropdown");
                throw;
            }
        }
    }
}