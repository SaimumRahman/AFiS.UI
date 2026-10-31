using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounting;

public partial class ManualJournalEntryComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected const string CompanyCode = "AFHO";

    [SupplyParameterFromQuery(Name = "entryId")]
    public long? EntryId { get; set; }

    protected ManualJournalContextDTO? Context { get; set; }
    protected List<AccountingBranchDTO> Branches { get; set; } = new();
    protected List<AccountingPartyDTO> Parties { get; set; } = new();
    protected List<ManualJournalPeriodDTO> OpenPeriods { get; set; } = new();
    protected List<AccountOption> AccountOptions { get; set; } = new();
    private Dictionary<string, AccountOption> _accountLookup = new(StringComparer.OrdinalIgnoreCase);

    protected string? SelectedBranchCode { get; set; }
    protected DateTime EntryDate { get; set; } = DateTime.Today;
    protected string? ReferenceNo { get; set; }
    protected string? Narration { get; set; }

    protected long CurrentEntryId { get; set; }
    protected int? VoucherNo { get; set; }
    protected string Status { get; set; } = "DRAFT";
    protected string? SourceSystem { get; set; }
    protected long? ReversalOf { get; set; }
    protected int? ReversedByVoucherNo { get; set; }
    protected bool IsEditable { get; set; } = true;
    protected string? LoadedPeriodName { get; set; }

    protected List<MjRow> Rows { get; set; } = new();

    protected bool IsLoading { get; set; }
    protected bool IsProcessing { get; set; }
    protected bool IsDirty { get; set; }
    protected int CurrentUserId { get; set; }

    protected string VoucherDisplay => VoucherNo.HasValue ? $"JRN-{VoucherNo}" : "DRAFT";

    protected bool IsManualEntry =>
        string.Equals(SourceSystem, "MANUAL", StringComparison.OrdinalIgnoreCase);

    // ── Live totals ─────────────────────────────────────────────────────────

    protected List<MjRow> ActiveRows => Rows
        .Where(r => !string.IsNullOrWhiteSpace(r.AccountCode)
                    || (r.Debit ?? 0) != 0
                    || (r.Credit ?? 0) != 0)
        .ToList();

    protected bool HasLines => ActiveRows.Count > 0;
    protected decimal TotalDebit => ActiveRows.Sum(r => r.Debit ?? 0);
    protected decimal TotalCredit => ActiveRows.Sum(r => r.Credit ?? 0);
    protected decimal Difference => TotalDebit - TotalCredit;
    protected bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;

    protected bool HasNoNetEffect
    {
        get
        {
            var lines = ActiveRows.Where(r => AccountOf(r) != null).ToList();
            if (lines.Count < 2) return false;

            var nets = lines
                .GroupBy(r => r.AccountCode!, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Sum(r => (r.Debit ?? 0) - (r.Credit ?? 0)))
                .ToList();

            return nets.Count > 0 && nets.All(v => v == 0m);
        }
    }

    protected ManualJournalPeriodDTO? SelectedPeriod => OpenPeriods.FirstOrDefault(
        p => EntryDate.Date >= p.StartDate.Date && EntryDate.Date <= p.EndDate.Date);

    // ── Lifecycle ───────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");

        await LoadContextAsync();

        if (EntryId.HasValue && EntryId.Value > 0)
            await LoadEntryAsync(EntryId.Value);
        else
            StartNewEntry();
    }

    public void Dispose()
    {
        NavigationGuard.IsGuardActive = false;
    }

    private async Task LoadContextAsync()
    {
        try
        {
            IsLoading = true;
            Context = await _serviceUnitOfWork.ManualJournalService.GetContext(
                CompanyCode, await GetLocalStorageInt("StoreId"), null);

            Branches = Context?.Branches ?? new List<AccountingBranchDTO>();
            Parties = (Context?.Parties ?? new List<AccountingPartyDTO>())
                .OrderBy(p => p.PartyType).ThenBy(p => p.Code).ToList();
            OpenPeriods = Context?.OpenPeriods ?? new List<ManualJournalPeriodDTO>();

            AccountOptions = (Context?.Accounts ?? new List<ChartOfAccountDTO>())
                .OrderBy(a => a.Code)
                .Select(a => new AccountOption
                {
                    AccountId = a.AccountId,
                    Code = a.Code,
                    Name = a.Name,
                    Nature = a.Nature,
                    RequiresParty = a.RequiresParty,
                    RequiresBranch = a.RequiresBranch,
                    PartyTypes = a.PartyTypes ?? new List<string>()
                })
                .ToList();

            _accountLookup = AccountOptions
                .GroupBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load accounts: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void StartNewEntry()
    {
        CurrentEntryId = 0;
        VoucherNo = null;
        Status = "DRAFT";
        SourceSystem = "MANUAL";
        ReversalOf = null;
        ReversedByVoucherNo = null;
        IsEditable = true;

        SelectedBranchCode = Context?.DefaultBranchCode ?? Branches.FirstOrDefault()?.Code;
        EntryDate = AsDateOnly(Context?.DefaultEntryDate ?? DateTime.Today);
        ReferenceNo = null;
        Narration = null;
        LoadedPeriodName = null;
        Rows = NewBlankRows();

        IsDirty = false;
        NavigationGuard.IsGuardActive = false;
    }

    private async Task LoadEntryAsync(long entryId)
    {
        try
        {
            IsLoading = true;

            var entry = await _serviceUnitOfWork.ManualJournalService.Get(entryId);
            if (entry == null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Not found", "That journal no longer exists.");
                NavigationGuard.IsGuardActive = false;
                NavigationManager.NavigateTo("/ManualJournalList");
                return;
            }

            EntryId = entry.EntryId;
            CurrentEntryId = entry.EntryId;
            SelectedBranchCode = entry.BranchCode;
            EntryDate = AsDateOnly(entry.EntryDate);
            ReferenceNo = entry.ReferenceNo;
            Narration = entry.Narration;
            Status = entry.Status;
            SourceSystem = entry.SourceSystem;
            VoucherNo = entry.VoucherNo;
            ReversalOf = entry.ReversalOf;
            ReversedByVoucherNo = entry.ReversedByVoucherNo;
            IsEditable = entry.IsEditable;
            LoadedPeriodName = entry.PeriodName;

            Rows = entry.Lines
                .OrderBy(l => l.LineNo)
                .Select(l => new MjRow
                {
                    AccountCode = l.AccountCode,
                    PartyId = l.PartyId,
                    Debit = l.Debit > 0 ? l.Debit : null,
                    Credit = l.Credit > 0 ? l.Credit : null,
                    Memo = l.Memo
                })
                .ToList();

            if (Rows.Count == 0)
                Rows = NewBlankRows();

            IsDirty = false;
            NavigationGuard.IsGuardActive = false;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load journal: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    // ── Header changes ──────────────────────────────────────────────────────

    protected void OnBranchChanged(string? value)
    {
        SelectedBranchCode = value;
        MarkDirty();
    }

    protected void OnEntryDateChanged(DateTime value)
    {
        EntryDate = AsDateOnly(value);
        MarkDirty();
    }

    // ── Line handlers ───────────────────────────────────────────────────────

    protected void OnAccountChanged(MjRow row, string? code)
    {
        row.AccountCode = code;

        var option = AccountOf(row);
        if (option == null)
        {
            row.PartyId = null;
        }
        else if (row.PartyId.HasValue &&
                 !GetPartyOptions(row).Any(p => p.PartyId == row.PartyId.Value))
        {
            // The previously chosen party is not valid for the new account.
            row.PartyId = null;
        }

        MarkDirty();
    }

    protected void OnPartyChanged(MjRow row, int? partyId)
    {
        row.PartyId = partyId;
        MarkDirty();
    }

    protected void OnDebitChanged(MjRow row, decimal? value)
    {
        row.Debit = value.HasValue && value.Value != 0 ? Math.Round(value.Value, 2) : null;
        if (row.Debit.HasValue && row.Debit.Value > 0) row.Credit = null;
        MarkDirty();
    }

    protected void OnCreditChanged(MjRow row, decimal? value)
    {
        row.Credit = value.HasValue && value.Value != 0 ? Math.Round(value.Value, 2) : null;
        if (row.Credit.HasValue && row.Credit.Value > 0) row.Debit = null;
        MarkDirty();
    }

    protected void AddRow()
    {
        Rows.Add(new MjRow());
        MarkDirty();
    }

    protected void InsertRowAfter(MjRow row)
    {
        var index = Rows.IndexOf(row);
        Rows.Insert(index < 0 ? Rows.Count : index + 1, new MjRow());
        MarkDirty();
    }

    protected void DuplicateRow(MjRow row)
    {
        var index = Rows.IndexOf(row);
        Rows.Insert(index < 0 ? Rows.Count : index + 1, new MjRow
        {
            AccountCode = row.AccountCode,
            PartyId = row.PartyId,
            Memo = row.Memo
        });
        MarkDirty();
    }

    protected void RemoveRow(MjRow row)
    {
        if (Rows.Count <= 2) return;
        Rows.Remove(row);
        MarkDirty();
    }

    // ── Account helpers ─────────────────────────────────────────────────────

    protected AccountOption? AccountOf(MjRow row)
    {
        if (string.IsNullOrWhiteSpace(row.AccountCode)) return null;
        return _accountLookup.TryGetValue(row.AccountCode.Trim(), out var option) ? option : null;
    }

    protected bool ShowParty(MjRow row)
    {
        var option = AccountOf(row);
        return option != null && (option.RequiresParty || option.PartyTypes.Count > 0);
    }

    protected bool PartyRequired(MjRow row) => AccountOf(row)?.RequiresParty == true;

    protected IEnumerable<AccountingPartyDTO> GetPartyOptions(MjRow row)
    {
        var option = AccountOf(row);
        if (option == null) return Enumerable.Empty<AccountingPartyDTO>();

        if (option.PartyTypes.Count == 0)
            return option.RequiresParty ? Parties : Enumerable.Empty<AccountingPartyDTO>();

        return Parties.Where(p =>
            option.PartyTypes.Contains(p.PartyType, StringComparer.OrdinalIgnoreCase));
    }

    private int LineNo(MjRow row) => Rows.IndexOf(row) + 1;

    // ── Validation ──────────────────────────────────────────────────────────

    protected string? Validate()
    {
        if (string.IsNullOrWhiteSpace(SelectedBranchCode)) return "Select a branch.";
        if (string.IsNullOrWhiteSpace(Narration)) return "Narration is required.";
        if (Narration.Trim().Length > 2000) return "Narration cannot exceed 2000 characters.";
        if (SelectedPeriod == null) return $"No open fiscal period covers {EntryDate:dd-MMM-yyyy}.";

        var lines = ActiveRows;
        if (lines.Count < 2) return "A journal needs at least two lines.";

        foreach (var row in lines)
        {
            var option = AccountOf(row);
            var label = $"Line {LineNo(row)}";

            if (option == null)
                return $"{label}: select a valid ledger account.";

            var debit = row.Debit ?? 0;
            var credit = row.Credit ?? 0;

            if (debit < 0 || credit < 0)
                return $"{label}: amounts cannot be negative.";
            if (debit > 0 && credit > 0)
                return $"{label}: enter either a debit or a credit, not both.";
            if (debit == 0 && credit == 0)
                return $"{label}: enter a debit or a credit.";

            if (option.RequiresParty && (!row.PartyId.HasValue || row.PartyId.Value <= 0))
                return $"{label}: party is required for {option.Display}.";

            if (row.PartyId.HasValue && row.PartyId.Value > 0 && option.PartyTypes.Count > 0)
            {
                var party = Parties.FirstOrDefault(p => p.PartyId == row.PartyId.Value);
                if (party == null)
                    return $"{label}: the selected party is not available.";
                if (!option.PartyTypes.Contains(party.PartyType, StringComparer.OrdinalIgnoreCase))
                    return $"{label}: party type {party.PartyType} is not allowed on {option.Code}.";
            }
        }

        if (TotalDebit != TotalCredit)
            return $"The journal is not balanced: debit {TotalDebit:N2} vs credit {TotalCredit:N2}.";
        if (TotalDebit <= 0)
            return "The journal total must be greater than zero.";
        if (HasNoNetEffect)
            return "Every account nets to zero — this entry has no accounting effect.";

        return null;
    }

    // ── Save / post / reverse ───────────────────────────────────────────────

    protected async Task Save(bool postNow)
    {
        if (!IsEditable) return;

        var error = Validate();
        if (error != null)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", error);
            return;
        }

        var request = new ManualJournalSaveRequestDTO
        {
            EntryId = CurrentEntryId,
            CompanyCode = CompanyCode,
            BranchCode = SelectedBranchCode!,
            VoucherTypeCode = "JRN",
            EntryDate = AsDateOnly(EntryDate),
            ReferenceNo = string.IsNullOrWhiteSpace(ReferenceNo) ? null : ReferenceNo.Trim(),
            Narration = Narration!.Trim(),
            CreatedBy = CurrentUserId,
            PostNow = postNow,
            Lines = ActiveRows.Select(r => new ManualJournalLineDTO
            {
                AccountCode = r.AccountCode!.Trim(),
                PartyId = r.PartyId,
                Debit = r.Debit ?? 0,
                Credit = r.Credit ?? 0,
                Memo = string.IsNullOrWhiteSpace(r.Memo) ? null : r.Memo.Trim()
            }).ToList()
        };

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.ManualJournalService.Save(request);

            var message = result.Errors != null && result.Errors.Count > 0
                ? string.Join(" | ", result.Errors
                    .Select(e => e.LineNo.HasValue ? $"Line {e.LineNo}: {e.Message}" : e.Message)
                    .Take(5))
                : result.Message;

            notificationService.Notify(
                result.Success ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.Success ? (postNow ? "Posted" : "Saved") : "Error",
                message ?? (result.Success ? "Journal saved." : "The journal could not be saved."));

            if (!result.Success) return;

            IsDirty = false;
            NavigationGuard.IsGuardActive = false;

            if (result.EntryId > 0)
            {
                EntryId = result.EntryId;
                await LoadEntryAsync(result.EntryId);
                NavigationGuard.IsGuardActive = false;
                NavigationManager.NavigateTo($"/ManualJournalEntry?entryId={result.EntryId}", replace: true);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save journal: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    protected async Task ReverseJournal()
    {
        if (!IsEditable && Status == "POSTED" && IsManualEntry)
        {
            var confirmed = await dialogService.Confirm(
                $"Reverse {VoucherDisplay}? A contra journal is posted and this entry becomes REVERSED. " +
                "You can then enter a corrected journal.",
                "Reverse Journal",
                new ConfirmOptions { OkButtonText = "Reverse", CancelButtonText = "Cancel" });

            if (confirmed != true) return;

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.ManualJournalService.Reverse(CurrentEntryId, CurrentUserId);

                notificationService.Notify(
                    result.Success ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.Success ? "Reversed" : "Error",
                    result.Message ?? (result.Success ? "Journal reversed." : "The journal could not be reversed."));

                if (result.Success)
                    await LoadEntryAsync(CurrentEntryId);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to reverse journal: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }
    }

    protected void GoToList()
    {
        // Only prompt when there is something to lose.
        NavigationGuard.IsGuardActive = IsEditable && IsDirty;
        NavigationManager.NavigateTo("/ManualJournalList");
    }

    // ── Small helpers ───────────────────────────────────────────────────────

    protected void MarkDirty()
    {
        IsDirty = true;
        if (IsEditable)
            NavigationGuard.IsGuardActive = true;
    }

    private static List<MjRow> NewBlankRows() => new() { new MjRow(), new MjRow() };

    private static DateTime AsDateOnly(DateTime value)
        => DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(key);
            return int.TryParse(result.Value, out var value) ? value : 0;
        }
        catch
        {
            return 0;
        }
    }

    // ── View models ─────────────────────────────────────────────────────────

    public class AccountOption
    {
        public int AccountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public bool RequiresParty { get; set; }
        public bool RequiresBranch { get; set; }
        public List<string> PartyTypes { get; set; } = new();
        public string Display => $"{Code} - {Name}";
    }

    public class MjRow
    {
        public Guid Key { get; } = Guid.NewGuid();
        public string? AccountCode { get; set; }
        public int? PartyId { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string? Memo { get; set; }
    }
}
