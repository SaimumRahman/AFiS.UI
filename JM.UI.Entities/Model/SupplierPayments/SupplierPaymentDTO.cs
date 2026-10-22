using System;

namespace JM.UI.Entities.Model.SupplierPayments
{
    public class SupplierPaymentDTO
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public int StoreId { get; set; }

        public decimal PaymentAmount { get; set; }
        public string? PaymentMethod { get; set; } = "Cash"; // Cash | FinancialAccount | Cheque
        public string? ReferenceNo { get; set; }

        /// <summary>Selected FinancialAccounts row for bank / MFS / cheque payments.</summary>
        public int? FinancialAccountId { get; set; }

        /// <summary>The GL ledger posted to (cash ledger or the financial account's ledger).</summary>
        public string? PaymentLedgerCode { get; set; }

        /// <summary>MFS / bank transaction reference.</summary>
        public string? TransactionNo { get; set; }

        public int? BankId { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }

        public string? Remarks { get; set; }
        public string? UserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }

        // UI Helpers
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? BankName { get; set; }

        public string? FinancialAccountName { get; set; }
        public string? LedgerName { get; set; }
        public string? PaymentSourceName { get; set; }
        public bool IsPostDatedCheque { get; set; }
    }
}
