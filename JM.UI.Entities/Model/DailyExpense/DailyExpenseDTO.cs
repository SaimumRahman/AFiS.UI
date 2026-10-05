using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.DailyExpense
{
    public class DailyExpenseDTO
    {
        public int DailyExpenseId { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int BillTypeId { get; set; }
        public string BillTypeName { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public int? FinancialAccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int? LedgerAccountId { get; set; }
        public string LedgerAccountCode { get; set; } = string.Empty;
        public string LedgerAccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReceiptBillNo { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Ledger posting state. An expense is saved unposted and only reaches the
        // GL through the Post Ledger page.
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public int? PostedBy { get; set; }
        public string PostedByName { get; set; }
        public long? AccVoucherEntryId { get; set; }
        public int? VoucherNo { get; set; }
        public string VoucherStatus { get; set; }
    }

    public class PostDailyExpensesDTO
    {
        public List<int> Ids { get; set; } = new();
        public int PostedBy { get; set; }
    }

    public class UnpostDailyExpenseDTO
    {
        public int Id { get; set; }
        public int UnpostedBy { get; set; }
    }

    public class BillTypeDTO
    {
        public int BillTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}