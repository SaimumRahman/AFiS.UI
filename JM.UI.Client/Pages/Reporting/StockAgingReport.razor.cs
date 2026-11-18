using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class StockAgingReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public StockAgingReportPdfService PdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<StoreDTO> Stores { get; set; } = new();
    protected List<GroupModelDTO> Groups { get; set; } = new();
    protected List<ItemBrandDTO> Brands { get; set; } = new();

    protected int? SelectedGroupId { get; set; }
    protected int? SelectedBrandId { get; set; }
    protected string SelectedAgingBucket { get; set; } = "ALL";
    protected int? SelectedSourceStoreId { get; set; }
    protected string SearchText { get; set; } = string.Empty;

    protected bool CanSelectAnyStore { get; set; }
    protected int CurrentUserId { get; set; }
    protected int UserStoreId { get; set; }
    protected string CurrentUserName { get; set; } = "-";

    protected bool IncludeHealthy { get; set; } = true;
    protected StockAgingReportDTO Report { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }
    protected bool HasGenerated { get; set; }

    protected string ActiveMode { get; set; } = "all";

    protected HashSet<int> SelectedIds { get; set; } = new();

    protected string SelectedStoreName => Stores.FirstOrDefault(s => s.Id == SelectedSourceStoreId)?.Name ?? "-";

    protected IEnumerable<string> AgingBuckets => new[] { "ALL", "0-30", "31-60", "61-90", "91-180", "180+" };

    /// <summary>
    /// Quantity text for a row. Items whose measurement unit allows fractions
    /// (MTR / YDS …) are shown with two decimals exactly as stored; every other
    /// unit is shown as a whole number.
    /// </summary>
    protected static string QtyText(decimal qty, string? uomName)
        => qty.ToString(
            JM.UI.Entities.Model.MesurementUnits.UomRules.AllowsDecimalQuantity(uomName) ? "N2" : "N0");

    /// <summary>Unit label exactly as saved on the item (falls back to "pcs").</summary>
    protected static string UomLabel(string? uomName)
        => string.IsNullOrWhiteSpace(uomName) ? "pcs" : uomName.Trim();

    /// <summary>
    /// Total text for a footer cell that mixes measurement units, so it cannot use
    /// a single unit's decimals: up to three decimals, trailing zeros trimmed.
    /// </summary>
    protected static string SumText(decimal qty) => qty.ToString("#,##0.###");

    protected IEnumerable<StockAgingRowDTO> FilteredRows =>
        Report.Rows.Where(r => ActiveMode == "all" ||
            (ActiveMode == "transfer" && r.Category == "Transfer") ||
            (ActiveMode == "dead" && r.Category == "Dead") ||
            (ActiveMode == "orphan" && r.Category == "Orphan"));

    /// <summary>
    /// True when the report is scoped to one source branch. In that case only that
    /// branch's column is shown and the row totals come from that branch alone.
    /// </summary>
    protected bool IsBranchScoped => SelectedSourceStoreId.HasValue && SelectedSourceStoreId.Value > 0;

    /// <summary>Store columns to render: all branches, or just the selected source branch.</summary>
    protected List<StockAgingStoreDTO> DisplayStores =>
        IsBranchScoped
            ? Report.Stores.Where(s => s.StoreId == SelectedSourceStoreId!.Value).ToList()
            : Report.Stores;

    /// <summary>Quantity shown in the row total column (branch-scoped when a source branch is picked).</summary>
    protected decimal DisplayTotalQty(StockAgingRowDTO row) =>
        IsBranchScoped
            ? row.Cells.FirstOrDefault(c => c.StoreId == SelectedSourceStoreId!.Value)?.Qty ?? 0
            : row.TotalQty;

    /// <summary>Cost of <see cref="DisplayTotalQty"/>.</summary>
    protected decimal DisplayTotalCost(StockAgingRowDTO row) => DisplayTotalQty(row) * row.Cost;

    protected decimal FilteredTotalQty => FilteredRows.Sum(r => DisplayTotalQty(r));

    protected int CountTransfer => Report.Rows.Count(r => r.Category == "Transfer");
    protected int CountDead => Report.Rows.Count(r => r.Category == "Dead");
    protected int CountOrphan => Report.Rows.Count(r => r.Category == "Orphan");
    protected int CountAll => Report.Rows.Count;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        UserStoreId = await GetLocalStorageInt("StoreId");
        CurrentUserName = await GetLocalStorageString("UserName") ?? "-";

        CanSelectAnyStore = CurrentUserId == 1;
        if (!CanSelectAnyStore)
        {
            try
            {
                CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                                        .GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch { CanSelectAnyStore = false; }
        }

        await LoadStores();
        await Task.WhenAll(LoadGroups(), LoadBrands());

        if (!CanSelectAnyStore)
            SelectedSourceStoreId = UserStoreId > 0 ? UserStoreId : Stores.FirstOrDefault()?.Id;
    }

    private async Task<int> GetLocalStorageInt(string key)
    {
        try { var r = await _localStorage.GetAsync<string>(key); if (r.Success && int.TryParse(r.Value, out var v)) return v; }
        catch { }
        return 0;
    }

    private async Task<string?> GetLocalStorageString(string key)
    {
        try { var r = await _localStorage.GetAsync<string>(key); if (r.Success) return r.Value; }
        catch { }
        return null;
    }

    protected async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
            Stores = CanSelectAnyStore ? stores.ToList() : stores.Where(s => s.Id == UserStoreId).ToList();
            SelectedSourceStoreId = Stores.FirstOrDefault()?.Id;
        }
        catch { Stores = new List<StoreDTO>(); }
    }

    protected async Task LoadGroups()
    {
        try { Groups = (await _serviceUnitOfWork.GroupService.GetGroups())?.ToList() ?? new(); }
        catch { Groups = new(); }
    }

    protected async Task LoadBrands()
    {
        try { Brands = (await _serviceUnitOfWork.ItemBrandService.GetItemBrands())?.ToList() ?? new(); }
        catch { Brands = new(); }
    }

    protected async Task Generate()
    {
        IsLoading = true;
        StateHasChanged();
        try
        {
            Report = await _serviceUnitOfWork.StockService.GetStockAging(
                SelectedGroupId, SelectedBrandId, SelectedAgingBucket, SelectedSourceStoreId, SearchText, IncludeHealthy) ?? new StockAgingReportDTO();
            HasGenerated = true;
            SelectedIds.Clear();
            ActiveMode = "all";
        }
        catch (Exception ex)
        {
            Report = new StockAgingReportDTO();
            notificationService.Notify(NotificationSeverity.Error, "Failed", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void ClearFilters()
    {
        SelectedGroupId = null;
        SelectedBrandId = null;
        SelectedAgingBucket = "ALL";
        SearchText = string.Empty;
        ActiveMode = "all";
        SelectedIds.Clear();
    }

    protected void SetMode(string mode) => ActiveMode = mode;

    protected void ToggleSelected(int id)
    {
        if (SelectedIds.Contains(id)) SelectedIds.Remove(id);
        else SelectedIds.Add(id);
    }

    protected void ToggleAll(bool value)
    {
        foreach (var r in FilteredRows)
        {
            if (value) SelectedIds.Add(r.ItemId);
            else SelectedIds.Remove(r.ItemId);
        }
    }

    protected void GenerateTransferDraft()
    {
        if (SelectedIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Selection", "Select at least one SKU row first.");
            return;
        }
        notificationService.Notify(NotificationSeverity.Success, "Draft Created",
            $"Created draft Inter-Branch Transfer for {SelectedIds.Count} selected SKU(s) awaiting HO approval.");
    }

    protected static string Money(decimal value) => value.ToString("N0");
    protected static string Money2(decimal value) => value.ToString("N2");

    protected async Task PrintReport()
    {
        if (!Report.Rows.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "Generate the report first.");
            return;
        }
        try
        {
            IsPrinting = true;
            StateHasChanged();
            var pdfBytes = PdfService.Generate(Report, SelectedStoreName, CurrentUserName, SelectedSourceStoreId);
            var fileName = $"StockAging_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            notificationService.Notify(NotificationSeverity.Success, "Success", "Report generated.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed", $"Could not generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }
}