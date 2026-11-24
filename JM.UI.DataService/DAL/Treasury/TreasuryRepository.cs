using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JM.UI.Entities.Model.Treasury;
using JM.UI.DataService.DAL;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Treasury
{
    public interface ITreasuryRepository
    {
        Task<List<TreasuryLedgerDTO>> GetContraAccountsAsync(int storeId, string branchCode, DateTime asOfDate);
        Task<TreasuryResultDTO> PostContraAsync(ContraPostRequestDTO request);
        Task<TreasuryResultDTO> ReverseContraAsync(long entryId, int userId);
        Task<List<ContraListRowDTO>> ListContraAsync(string? branchCode, DateTime from, DateTime to);
        Task<TreasuryResultDTO> RefillPettyCashAsync(PettyCashRefillRequestDTO request);

        // Settlement
        Task<List<SettlementPendingRowDTO>> GetSettlementPendingAsync(int? storeId, DateTime? from, DateTime? to);
        Task<TreasuryResultDTO> PostSettlementAsync(SettlementTransferRequestDTO request);
        Task<TreasuryResultDTO> ReverseSettlementAsync(int settlementId, int userId);
        Task<List<SettlementHistoryRowDTO>> ListSettlementAsync(int? storeId, DateTime? from, DateTime? to);

        // Cash swap
        Task<List<CashSwapRowDTO>> ListCashSwapsAsync(int? storeId);
        Task<TreasuryResultDTO> SendCashSwapAsync(CashSwapSendRequestDTO request);
        Task<TreasuryResultDTO> ReceiveCashSwapAsync(CashSwapActionRequestDTO request);
        Task<TreasuryResultDTO> CancelCashSwapAsync(CashSwapActionRequestDTO request);

        // Branch loan
        Task<List<BranchLoanRowDTO>> ListBranchLoansAsync(int? storeId);
        Task<List<BranchLoanAuditRowDTO>> ListBranchLoanAuditAsync();
        Task<TreasuryResultDTO> SendBranchLoanAsync(BranchLoanSendRequestDTO request);
        Task<TreasuryResultDTO> AcceptBranchLoanAsync(BranchLoanAcceptRequestDTO request);
        Task<TreasuryResultDTO> RepayBranchLoanAsync(BranchLoanRepayRequestDTO request);
        Task<TreasuryResultDTO> ReverseBranchLoanAsync(int loanId, int userId);
    }

    public class TreasuryRepository : BaseRepository, ITreasuryRepository
    {
        private readonly ILogger<TreasuryRepository> _logger;

        public TreasuryRepository(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<TreasuryRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        private HttpClient Client => GetAuthenticatedClient();

        public async Task<List<TreasuryLedgerDTO>> GetContraAccountsAsync(int storeId, string branchCode, DateTime asOfDate)
            => await Client.GetFromJsonAsync<List<TreasuryLedgerDTO>>($"Contra/accounts?storeId={storeId}&branchCode={Uri.EscapeDataString(branchCode)}&asOfDate={asOfDate:yyyy-MM-dd}") ?? new();

        public async Task<TreasuryResultDTO> PostContraAsync(ContraPostRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("Contra/post", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> ReverseContraAsync(long entryId, int userId)
        {
            var res = await Client.PostAsync($"Contra/reverse/{entryId}?userId={userId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<List<ContraListRowDTO>> ListContraAsync(string? branchCode, DateTime from, DateTime to)
        {
            var url = $"Contra/list?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(branchCode)) url += $"&branchCode={Uri.EscapeDataString(branchCode)}";
            return await Client.GetFromJsonAsync<List<ContraListRowDTO>>(url) ?? new();
        }

        public async Task<TreasuryResultDTO> RefillPettyCashAsync(PettyCashRefillRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("Contra/refill-petty-cash", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<List<SettlementPendingRowDTO>> GetSettlementPendingAsync(int? storeId, DateTime? from, DateTime? to)
        {
            var url = "Settlement/pending";
            var q = new List<string>();
            if (storeId.HasValue) q.Add($"storeId={storeId}");
            if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
            if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
            if (q.Count > 0) url += "?" + string.Join("&", q);
            return await Client.GetFromJsonAsync<List<SettlementPendingRowDTO>>(url) ?? new();
        }

        public async Task<TreasuryResultDTO> PostSettlementAsync(SettlementTransferRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("Settlement/transfer", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> ReverseSettlementAsync(int settlementId, int userId)
        {
            var res = await Client.PostAsync($"Settlement/reverse/{settlementId}?userId={userId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<List<SettlementHistoryRowDTO>> ListSettlementAsync(int? storeId, DateTime? from, DateTime? to)
        {
            var url = "Settlement/history";
            var q = new List<string>();
            if (storeId.HasValue) q.Add($"storeId={storeId}");
            if (from.HasValue) q.Add($"from={from:yyyy-MM-dd}");
            if (to.HasValue) q.Add($"to={to:yyyy-MM-dd}");
            if (q.Count > 0) url += "?" + string.Join("&", q);
            return await Client.GetFromJsonAsync<List<SettlementHistoryRowDTO>>(url) ?? new();
        }

        public async Task<List<CashSwapRowDTO>> ListCashSwapsAsync(int? storeId)
        {
            var url = "CashSwap/list";
            if (storeId.HasValue) url += $"?storeId={storeId}";
            return await Client.GetFromJsonAsync<List<CashSwapRowDTO>>(url) ?? new();
        }

        public async Task<TreasuryResultDTO> SendCashSwapAsync(CashSwapSendRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("CashSwap/send", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> ReceiveCashSwapAsync(CashSwapActionRequestDTO request)
        {
            var res = await Client.PostAsync($"CashSwap/receive/{request.SwapId}?userId={request.UserId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> CancelCashSwapAsync(CashSwapActionRequestDTO request)
        {
            var res = await Client.PostAsync($"CashSwap/cancel/{request.SwapId}?userId={request.UserId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<List<BranchLoanRowDTO>> ListBranchLoansAsync(int? storeId)
        {
            var url = "BranchLoan/list";
            if (storeId.HasValue) url += $"?storeId={storeId}";
            return await Client.GetFromJsonAsync<List<BranchLoanRowDTO>>(url) ?? new();
        }

        public async Task<List<BranchLoanAuditRowDTO>> ListBranchLoanAuditAsync()
            => await Client.GetFromJsonAsync<List<BranchLoanAuditRowDTO>>("BranchLoan/audit") ?? new();

        public async Task<TreasuryResultDTO> SendBranchLoanAsync(BranchLoanSendRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("BranchLoan/send", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> AcceptBranchLoanAsync(BranchLoanAcceptRequestDTO request)
        {
            var res = await Client.PostAsync($"BranchLoan/accept/{request.LoanId}?userId={request.UserId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> RepayBranchLoanAsync(BranchLoanRepayRequestDTO request)
        {
            var res = await Client.PostAsJsonAsync("BranchLoan/repay", request);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }

        public async Task<TreasuryResultDTO> ReverseBranchLoanAsync(int loanId, int userId)
        {
            var res = await Client.PostAsync($"BranchLoan/reverse/{loanId}?userId={userId}", null);
            if (!res.IsSuccessStatusCode) return new TreasuryResultDTO { Success = false, Message = await res.Content.ReadAsStringAsync() };
            return await res.Content.ReadFromJsonAsync<TreasuryResultDTO>() ?? new TreasuryResultDTO { Success = true };
        }
    }
}
