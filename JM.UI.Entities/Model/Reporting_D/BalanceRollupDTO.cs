using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class BalanceRollupDTO
    {
        public int PeriodId { get; set; }
        public string? RollupCode { get; set; }
        public string? RollupName { get; set; }
        public bool IsGroup { get; set; }
        public string? BranchCode { get; set; }

        /// <summary>Net group total for the period (positive = debit, negative = credit).</summary>
        public decimal Net { get; set; }
    }
}