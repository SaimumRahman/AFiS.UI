using JM.UI.DataService.DAL.Treasury;
using JM.UI.Entities.Model.Treasury;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Treasury
{
    public interface ITreasuryService
    {
        Task<List<TreasuryLedgerDTO>> GetContraAccountsAsync(int storeId, string branchCode, DateTime asOfDate);
        Task<TreasuryResultDTO> PostContraAsync(ContraPostRequestDTO request);
        Task<TreasuryResultDTO> ReverseContraAsync(long entryId, int userId);
        Task<List<ContraListRowDTO>> ListContraAsync(string? branchCode, DateTime from, DateTime to);
        Task<TreasuryResultDTO> RefillPettyCashAsync(PettyCashRefillRequestDTO request);

        Task<List<SettlementPendingRowDTO>> GetSettlementPendingAsync(int? storeId, DateTime? from, DateTime? to);
        Task<TreasuryResultDTO> PostSettlementAsync(SettlementTransferRequestDTO request);
        Task<TreasuryResultDTO> ReverseSettlementAsync(int settlementId, int userId);
        Task<List<SettlementHistoryRowDTO>> ListSettlementAsync(int? storeId, DateTime? from, DateTime? to);

        Task<List<CashSwapRowDTO>> ListCashSwapsAsync(int? storeId);
        Task<TreasuryResultDTO> SendCashSwapAsync(CashSwapSendRequestDTO request);
        Task<TreasuryResultDTO> ReceiveCashSwapAsync(CashSwapActionRequestDTO request);
        Task<TreasuryResultDTO> CancelCashSwapAsync(CashSwapActionRequestDTO request);

        Task<List<BranchLoanRowDTO>> ListBranchLoansAsync(int? storeId);
        Task<List<BranchLoanAuditRowDTO>> ListBranchLoanAuditAsync();
        Task<TreasuryResultDTO> SendBranchLoanAsync(BranchLoanSendRequestDTO request);
        Task<TreasuryResultDTO> AcceptBranchLoanAsync(BranchLoanAcceptRequestDTO request);
        Task<TreasuryResultDTO> RepayBranchLoanAsync(BranchLoanRepayRequestDTO request);
        Task<TreasuryResultDTO> ReverseBranchLoanAsync(int loanId, int userId);
    }

    public class TreasuryService : ITreasuryService
    {
        private readonly ITreasuryRepository _repo;
        public TreasuryService(ITreasuryRepository repo) => _repo = repo;

        public Task<List<TreasuryLedgerDTO>> GetContraAccountsAsync(int storeId, string branchCode, DateTime asOfDate)
            => _repo.GetContraAccountsAsync(storeId, branchCode, asOfDate);

        public Task<TreasuryResultDTO> PostContraAsync(ContraPostRequestDTO request)
            => _repo.PostContraAsync(request);

        public Task<TreasuryResultDTO> ReverseContraAsync(long entryId, int userId)
            => _repo.ReverseContraAsync(entryId, userId);

        public Task<List<ContraListRowDTO>> ListContraAsync(string? branchCode, DateTime from, DateTime to)
            => _repo.ListContraAsync(branchCode, from, to);

        public Task<TreasuryResultDTO> RefillPettyCashAsync(PettyCashRefillRequestDTO request)
            => _repo.RefillPettyCashAsync(request);

        public Task<List<SettlementPendingRowDTO>> GetSettlementPendingAsync(int? storeId, DateTime? from, DateTime? to)
            => _repo.GetSettlementPendingAsync(storeId, from, to);
        public Task<TreasuryResultDTO> PostSettlementAsync(SettlementTransferRequestDTO request)
            => _repo.PostSettlementAsync(request);
        public Task<TreasuryResultDTO> ReverseSettlementAsync(int settlementId, int userId)
            => _repo.ReverseSettlementAsync(settlementId, userId);
        public Task<List<SettlementHistoryRowDTO>> ListSettlementAsync(int? storeId, DateTime? from, DateTime? to)
            => _repo.ListSettlementAsync(storeId, from, to);

        public Task<List<CashSwapRowDTO>> ListCashSwapsAsync(int? storeId) => _repo.ListCashSwapsAsync(storeId);
        public Task<TreasuryResultDTO> SendCashSwapAsync(CashSwapSendRequestDTO request) => _repo.SendCashSwapAsync(request);
        public Task<TreasuryResultDTO> ReceiveCashSwapAsync(CashSwapActionRequestDTO request) => _repo.ReceiveCashSwapAsync(request);
        public Task<TreasuryResultDTO> CancelCashSwapAsync(CashSwapActionRequestDTO request) => _repo.CancelCashSwapAsync(request);

        public Task<List<BranchLoanRowDTO>> ListBranchLoansAsync(int? storeId) => _repo.ListBranchLoansAsync(storeId);
        public Task<List<BranchLoanAuditRowDTO>> ListBranchLoanAuditAsync() => _repo.ListBranchLoanAuditAsync();
        public Task<TreasuryResultDTO> SendBranchLoanAsync(BranchLoanSendRequestDTO request) => _repo.SendBranchLoanAsync(request);
        public Task<TreasuryResultDTO> AcceptBranchLoanAsync(BranchLoanAcceptRequestDTO request) => _repo.AcceptBranchLoanAsync(request);
        public Task<TreasuryResultDTO> RepayBranchLoanAsync(BranchLoanRepayRequestDTO request) => _repo.RepayBranchLoanAsync(request);
        public Task<TreasuryResultDTO> ReverseBranchLoanAsync(int loanId, int userId) => _repo.ReverseBranchLoanAsync(loanId, userId);
    }
}
