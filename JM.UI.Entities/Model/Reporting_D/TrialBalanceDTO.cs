using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class TrialBalanceDTO
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Nature { get; set; }

        /// <summary>Balance carried in from before the period (B/S: all history; P&amp;L: fiscal-year start).</summary>
        public decimal Opening { get; set; }

        /// <summary>Sum of debit lines posted within the period.</summary>
        public decimal PeriodDebit { get; set; }

        /// <summary>Sum of credit lines posted within the period.</summary>
        public decimal PeriodCredit { get; set; }

        /// <summary>Opening + PeriodDebit - PeriodCredit.</summary>
        public decimal Closing { get; set; }
    }
}