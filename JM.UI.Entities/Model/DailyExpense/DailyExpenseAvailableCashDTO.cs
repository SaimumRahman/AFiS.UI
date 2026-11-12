using System;

namespace JM.UI.Entities.Model.DailyExpense
{
    public class DailyExpenseAvailableCashDTO
    {
        public int StoreId { get; set; }
        public int? CostCenterId { get; set; }
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public bool CostCenterLinked { get; set; }
        public int LedgerAccountId { get; set; }
        public string? LedgerCode { get; set; }
        public string? LedgerName { get; set; }
        public decimal Funded { get; set; }
        public decimal Spent { get; set; }
        public decimal AvailableCash { get; set; }
    }
}