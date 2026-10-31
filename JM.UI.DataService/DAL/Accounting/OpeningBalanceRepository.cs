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
    public class OpeningBalanceRepository : BaseRepository, IOpeningBalanceRepository
    {
        private readonly ILogger<OpeningBalanceRepository> _logger;

        public OpeningBalanceRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<OpeningBalanceRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<OpeningBalanceViewDTO?> GetOpeningBalance(string companyCode, string branchCode, int fiscalYearId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = $"api/OpeningBalance/GetOpeningBalance?companyCode={Uri.EscapeDataString(companyCode)}" +
                          $"&branchCode={Uri.EscapeDataString(branchCode)}&fiscalYearId={fiscalYearId}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<OpeningBalanceViewDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching opening balance {Branch}-FY{FiscalYearId}", branchCode, fiscalYearId);
                throw;
            }
        }

        public async Task<IEnumerable<AccountingFiscalYearDTO>> GetFiscalYears(string companyCode)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(
                    $"api/OpeningBalance/GetFiscalYears?companyCode={Uri.EscapeDataString(companyCode)}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<AccountingFiscalYearDTO>>();
                return result ?? new List<AccountingFiscalYearDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fiscal years for {CompanyCode}", companyCode);
                throw;
            }
        }

        public async Task<IEnumerable<AccountingBranchDTO>> GetBranches(string companyCode)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(
                    $"api/OpeningBalance/GetBranches?companyCode={Uri.EscapeDataString(companyCode)}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<AccountingBranchDTO>>();
                return result ?? new List<AccountingBranchDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching branches for {CompanyCode}", companyCode);
                throw;
            }
        }

        public async Task<IEnumerable<AccountingPartyDTO>> GetParties(string? partyType)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var url = "api/OpeningBalance/GetParties";
                if (!string.IsNullOrWhiteSpace(partyType))
                    url += $"?partyType={Uri.EscapeDataString(partyType)}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<AccountingPartyDTO>>();
                return result ?? new List<AccountingPartyDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching parties for type {PartyType}", partyType);
                throw;
            }
        }

        public async Task<ResponseResult> PostOpeningBalance(OpeningBalanceRequestDTO request)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("api/OpeningBalance/PostOpeningBalance", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                    ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting opening balance");
                throw;
            }
        }
    }
}
