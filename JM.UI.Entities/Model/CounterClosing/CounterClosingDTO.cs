using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.CounterClosing
{
    /// <summary>A physical sales counter belonging to a store.</summary>
    public class CounterDTO
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>Create / edit payload for a counter (manager only).</summary>
    public class SaveCounterDTO
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }

    /// <summary>Cash / card / MFS total for one payment method.</summary>
    public class CounterPaymentSummaryDTO
    {
        public int TransactionTypeId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsCash { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    /// <summary>One invoice / return document shown in the day's transaction grid.</summary>
    public class CounterTransactionDTO
    {
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalBill { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsReturnExchange { get; set; }
        public bool IsDraft { get; set; }
        public bool IsBooking { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
    }

    public class CounterCashMovementDTO
    {
        public string Direction { get; set; } = "OUT"; // IN / OUT
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Everything the closing screen needs for one (counter, business day):
    /// transactions, per-method totals and the server-computed expected cash.
    /// </summary>
    public class CounterClosingPreviewDTO
    {
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public int CounterId { get; set; }
        public string? CounterName { get; set; }

        public DateTime BusinessDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public bool IsClosed { get; set; }
        public long? ClosingId { get; set; }
        public string? ClosingStatus { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedByName { get; set; }
        public decimal? ActualCash { get; set; }
        public decimal? Difference { get; set; }
        public string? Note { get; set; }

        public decimal OpeningCash { get; set; }
        public decimal CashSales { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal ExpectedCash { get; set; }

        public int CashTransactionCount { get; set; }
        public decimal NonCashTotal { get; set; }
        public int PendingCount { get; set; }

        public List<CounterPaymentSummaryDTO> PaymentSummary { get; set; } = new();
        public List<CounterPaymentSummaryDTO> CashOutSummary { get; set; } = new();
        public List<CounterTransactionDTO> Transactions { get; set; } = new();
        public List<CounterCashMovementDTO> CashMovements { get; set; } = new();
    }

    /// <summary>A stored closing record (history grid).</summary>
    public class CounterClosingDTO
    {
        public long Id { get; set; }
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public int CounterId { get; set; }
        public string? CounterName { get; set; }
        public DateTime BusinessDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public decimal OpeningCash { get; set; }
        public decimal CashSales { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal ActualCash { get; set; }
        public decimal Difference { get; set; }

        public string? Note { get; set; }
        public int ClosedBy { get; set; }
        public string? ClosedByName { get; set; }
        public DateTime ClosedAt { get; set; }

        public string Status { get; set; } = "CLOSED";
        public int? ReopenedBy { get; set; }
        public string? ReopenedByName { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public string? ReopenReason { get; set; }
    }

    /// <summary>Payload for closing a counter.</summary>
    public class SubmitCounterCloseDTO
    {
        public int StoreId { get; set; }
        public int CounterId { get; set; }
        public DateTime BusinessDate { get; set; }
        public decimal ActualCash { get; set; }
        public string? Note { get; set; }
        public int ClosedBy { get; set; }
        public string? ClosedByName { get; set; }
        public List<CounterCashMovementDTO> CashMovements { get; set; } = new();
    }

    /// <summary>Payload for an admin reopen (audited).</summary>
    public class ReopenCounterCloseDTO
    {
        public long ClosingId { get; set; }
        public int ReopenedBy { get; set; }
        public string? ReopenedByName { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
