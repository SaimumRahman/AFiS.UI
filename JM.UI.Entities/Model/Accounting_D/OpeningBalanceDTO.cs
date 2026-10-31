using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Accounting_D
{
    public class OpeningBalanceLineDTO
    {
        public string AccountCode { get; set; } = string.Empty;
        public int? PartyId { get; set; }
        public string? PartyTypeCode { get; set; }
        public string? PartyCode { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Memo { get; set; }
    }

    public class OpeningBalanceRequestDTO
    {
        public string CompanyCode { get; set; } = "AFHO";
        public string BranchCode { get; set; } = "HO";
        public int FiscalYearId { get; set; }
        public DateTime EntryDate { get; set; }
        public string? Narration { get; set; }
        public int CreatedBy { get; set; }
        public List<OpeningBalanceLineDTO> Lines { get; set; } = new();
    }

    public class OpeningBalanceViewDTO
    {
        public long EntryId { get; set; }
        public string? SourceRef { get; set; }
        public string? Status { get; set; }
        public DateTime EntryDate { get; set; }
        public string? BranchCode { get; set; }
        public int FiscalYearId { get; set; }
        public string? Narration { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        /// <summary>True when the branch/year already has non-OB posted activity.
        /// The page stays editable; this only drives a retroactive-change warning.</summary>
        public bool HasActivity { get; set; }
        public string? WarningMessage { get; set; }
        public List<OpeningBalanceLineDTO> Lines { get; set; } = new();
    }

    public class AccountingPartyDTO
    {
        public int PartyId { get; set; }
        public string PartyType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Display => $"{Code} - {Name}";
    }

    public class AccountingFiscalYearDTO
    {
        public short FiscalYearId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Display => Code;
    }

    public class AccountingBranchDTO
    {
        public short BranchId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Display => string.IsNullOrWhiteSpace(Name) ? Code : $"{Code} - {Name}";
    }
}
