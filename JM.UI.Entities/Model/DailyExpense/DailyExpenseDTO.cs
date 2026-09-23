using System;

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
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReceiptBillNo { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class BillTypeDTO
    {
        public int BillTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}