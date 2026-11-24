using System;

namespace JM.UI.Entities.Model.Treasury
{
    public class TreasuryLedgerDTO
    {
        public string LedgerCode { get; set; } = string.Empty;
        public string LedgerName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class ContraPostRequestDTO
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public string ContraType { get; set; } = "CASH_TO_BANK";
        public string? FromLedgerCode { get; set; }
        public string? ToLedgerCode { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class ContraListRowDTO
    {
        public long EntryId { get; set; }
        public int? VoucherNo { get; set; }
        public DateTime EntryDate { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PettyCashRefillRequestDTO
    {
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class TreasuryResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long EntryId { get; set; }
    }

    // ============================ SETTLEMENT ============================
    public class SettlementPendingRowDTO
    {
        public int PaymentTransactionId { get; set; }
        public int? SalesMasterId { get; set; }
        public int? BookingMasterId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public int StoreId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int ChannelFinancialAccountId { get; set; }
        public string ChannelLedgerCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class SettlementTransferRequestDTO
    {
        public int PaymentTransactionId { get; set; }
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public int ReceivingFinancialAccountId { get; set; }
        public string CommissionLedgerCode { get; set; } = string.Empty;
        public decimal TransferAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class SettlementHistoryRowDTO
    {
        public int SettlementId { get; set; }
        public int PaymentTransactionId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public decimal SettlementAmount { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public string ReceivingAccount { get; set; } = string.Empty;
        public string CommissionLedger { get; set; } = string.Empty;
        public long EntryId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    // ============================ CASH SWAP ============================
    public class CashSwapSendRequestDTO
    {
        public int FromStoreId { get; set; }
        public int ToStoreId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class CashSwapRowDTO
    {
        public int SwapId { get; set; }
        public string SwapNo { get; set; } = string.Empty;
        public int FromStoreId { get; set; }
        public string FromBranchCode { get; set; } = string.Empty;
        public int ToStoreId { get; set; }
        public string ToBranchCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int SentBy { get; set; }
        public DateTime SentDate { get; set; }
        public int? ReceivedBy { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int? CancelledBy { get; set; }
        public DateTime? CancelledDate { get; set; }
    }

    public class CashSwapActionRequestDTO
    {
        public int SwapId { get; set; }
        public int UserId { get; set; }
    }

    // ============================ BRANCH LOAN ============================
    public class BranchLoanSendRequestDTO
    {
        public int SenderStoreId { get; set; }
        public int ReceiverStoreId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class BranchLoanRowDTO
    {
        public int LoanId { get; set; }
        public string LoanNo { get; set; } = string.Empty;
        public int SenderStoreId { get; set; }
        public string SenderBranchCode { get; set; } = string.Empty;
        public int ReceiverStoreId { get; set; }
        public string ReceiverBranchCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int SentBy { get; set; }
        public DateTime SentDate { get; set; }
        public int? AcceptedBy { get; set; }
        public DateTime? AcceptedDate { get; set; }
    }

    public class BranchLoanAcceptRequestDTO
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public DateTime? EntryDate { get; set; }
    }

    public class BranchLoanRepaySplitDTO
    {
        public string SourceLedgerCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BranchLoanRepayRequestDTO
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public string ReceivingLedgerCode { get; set; } = string.Empty;
        public List<BranchLoanRepaySplitDTO> Splits { get; set; } = new();
        public DateTime? EntryDate { get; set; }
    }

    public class BranchLoanAuditRowDTO
    {
        public DateTime Ts { get; set; }
        public string Action { get; set; } = string.Empty;
        public string LoanNo { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Receiver { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string By { get; set; } = string.Empty;
    }
}
