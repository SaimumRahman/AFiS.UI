using JM.UI.Entities.Model.Accounting_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Accounting;

public partial class OpeningBalanceComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected const string CompanyCode = "AFHO";
    protected const string ContraAccountCode = "314001";

    protected List<AccountingBranchDTO> Branches { get; set; } = new();
    protected List<AccountingFiscalYearDTO> FiscalYears { get; set; } = new();
    protected List<AccountingPartyDTO> Parties { get; set; } = new();

    // Chart of accounts (active groups + ledgers) for the tree picker.
    protected List<ChartOfAccountDTO> AllAccounts { get; set; } = new();
    protected List<AccountTreeNode> AccountTree { get; set; } = new();
    protected Dictionary<string, ChartOfAccountDTO> LedgerLookup { get; set; } = new();

    protected string? SelectedBranchCode { get; set; }
    protected short SelectedFiscalYearId { get; set; }
    protected int CurrentUserId { get; set; }

    protected bool IsLoading { get; set; }
    protected bool IsProcessing { get; set; }

    protected OpeningBalanceViewDTO? Existing { get; set; }
    protected bool IsLocked { get; set; }
    protected string? LockReason { get; set; }
    protected long ExistingEntryId { get; set; }
    protected string? ExistingStatus { get; set; }

    protected DateTime EntryDate { get; set; } = DateTime.Today;
    protected string? Narration { get; set; }
    protected List<ObRow> Rows { get; set; } = new();

    // ── Tree picker state (one shared popup, anchored to the active row) ────
    protected RadzenPopup? popup;
    protected ObRow? ActiveRow;
    protected object? TreeSelection;
    protected bool IsTreeOpen;

    protected AccountingFiscalYearDTO? SelectedFiscalYear
        => FiscalYears.FirstOrDefault(f => f.FiscalYearId == SelectedFiscalYearId);

    protected decimal TotalDebit => Rows.Sum(r => r.Debit ?? 0);
    protected decimal TotalCredit => Rows.Sum(r => r.Credit ?? 0);
    protected decimal Difference => TotalDebit - TotalCredit;
    protected decimal ContraAmount => Math.Abs(Difference);
    protected bool ContraIsDebit => Difference < 0;

    public class AccountTreeNode
    {
        public int AccountId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public List<AccountTreeNode> Children { get; set; } = new();
        public string Display => $"{Code} - {Name}";
    }

    public class ObRow
    {
        public string? AccountCode { get; set; }
        public int? PartyId { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string? Memo { get; set; }
        public ElementReference Trigger { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        await LoadLookups();
        await LoadOpeningBalance();
    }

    private async Task LoadLookups()
    {
        try
        {
            IsLoading = true;

            Branches = (await _serviceUnitOfWork.OpeningBalanceService.GetBranches(CompanyCode))?.ToList()
                       ?? new List<AccountingBranchDTO>();
            FiscalYears = (await _serviceUnitOfWork.OpeningBalanceService.GetFiscalYears(CompanyCode))?.ToList()
                          ?? new List<AccountingFiscalYearDTO>();
            Parties = (await _serviceUnitOfWork.OpeningBalanceService.GetParties(null))?.ToList()
                      ?? new List<AccountingPartyDTO>();

            AllAccounts = (await _serviceUnitOfWork.ChartOfAccountService.GetChartOfAccounts())?
                              .Where(a => a.IsActive)
                              .ToList() ?? new List<ChartOfAccountDTO>();

            LedgerLookup = AllAccounts
                .Where(a => !a.IsGroup)
                .GroupBy(a => a.Code)
                .ToDictionary(g => g.Key, g => g.First());

            AccountTree = BuildAccountTree(AllAccounts);

            SelectedBranchCode ??= Branches.FirstOrDefault()?.Code;
            if (SelectedFiscalYearId == 0)
                SelectedFiscalYearId = FiscalYears.FirstOrDefault()?.FiscalYearId ?? 0;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookups: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static List<AccountTreeNode> BuildAccountTree(List<ChartOfAccountDTO> accounts)
    {
        var byParent = accounts
            .GroupBy(a => a.ParentId ?? 0)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<AccountTreeNode> Build(int parentId)
        {
            if (!byParent.TryGetValue(parentId, out var children))
                return new List<AccountTreeNode>();

            return children
                .OrderBy(c => c.Code.Length).ThenBy(c => c.Code)
                .Select(c => new AccountTreeNode
                {
                    AccountId = c.AccountId,
                    Code = c.Code,
                    Name = c.Name,
                    IsGroup = c.IsGroup,
                    Children = Build(c.AccountId)
                })
                .ToList();
        }

        return Build(0);
    }

    protected async Task LoadOpeningBalance()
    {
        if (string.IsNullOrWhiteSpace(SelectedBranchCode) || SelectedFiscalYearId == 0)
            return;

        try
        {
            IsLoading = true;

            Existing = await _serviceUnitOfWork.OpeningBalanceService.GetOpeningBalance(
                CompanyCode, SelectedBranchCode, SelectedFiscalYearId);

            IsLocked = Existing?.IsLocked == true;
            LockReason = Existing?.LockReason;
            ExistingEntryId = Existing?.EntryId ?? 0;
            ExistingStatus = Existing?.Status;

            if (Existing != null && Existing.EntryId > 0)
            {
                Rows = Existing.Lines
                    .Where(l => !string.Equals(l.AccountCode, ContraAccountCode, StringComparison.Ordinal))
                    .Select(l => new ObRow
                    {
                        AccountCode = l.AccountCode,
                        PartyId = l.PartyId,
                        Debit = l.Debit > 0 ? l.Debit : null,
                        Credit = l.Credit > 0 ? l.Credit : null,
                        Memo = l.Memo
                    })
                    .ToList();

                if (Rows.Count == 0)
                    Rows.Add(new ObRow());

                EntryDate = AsDateOnly(Existing.EntryDate);
                Narration = Existing.Narration;
            }
            else
            {
                Rows = new List<ObRow> { new ObRow() };
                EntryDate = AsDateOnly(SelectedFiscalYear?.StartDate ?? DateTime.Today);
                Narration = $"Opening Balance - {SelectedBranchCode} - {SelectedFiscalYear?.Code}";
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task OnHeaderChanged() => await LoadOpeningBalance();

    protected void AddRow() => Rows.Add(new ObRow());

    protected void RemoveRow(ObRow row) => Rows.Remove(row);

    // ── Ledger tree picker ─────────────────────────────────────────────────

    protected async Task OpenTree(ObRow row)
    {
        if (IsLocked || popup == null)
            return;

        TreeSelection = null;
        ActiveRow = row;
        IsTreeOpen = true;
        await popup.ToggleAsync(row.Trigger);
    }

    protected async Task OnTreeSelect()
    {
        if (!IsTreeOpen)
            return;

        // Groups are expand/collapse only; only ledger leaves are selectable.
        if (TreeSelection is AccountTreeNode node && !node.IsGroup && ActiveRow != null)
        {
            ActiveRow.AccountCode = node.Code;
            ActiveRow.PartyId = null;
            await CloseTree();
        }
    }

    protected void OnTreeClose()
    {
        IsTreeOpen = false;
        ActiveRow = null;
    }

    private async Task CloseTree()
    {
        var trigger = ActiveRow?.Trigger ?? default;
        IsTreeOpen = false;
        ActiveRow = null;
        if (popup != null)
            await popup.CloseAsync(trigger);
    }

    protected string? LedgerDisplay(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return LedgerLookup.TryGetValue(code, out var account)
            ? $"{account.Code} - {account.Name}"
            : code;
    }

    protected ChartOfAccountDTO? AccountOf(ObRow row)
    {
        if (string.IsNullOrWhiteSpace(row.AccountCode))
            return null;

        return LedgerLookup.TryGetValue(row.AccountCode, out var account) ? account : null;
    }

    protected bool RowRequiresParty(ObRow row) => AccountOf(row)?.RequiresParty == true;

    protected IEnumerable<AccountingPartyDTO> GetPartyOptions(ObRow row)
    {
        var account = AccountOf(row);
        if (account == null || !account.RequiresParty)
            return Enumerable.Empty<AccountingPartyDTO>();

        var types = account.PartyTypes ?? new List<string>();
        return types.Count == 0 ? Parties : Parties.Where(p => types.Contains(p.PartyType));
    }

    private string? ValidateRows()
    {
        var active = Rows.Where(r => !string.IsNullOrWhiteSpace(r.AccountCode)).ToList();
        if (active.Count == 0)
            return "Enter at least one opening-balance line.";

        foreach (var row in active)
        {
            var debit = row.Debit ?? 0;
            var credit = row.Credit ?? 0;

            if (debit < 0 || credit < 0)
                return "Amounts cannot be negative.";
            if (debit > 0 && credit > 0)
                return $"Line '{row.AccountCode}' cannot have both a debit and a credit.";
            if (debit <= 0 && credit <= 0)
                return $"Enter a debit or credit for '{row.AccountCode}'.";

            var account = AccountOf(row);
            if (account != null && account.RequiresParty && (row.PartyId == null || row.PartyId <= 0))
                return $"Select a party for '{account.Code} - {account.Name}'.";
        }

        return null;
    }

    protected async Task Save()
    {
        if (IsLocked)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Locked", LockReason ?? "Opening balance is locked.");
            return;
        }

        var error = ValidateRows();
        if (error != null)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", error);
            return;
        }

        // Posted activity no longer blocks editing; confirm the retroactive change.
        if (Existing?.HasActivity == true)
        {
            var confirmed = await dialogService.Confirm(
                Existing.WarningMessage ?? "This fiscal year already has posted activity. Saving will change posted balances retroactively.",
                "Confirm Opening Balance Change",
                new ConfirmOptions { OkButtonText = "Save anyway", CancelButtonText = "Cancel" });

            if (confirmed != true)
                return;
        }

        var request = new OpeningBalanceRequestDTO
        {
            CompanyCode = CompanyCode,
            BranchCode = SelectedBranchCode!,
            FiscalYearId = SelectedFiscalYearId,
            EntryDate = AsDateOnly(EntryDate),
            Narration = Narration,
            CreatedBy = CurrentUserId,
            Lines = Rows
                .Where(r => !string.IsNullOrWhiteSpace(r.AccountCode))
                .Select(r => new OpeningBalanceLineDTO
                {
                    AccountCode = r.AccountCode!.Trim(),
                    PartyId = r.PartyId,
                    Debit = r.Debit ?? 0,
                    Credit = r.Credit ?? 0,
                    Memo = r.Memo
                })
                .ToList()
        };

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.OpeningBalanceService.PostOpeningBalance(request);

            notificationService.Notify(
                result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                result.IsSuccessStatus ? "Success" : "Error",
                result.Message);

            if (result.IsSuccessStatus)
                await LoadOpeningBalance();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to post opening balance: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    /// <summary>Strips any time/time-zone component so the entry date serialises as a
    /// plain calendar date (no offset). The API stores it via CAST(@EntryDate AS DATE)
    /// and resolves the fiscal period from that date, so a shifted value could land in
    /// the wrong (or a closed) period.</summary>
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
}
