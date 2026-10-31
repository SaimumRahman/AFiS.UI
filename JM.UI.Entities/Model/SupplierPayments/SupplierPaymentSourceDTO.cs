using System.Collections.Generic;

namespace JM.UI.Entities.Model.SupplierPayments
{
    /// <summary>
    /// One selectable payment source: a cash ledger or a FinancialAccounts row
    /// (MFS / bank). Balance is the branch balance of the GL ledger it posts to.
    /// </summary>
    public class SupplierPaymentSourceDTO
    {
        public string SourceType { get; set; } = "CASH";
        public int? FinancialAccountId { get; set; }
        public string? LedgerCode { get; set; }
        public string? LedgerName { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? AccountNo { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public bool IsBank { get; set; }
        public decimal Balance { get; set; }

        /// <summary>Text shown in the plain cash-ledger dropdown.</summary>
        public string CashDisplay => $"{LedgerName} ({LedgerCode})";

        /// <summary>Formatted balance for the dropdown grid (avoids a template).</summary>
        public string BalanceDisplay => Balance.ToString("N2");
    }

    public class SupplierPaymentSourcesDTO
    {
        public int StoreId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public bool IsSystemGroupUser { get; set; }
        public List<SupplierPaymentSourceDTO> CashLedgers { get; set; } = new();
        public List<SupplierPaymentSourceDTO> FinancialAccounts { get; set; } = new();
    }
}
