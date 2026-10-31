using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Accounting_D
{
    /// <summary>One manual-journal line. Exactly one of Debit / Credit must be positive.</summary>
    public class ManualJournalLineDTO
    {
        public long LineId { get; set; }
        public short LineNo { get; set; }
        public int AccountId { get; set; }

        public string AccountCode { get; set; } = string.Empty;
        public string? AccountName { get; set; }
        public string? Nature { get; set; }

        public int? PartyId { get; set; }
        public string? PartyType { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }

        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Memo { get; set; }

        public bool RequiresParty { get; set; }
        public bool RequiresBranch { get; set; }
        public bool IsGroup { get; set; }
        public bool IsActive { get; set; }
        public List<string> AllowedPartyTypes { get; set; } = new();
    }

    /// <summary>A manual journal header and its lines. Drafts have no voucher number;
    /// the number is assigned (gapless) only when the entry is posted.</summary>
    public class ManualJournalDTO
    {
        public long EntryId { get; set; }

        public string CompanyCode { get; set; } = "AFHO";
        public short CompanyId { get; set; }

        public short BranchId { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string? BranchName { get; set; }

        public string VoucherTypeCode { get; set; } = "JRN";
        public int? VoucherNo { get; set; }

        public short FiscalYearId { get; set; }
        public string? FiscalYearCode { get; set; }
        public int PeriodId { get; set; }
        public string? PeriodName { get; set; }

        public DateTime EntryDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Narration { get; set; }

        public string Status { get; set; } = "DRAFT";
        public string? SourceSystem { get; set; }
        public long? ReversalOf { get; set; }
        public long? ReversedByEntryId { get; set; }
        public int? ReversedByVoucherNo { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PostedBy { get; set; }
        public DateTime? PostedAt { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsEditable { get; set; }

        public List<ManualJournalLineDTO> Lines { get; set; } = new();

        public string VoucherDisplay => VoucherNo.HasValue ? $"JRN-{VoucherNo}" : "DRAFT";
    }

    public class ManualJournalSaveRequestDTO
    {
        public long EntryId { get; set; }
        public string CompanyCode { get; set; } = "AFHO";
        public string BranchCode { get; set; } = "HO";
        public string VoucherTypeCode { get; set; } = "JRN";
        public DateTime EntryDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Narration { get; set; }
        public int CreatedBy { get; set; }
        public bool PostNow { get; set; }
        public List<ManualJournalLineDTO> Lines { get; set; } = new();
    }

    public class ManualJournalErrorDTO
    {
        public int? LineNo { get; set; }
        public string? Field { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ManualJournalSaveResultDTO
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public long EntryId { get; set; }
        public int? VoucherNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public List<ManualJournalErrorDTO> Errors { get; set; } = new();
    }

    public class ManualJournalSummaryDTO
    {
        public long EntryId { get; set; }
        public int? VoucherNo { get; set; }
        public DateTime EntryDate { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Narration { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public int LineCount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PostedBy { get; set; }
        public DateTime? PostedAt { get; set; }
        public long? ReversalOf { get; set; }
        public long? ReversedByEntryId { get; set; }
        public int? ReversedByVoucherNo { get; set; }

        public string VoucherDisplay => VoucherNo.HasValue ? $"JRN-{VoucherNo}" : "(draft)";
    }

    public class ManualJournalListRequestDTO
    {
        public string CompanyCode { get; set; } = "AFHO";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? BranchCode { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ManualJournalListResultDTO
    {
        public int TotalCount { get; set; }
        public List<ManualJournalSummaryDTO> Items { get; set; } = new();
    }

    public class ManualJournalPeriodDTO
    {
        public int PeriodId { get; set; }
        public short FiscalYearId { get; set; }
        public string FiscalYearCode { get; set; } = string.Empty;
        public short PeriodNo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public string Display => $"{FiscalYearCode} · P{PeriodNo} ({StartDate:dd-MMM-yy} – {EndDate:dd-MMM-yy})";
    }

    /// <summary>Everything the entry screen needs in one round-trip.</summary>
    public class ManualJournalContextDTO
    {
        public string CompanyCode { get; set; } = "AFHO";
        public short CompanyId { get; set; }
        public List<AccountingBranchDTO> Branches { get; set; } = new();
        public List<ManualJournalPeriodDTO> OpenPeriods { get; set; } = new();
        public List<ChartOfAccountDTO> Accounts { get; set; } = new();
        public List<AccountingPartyDTO> Parties { get; set; } = new();
        public string DefaultBranchCode { get; set; } = "HO";
        public DateTime? DefaultEntryDate { get; set; }
    }
}
