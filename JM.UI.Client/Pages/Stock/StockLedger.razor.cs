using JM.UI.Client.Pages.Dialog.Stock;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.Stock;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Stock;

public partial class StockLedgerComponent : PosComponentBase, IDisposable
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<StockLedgerPivotRow> StockLedgerGrid = default!;
    protected IEnumerable<StockLedgerDTO> StockLedgerList = new List<StockLedgerDTO>();
    protected IEnumerable<StoreDTO> StoreList = new List<StoreDTO>();
    protected bool IsLoading;

    /// <summary>Set when the branch bands change, so the grid re-registers its columns.</summary>
    protected bool GridNeedsReload;

    /// <summary>
    /// Identity of the current set of branch bands. Bound to the grid with @key so
    /// the grid is re-created - with all banded columns in order - whenever the set
    /// of branches changes. Without this, Radzen keeps the columns registered on the
    /// very first (empty, pre-data) render and the Grand Total band ends up next to
    /// Sale Price.
    /// </summary>
    protected string GridKey =>
        string.Join("|", StoreGroups.Select(g => g.StoreName));

    // ── Pivoted table state ──
    // The grid is a branch-wise pivot: one row per item, one banded four-column
    // block per store, plus a grand-total block. Built from the same flat ledger
    // rows the page always fetched, so the store permission rules are untouched.
    protected List<StockLedgerStoreGroup> StoreGroups { get; set; } = new();
    protected List<StockLedgerPivotRow> PivotRows { get; set; } = new();

    protected int PivotItemCount => PivotRows.Count;

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected int? SelectedStoreId { get; set; }
    protected int CurrentUserId { get; set; } = 0;

    // ── Item attribute filters ──
    // All optional: a null selection means "do not filter", which keeps the
    // unfiltered result identical to the original ledger.
    protected List<GroupModelDTO> Groups { get; set; } = new();
    protected List<SubGroupModelDTO> AllSubGroups { get; set; } = new();
    protected List<DesignModelDTO> AllDesigns { get; set; } = new();
    protected List<ItemBrandDTO> Brands { get; set; } = new();
    protected IEnumerable<SubGroupModelDTO> FilteredSubGroups { get; set; } = new List<SubGroupModelDTO>();
    protected IEnumerable<DesignModelDTO> FilteredDesigns { get; set; } = new List<DesignModelDTO>();

    protected int? SelectedGroupId { get; set; }
    protected int? SelectedSubGroupId { get; set; }
    protected int? SelectedDesignId { get; set; }
    protected int? SelectedBrandId { get; set; }

    /// <summary>Free-text filter: partial barcode OR partial item name.</summary>
    protected string? SearchText { get; set; }

    // Any user whose group is flagged IsSystem (GroupsRole.IsSystem = 1) - plus the
    // built-in super admin - may pick any store here. Everyone else is pinned to the
    // store they are signed in to.
    protected bool CanSelectAnyStore { get; set; }
    protected bool IsStoreDropdownDisabled => !CanSelectAnyStore;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");

        // Must be resolved before LoadStores(), which branches on it.
        await ResolveStoreSelectionPermission();

        await LoadStores();
        await LoadFilterLookups();
        await LoadStockLedger();
    }

    private async Task ResolveStoreSelectionPermission()
    {
        CanSelectAnyStore = CurrentUserId == 1;
        if (CanSelectAnyStore) return;

        try
        {
            CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                                    .GetSystemGroupCountByUserId(CurrentUserId) > 0;
        }
        catch (Exception ex)
        {
            // Fail closed: a failed lookup must not hand out cross-store visibility.
            CanSelectAnyStore = false;
            Console.WriteLine($"[DEBUG] ResolveStoreSelectionPermission failed: {ex.Message}");
        }
    }

    // Each lookup is loaded independently, so a failure leaves an empty dropdown
    // instead of breaking the whole page.
    private async Task LoadFilterLookups()
    {
        try { Groups = (await _serviceUnitOfWork.GroupService.GetGroups()).ToList(); }
        catch (Exception ex) { Console.WriteLine($"[DEBUG] Load Groups failed: {ex.Message}"); }

        try { AllSubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups()).ToList(); }
        catch (Exception ex) { Console.WriteLine($"[DEBUG] Load SubGroups failed: {ex.Message}"); }

        try { AllDesigns = (await _serviceUnitOfWork.DesignService.GetDesigns()).ToList(); }
        catch (Exception ex) { Console.WriteLine($"[DEBUG] Load Designs failed: {ex.Message}"); }

        try { Brands = (await _serviceUnitOfWork.ItemBrandService.GetItemBrands()).ToList(); }
        catch (Exception ex) { Console.WriteLine($"[DEBUG] Load Brands failed: {ex.Message}"); }

        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;
    }

    // ── Cascading filter handlers ──
    // These only refresh the child dropdown lists. The grid re-queries when the
    // Search button is pressed.

    protected void OnGroupChanged(int? groupId)
    {
        SelectedGroupId = groupId;
        SelectedSubGroupId = null;
        SelectedDesignId = null;

        FilteredSubGroups = groupId.HasValue
            ? AllSubGroups.Where(sg => sg.GroupId == groupId.Value).ToList()
            : AllSubGroups;

        FilteredDesigns = groupId.HasValue
            ? Enumerable.Empty<DesignModelDTO>()
            : AllDesigns;
    }

    protected void OnSubGroupChanged(int? subGroupId)
    {
        SelectedSubGroupId = subGroupId;
        SelectedDesignId = null;

        FilteredDesigns = subGroupId.HasValue
            ? AllDesigns.Where(d => d.SubGroupId == subGroupId.Value).ToList()
            : AllDesigns;
    }

    protected void OnDesignChanged(int? designId) => SelectedDesignId = designId;

    protected void OnBrandChanged(int? brandId) => SelectedBrandId = brandId;

    // Resets the item attribute and date filters. The store selection is deliberately
    // left alone: clearing it would widen the query to every store, which a user
    // without the right must never trigger.
    protected void ClearFilters()
    {
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedBrandId = null;
        SearchText = null;

        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;

        FromDate = null;
        ToDate = null;
    }

    private async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores();

            if (CanSelectAnyStore)
            {
                StoreList = stores;
            }
            else
            {
                var currentStoreId = await GetLocalStorageInt("StoreId");
                StoreList = stores.Where(s => s.Id == currentStoreId).ToList();
                SelectedStoreId = StoreList.FirstOrDefault()?.Id;
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stores: {ex.Message}");
        }
    }

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(key);
            if (result.Success && int.TryParse(result.Value, out int value))
                return value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
        }

        return 0;
    }

    protected async Task LoadStockLedger()
    {
        try
        {
            IsLoading = true;
            StockLedgerList = await _serviceUnitOfWork.StockService
                .GetStockLedger(FromDate, ToDate, null, SelectedStoreId,
                                SelectedGroupId, SelectedSubGroupId,
                                SelectedDesignId, SelectedBrandId, SearchText);

            BuildPivot();
        }
        catch (Exception ex)
        {
            StockLedgerList = new List<StockLedgerDTO>();
            PivotRows = new List<StockLedgerPivotRow>();
            StoreGroups = new List<StockLedgerStoreGroup>();
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stock ledger: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            // Branch bands are generated from StoreGroups, which is only known after
            // the query returns. Flag the grid so it re-registers its columns on the
            // next render - otherwise the band columns never appear and the Grand
            // Total looks like it sits right after Sale Price.
            GridNeedsReload = true;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!GridNeedsReload) return;
        GridNeedsReload = false;
        if (StockLedgerGrid != null)
        {
            await StockLedgerGrid.Reload();
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Pivot
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Folds the flat (store × item) ledger rows into the branch-wise table:
    /// one row per item, four measures per store, and a grand-total block.
    /// </summary>
    private void BuildPivot()
    {
        var rows = StockLedgerList?.ToList() ?? new List<StockLedgerDTO>();

        // Columns follow the store master order (so branches keep a familiar
        // sequence); a store missing from the master falls back to alphabetical.
        var preferredOrder = StoreList
            .Select((store, index) => new { Name = (store.Name ?? string.Empty).Trim(), Index = index })
            .Where(x => x.Name.Length > 0)
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Index, StringComparer.OrdinalIgnoreCase);

        StoreGroups = rows
            .Select(r => (r.StoreName ?? string.Empty).Trim())
            .Where(name => name.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => preferredOrder.TryGetValue(name, out var index) ? index : int.MaxValue)
            .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(name => new StockLedgerStoreGroup
            {
                StoreName = name,
                StoreId = rows.FirstOrDefault(r =>
                    string.Equals((r.StoreName ?? string.Empty).Trim(), name,
                                  StringComparison.OrdinalIgnoreCase))?.StoreId ?? 0
            })
            .ToList();

        PivotRows = rows
            .GroupBy(ItemKeyOf, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();

                var row = new StockLedgerPivotRow
                {
                    ItemKey = group.Key,
                    Barcode = string.IsNullOrWhiteSpace(first.ItemCode) ? "-" : first.ItemCode.Trim(),
                    ItemName = first.ItemName ?? string.Empty,
                    SalePrice = group.Max(r => r.SalePrice),
                    GroupName = FirstValue(group, r => r.GroupName),
                    SubGroupName = FirstValue(group, r => r.SubGroupName),
                    DesignName = FirstValue(group, r => r.DesignName),
                    BrandName = FirstValue(group, r => r.BrandName),
                    ImageBase64 = first.ImageBase64,
                    OldBarcode = first.OldBarcode,
                    Features = first.Features
                };

                // Active campaigns are resolved per store row by the API; an item
                // can carry the same campaign in several branches, so dedupe on
                // campaign id + store before showing them.
                row.Campaigns = group
                    .Where(r => r.CampaignId.HasValue && r.CampaignId.Value > 0)
                    .GroupBy(r => new
                    {
                        Id = r.CampaignId!.Value,
                        Store = r.CampaignStoreId ?? 0
                    })
                    .Select(g =>
                    {
                        var c = g.First();
                        return new StockLedgerCampaign
                        {
                            CampaignId = g.Key.Id,
                            Name = c.CampaignName ?? string.Empty,
                            DiscountValue = c.CampaignDiscountValue ?? 0m,
                            DiscountTypeId = c.CampaignDiscountTypeId ?? 1,
                            StartDate = c.CampaignStartDate ?? DateTime.MinValue,
                            EndDate = c.CampaignEndDate ?? DateTime.MinValue,
                            StoreId = c.CampaignStoreId
                        };
                    })
                    .OrderBy(c => c.IsBranchSpecific)
                    .ThenBy(c => c.EndDate)
                    .ToList();

                foreach (var storeRows in group.GroupBy(
                             r => (r.StoreName ?? string.Empty).Trim(),
                             StringComparer.OrdinalIgnoreCase))
                {
                    row.Cells[storeRows.Key] = new StockLedgerStoreCell
                    {
                        OpeningStock = storeRows.Sum(r => r.OpeningStock),
                        ReceiveQty = storeRows.Sum(r => r.ReceiveQty),
                        IssueQty = storeRows.Sum(r => r.IssueQty),
                        ClosingStock = storeRows.Sum(r => r.ClosingStock)
                    };
                }

                row.TotalOpening = row.Cells.Values.Sum(c => c.OpeningStock);
                row.TotalReceive = row.Cells.Values.Sum(c => c.ReceiveQty);
                row.TotalIssue = row.Cells.Values.Sum(c => c.IssueQty);
                row.TotalClosing = row.Cells.Values.Sum(c => c.ClosingStock);

                return row;
            })
            .OrderBy(r => r.ItemName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Barcode, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ItemKeyOf(StockLedgerDTO row)
        => string.IsNullOrWhiteSpace(row.ItemCode)
            ? "#" + (row.ItemName ?? string.Empty).Trim()
            : row.ItemCode.Trim();

    private static string FirstValue(IEnumerable<StockLedgerDTO> rows, Func<StockLedgerDTO, string> selector)
        => rows.Select(selector)
               .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? string.Empty;

    // ──────────────────────────────────────────────────────────────────
    //  Product details dialog
    // ──────────────────────────────────────────────────────────────────

    protected async Task OpenItemDetails(StockLedgerPivotRow row)
    {
        await dialogService.OpenAsync<StockLedgerItemDialog>(
            "Product Information",
            new Dictionary<string, object>
            {
                { nameof(StockLedgerItemDialog.Item), row },
                { nameof(StockLedgerItemDialog.Stores), StoreGroups }
            },
            new DialogOptions
            {
                Width = "760px",
                Resizable = true,
                Draggable = true,
                ShowClose = true
            });
    }

    // ── Display helpers ──────────────────────────────────────────────

    protected static string Qty(decimal value) => value.ToString("N2");
    protected static string Money(decimal value) => value.ToString("N2");

    protected static string BalanceStyle(decimal value)
        => value < 0 ? "color: var(--rz-danger); font-weight:700;" : "font-weight:700;";

    public void Dispose()
    {
        StockLedgerGrid?.Dispose();
    }
}
