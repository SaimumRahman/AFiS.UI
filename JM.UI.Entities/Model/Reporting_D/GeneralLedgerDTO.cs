using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class GeneralLedgerDTO
    {
        public long EntryId { get; set; }
        public long LineId { get; set; }
        public DateTime EntryDate { get; set; }
        public string? VoucherTypeCode { get; set; }
        public int VoucherNo { get; set; }
        public string? Status { get; set; }
        public short LineNo { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Net { get; set; }
        public string? Narration { get; set; }
        public string? Memo { get; set; }
    }
}