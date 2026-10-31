using System;

namespace JM.UI.Entities.Model.FinancialAccounts
{
    public class FinancialAccountDropdownDTO
    {
        public int Id { get; set; }
        public int FinancialAccountTypeId { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        /// <summary>Display name: the MFS type name for MFS accounts, the bank name otherwise.</summary>
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;

        /// <summary>Bank name when the account is a bank account.</summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>MFS type name when the account is a mobile financial service account.</summary>
        public string MFSTypeName { get; set; } = string.Empty;

        /// <summary>Store (branch) the financial account belongs to.</summary>
        public string StoreName { get; set; } = string.Empty;

        public int? StoreId { get; set; }

        /// <summary>GL ledger account code payments on this account post to.</summary>
        public string LedgerCode { get; set; } = string.Empty;
    }
}
