using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Reporting
{
    public class ReportingRepository : BaseRepository, IReportingRepository
    {
        public ReportingRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ReportingRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var url = "api/Reporting/profit-loss";
                var query = new List<string>();
                if (storeId.HasValue && storeId.Value > 0)
                    query.Add($"storeId={storeId.Value}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ProfitLossReportDTO>>();
                return result ?? new List<ProfitLossReportDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching profit loss report");
                throw new Exception("Failed to fetch profit loss report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<BookingReportDTO>> GetBookingReport(int? storeId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var url = "api/BookingReport/booking-report";
                var query = new List<string>();
                if (storeId.HasValue && storeId.Value > 0)
                    query.Add($"storeId={storeId.Value}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<BookingReportDTO>>();
                return result ?? new List<BookingReportDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking report");
                throw new Exception("Failed to fetch booking report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<CustomerWiseSalesDetailDTO>> GetCustomerWiseSalesDetail(int? storeId, int? customerId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var url = "api/CustomerWiseSalesDetail/detail";
                var query = new List<string>();
                if (storeId.HasValue && storeId.Value > 0)
                    query.Add($"storeId={storeId.Value}");
                if (customerId.HasValue && customerId.Value > 0)
                    query.Add($"customerId={customerId.Value}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<CustomerWiseSalesDetailDTO>>();
                return result ?? new List<CustomerWiseSalesDetailDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer-wise sales detail report");
                throw new Exception("Failed to fetch customer-wise sales detail report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<ExchangeReportDTO>> GetExchangeReport(int? storeId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var url = "api/ExchangeReport/detail";
                var query = new List<string>();
                if (storeId.HasValue && storeId.Value > 0)
                    query.Add($"storeId={storeId.Value}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<ExchangeReportDTO>>();
                return result ?? new List<ExchangeReportDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange report");
                throw new Exception("Failed to fetch exchange report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<GroupWiseSalesSummaryDTO>> GetGroupWiseSalesSummary(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var url = "api/GroupWiseSalesSummary/detail";
                var query = new List<string>();
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<GroupWiseSalesSummaryDTO>>();
                return result ?? new List<GroupWiseSalesSummaryDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching group-wise sales summary report");
                throw new Exception("Failed to fetch group-wise sales summary report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<TrialBalanceDTO>> GetTrialBalance(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode)
        {
            try
            {
                var url = "api/AccountingReport/trial-balance";
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyCode))
                    query.Add($"companyCode={Uri.EscapeDataString(companyCode)}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(branchCode))
                    query.Add($"branchCode={Uri.EscapeDataString(branchCode)}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<TrialBalanceDTO>>();
                return result ?? new List<TrialBalanceDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trial balance report");
                throw new Exception("Failed to fetch trial balance report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<GeneralLedgerDTO>> GetGeneralLedger(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode, int? accountId, int? voucherTypeId)
        {
            try
            {
                var url = "api/AccountingReport/general-ledger";
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyCode))
                    query.Add($"companyCode={Uri.EscapeDataString(companyCode)}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(branchCode))
                    query.Add($"branchCode={Uri.EscapeDataString(branchCode)}");
                if (accountId.HasValue && accountId.Value > 0)
                    query.Add($"accountId={accountId.Value}");
                if (voucherTypeId.HasValue && voucherTypeId.Value > 0)
                    query.Add($"voucherTypeId={voucherTypeId.Value}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<GeneralLedgerDTO>>();
                return result ?? new List<GeneralLedgerDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching general ledger report");
                throw new Exception("Failed to fetch general ledger report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<PartyBalanceDTO>> GetPartyBalance(string? companyCode, string? partyType)
        {
            try
            {
                var url = "api/AccountingReport/party-balance";
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyCode))
                    query.Add($"companyCode={Uri.EscapeDataString(companyCode)}");
                if (!string.IsNullOrWhiteSpace(partyType))
                    query.Add($"partyType={Uri.EscapeDataString(partyType)}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<PartyBalanceDTO>>();
                return result ?? new List<PartyBalanceDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching party balance report");
                throw new Exception("Failed to fetch party balance report: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<BalanceRollupDTO>> GetBalanceRollup(string? companyCode, int? periodId)
        {
            try
            {
                var url = "api/AccountingReport/balance-rollup";
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(companyCode))
                    query.Add($"companyCode={Uri.EscapeDataString(companyCode)}");
                if (periodId.HasValue && periodId.Value > 0)
                    query.Add($"periodId={periodId.Value}");
                if (query.Any())
                    url += "?" + string.Join("&", query);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<BalanceRollupDTO>>();
                return result ?? new List<BalanceRollupDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance rollup report");
                throw new Exception("Failed to fetch balance rollup report: " + ex.Message, ex);
            }
        }
    }
}
