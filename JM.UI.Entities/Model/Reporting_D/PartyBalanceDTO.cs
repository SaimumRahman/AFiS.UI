using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class PartyBalanceDTO
    {
        public int PartyId { get; set; }
        public string? PartyType { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        /// <summary>Net balance for the party/account (positive = receivable debit, negative = payable credit).</summary>
        public decimal Balance { get; set; }
    }
}