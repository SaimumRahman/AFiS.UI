using JM.UI.Client.Pages.Dialog.Discount;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Discount;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Discount;

public partial class DiscountManagerAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected DiscountManagerDTO Campaign { get; set; } = new();
    protected List<ItemDTO> AllItems { get; set; } = new();
    protected List<DiscountTypeDTO> DiscountTypes { get; set; } = new();
    protected List<StoreDTO> Stores { get; set; } = new();

    // Stores the campaign discount is valid for (mandatory - one or more stores)
    protected List<int> SelectedStoreIds { get; set; } = new();
    protected bool AllStoresSelected
    {
        get => _allStoresSelected;
        set
        {
            _allStoresSelected = value;
            if (value)
                SelectedStoreIds = Stores.Select(s => s.Id).ToList();
            else
                SelectedStoreIds = new();
            UpdateCampaignDetails();
        }
    }
    private bool _allStoresSelected = false;

    // Grid display items
    protected List<DiscountGridItem> GridItems { get; set; } = new();

    // The campaign's detail rows as last loaded/saved. Selections in the grid are
    // merged against this baseline, so products that are filtered out of the grid
    // (or that are not loaded yet) are never silently dropped from the campaign.
    private List<DiscountManagerDetailsDTO> _baselineDetails = new();

    /// <summary>Saved products that the current filter hides but that are still kept.</summary>
    protected int HiddenSavedItemCount { get; private set; }

    // Conflict keys ("itemId:storeId:campaignId") the user already accepted in the
    // "already in another campaign" dialog. They are what stops the dialog from being
    // asked twice (once when adding the item, again on Save).
    private readonly HashSet<string> _acknowledgedConflicts = new();

    private static string ConflictKey(CampaignConflictDTO c)
        => $"{c.ItemId}:{c.StoreId ?? 0}:{c.DiscountManagerId}";

    private static List<string> CampaignsFor(List<CampaignConflictDTO> conflicts, int itemId)
        => conflicts.Where(c => c.ItemId == itemId)
                    .Select(c => c.DiscountName)
                    .Distinct()
                    .ToList();


    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Discount Campaign" : "New Discount Campaign";
    protected string PageIcon => IsEditMode ? "edit" : "discount";

    // Bulk discount
    protected decimal BulkDiscountValue { get; set; }
    protected int BulkDiscountTypeId { get; set; } = 1;

    // Search / scan
    protected string SearchText { get; set; } = "";
    protected bool Scanning { get; set; }

    protected bool AllSelected => GridItems.Count > 0 && GridItems.All(x => x.Selected);
    protected int SelectedCount => GridItems.Count(x => x.Selected);
    protected bool ShowGlobalDiscount => AllSelected;

    // Campaign selection dropdown
    protected IEnumerable<DiscountManagerDTO> Campaigns { get; set; } = new List<DiscountManagerDTO>();
    protected int? SelectedCampaignId { get; set; }

    // Filter dropdowns - full lists
    protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
    protected IEnumerable<SubGroupModelDTO> AllSubGroups { get; set; } = new List<SubGroupModelDTO>();
    protected IEnumerable<DesignModelDTO> AllDesigns { get; set; } = new List<DesignModelDTO>();
    protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
    protected IEnumerable<SizesDTO> Sizes { get; set; } = new List<SizesDTO>();
    protected IEnumerable<ItemCatalogueDTO> Catalogues { get; set; } = new List<ItemCatalogueDTO>();

    // Filter selected values
    protected int? SelectedGroupId { get; set; }
    protected int? SelectedSubGroupId { get; set; }
    protected int? SelectedDesignId { get; set; }
    protected int? SelectedColorId { get; set; }
    protected int? SelectedSizeId { get; set; }
    protected int? SelectedCatalogueId { get; set; }

    // Cascading filtered dropdowns
    protected IEnumerable<SubGroupModelDTO> FilteredSubGroups { get; set; } = new List<SubGroupModelDTO>();
    protected IEnumerable<DesignModelDTO> FilteredDesigns { get; set; } = new List<DesignModelDTO>();

    protected override async Task OnInitializedAsync()
    {
        NavigationGuard.IsGuardActive = true;
        await TokenService.InitializeTokenAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            IsLoading = true;

            DiscountTypes = (await _serviceUnitOfWork.DiscountManagerService.GetDiscountTypes()).ToList();

        AllItems = new List<ItemDTO>();

        // Load filter lookup data
            Campaigns = (await _serviceUnitOfWork.DiscountManagerService.GetAll()).ToList();
            Groups = (await _serviceUnitOfWork.GroupService.GetGroups()).ToList();
            AllSubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups()).ToList();
            AllDesigns = (await _serviceUnitOfWork.DesignService.GetDesigns()).ToList();
            Colors = (await _serviceUnitOfWork.ColorsService.GetColorss()).ToList();
            Sizes = (await _serviceUnitOfWork.SizesService.GetSizess()).ToList();
            Catalogues = (await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues()).ToList();
            Stores = (await _serviceUnitOfWork.StoreService.GetStores()).ToList();

            if (IsEditMode)
            {
                Campaign = await _serviceUnitOfWork.DiscountManagerService.GetById(Id!.Value);
                if (Campaign == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Campaign not found.");
                    NavigationManager.NavigateTo("/DiscountManagerList");
                    return;
                }
            }
            else
            {
                Campaign = _serviceUnitOfWork.DiscountManagerService.CreateNew();
            }

            // Baseline = what is currently saved, so grid edits merge instead of replace.
            _baselineDetails = (Campaign.DiscountDetails ?? new List<DiscountManagerDetailsDTO>()).ToList();

            // Resolve the stores this campaign is scoped to (details may span multiple stores)
            if (Campaign.StoreIds != null && Campaign.StoreIds.Count > 0)
                SelectedStoreIds = Campaign.StoreIds.ToList();
            else
                SelectedStoreIds = (Campaign.DiscountDetails ?? new List<DiscountManagerDetailsDTO>())
                    .Where(d => d.StoreId.HasValue && d.StoreId.Value > 0)
                    .Select(d => d.StoreId!.Value)
                    .Distinct()
                    .ToList();
            if (!SelectedStoreIds.Any() && Campaign.StoreId.HasValue && Campaign.StoreId.Value > 0)
                SelectedStoreIds.Add(Campaign.StoreId.Value);
            _allStoresSelected = Stores.Count > 0 && SelectedStoreIds.Count == Stores.Count;

            // An existing campaign must show its products straight away, without the
            // user having to pick a filter first.
            await BuildGridItemsAsync(forceLoadItems: IsEditMode);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load data: {ex.Message}");
            if (IsEditMode)
                NavigationManager.NavigateTo("/DiscountManagerList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool AnyFilterActive() =>
        SelectedGroupId.HasValue || SelectedSubGroupId.HasValue || SelectedDesignId.HasValue ||
        SelectedColorId.HasValue || SelectedSizeId.HasValue || SelectedCatalogueId.HasValue;

    private async Task EnsureItemsLoadedAsync(bool force = false)
    {
        // Items are loaded on demand. Normally that means "when a filter is chosen",
        // but an existing campaign must show its saved products without the user
        // having to pick a filter first (= force).
        if (AllItems.Any()) return;
        if (!force && !AnyFilterActive()) return;

        var items = (await _serviceUnitOfWork.ItemService.GetItems()).ToList();

        // GetAllItems returns one row per (item, store) because of its StockMaster join.
        // A campaign is scoped to a single store, so show each item from the Items table
        // once, keyed on its unique Barcode (falling back to Id when the barcode is blank).
        AllItems = items
            .GroupBy(i => !string.IsNullOrWhiteSpace(i.Barcode) ? i.Barcode! : "#" + i.Id,
                     StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private async Task BuildGridItemsAsync(bool forceLoadItems = false)
    {
        await EnsureItemsLoadedAsync(forceLoadItems);
        var filtered = FilterItems();
        GridItems = new List<DiscountGridItem>();

        foreach (var item in filtered)
        {
            var detail = _baselineDetails.FirstOrDefault(d => d.ItemId == item.Id);
            GridItems.Add(new DiscountGridItem
            {
                Item = item,
                Selected = detail != null || !IsEditMode,
                DiscountValue = detail?.DiscountValue ?? 0,
                DiscountTypeId = detail?.DiscountTypeId ?? 1,
                DiscountTypeName = detail?.DiscountTypeName ?? DiscountTypes.FirstOrDefault()?.TypeName ?? "Percentage",
                Source = detail != null ? "loaded" : (!IsEditMode ? "all" : ""),
                CurrentSalePrice = item.SalePrice ?? 0
            });
        }

        // Saved products that the active filter hides — they stay in the campaign.
        var visibleItemIds = GridItems.Select(g => g.Item.Id).ToHashSet();
        HiddenSavedItemCount = _baselineDetails
            .Where(d => !visibleItemIds.Contains(d.ItemId))
            .Select(d => d.ItemId)
            .Distinct()
            .Count();
    }

    private List<ItemDTO> FilterItems()
    {
        var query = AllItems.AsEnumerable();

        if (SelectedGroupId.HasValue)
            query = query.Where(i => i.GroupId == SelectedGroupId.Value);

        if (SelectedSubGroupId.HasValue)
            query = query.Where(i => i.SubGroupId == SelectedSubGroupId.Value);

        if (SelectedDesignId.HasValue)
            query = query.Where(i => i.DesignId == SelectedDesignId.Value);

        if (SelectedColorId.HasValue)
            query = query.Where(i => i.ColorId == SelectedColorId.Value);

        if (SelectedSizeId.HasValue)
            query = query.Where(i => i.SizeId == SelectedSizeId.Value);

        if (SelectedCatalogueId.HasValue)
            query = query.Where(i => i.CatalogueId == SelectedCatalogueId.Value);

        return query.ToList();
    }

    protected async Task OnCampaignSelected(int? campaignId)
    {
        if (!campaignId.HasValue) return;

        var campaign = await _serviceUnitOfWork.DiscountManagerService.GetById(campaignId.Value);
        if (campaign == null) return;

        Campaign = campaign;
        SelectedCampaignId = campaignId;
        Id = campaign.Id;

        // Baseline = what is saved now, so the grid edits merge instead of replace.
        _baselineDetails = (campaign.DiscountDetails ?? new List<DiscountManagerDetailsDTO>()).ToList();

        // Sync the campaign's stores (details may span multiple stores)
        if (campaign.StoreIds != null && campaign.StoreIds.Count > 0)
            SelectedStoreIds = campaign.StoreIds.ToList();
        else
            SelectedStoreIds = (campaign.DiscountDetails ?? new List<DiscountManagerDetailsDTO>())
                .Where(d => d.StoreId.HasValue && d.StoreId.Value > 0)
                .Select(d => d.StoreId!.Value)
                .Distinct()
                .ToList();
        if (!SelectedStoreIds.Any() && campaign.StoreId.HasValue && campaign.StoreId.Value > 0)
            SelectedStoreIds.Add(campaign.StoreId.Value);
        // Assign the backing field: the property's setter calls UpdateCampaignDetails(),
        // which at this point still sees the previously loaded campaign's GridItems and
        // would merge those stale selections into this campaign.
        _allStoresSelected = Stores.Count > 0 && SelectedStoreIds.Count == Stores.Count;

        // Reset filters
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedColorId = null;
        SelectedSizeId = null;
        SelectedCatalogueId = null;
        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;
        BulkDiscountValue = 0;
        SearchText = string.Empty;
        _acknowledgedConflicts.Clear();

        await BuildGridItemsAsync(forceLoadItems: true);
        StateHasChanged();
    }

    protected void OnGroupCascadeChanged(int? groupId)
    {
        SelectedSubGroupId = null;
        SelectedDesignId = null;

        if (SelectedGroupId.HasValue)
            FilteredSubGroups = AllSubGroups.Where(sg => sg.GroupId == SelectedGroupId.Value).ToList();
        else
            FilteredSubGroups = AllSubGroups;

        FilteredDesigns = AllDesigns;
        StateHasChanged();
    }

    protected void OnSubGroupCascadeChanged(int? subGroupId)
    {
        SelectedDesignId = null;

        if (SelectedSubGroupId.HasValue)
            FilteredDesigns = AllDesigns.Where(d => d.SubGroupId == SelectedSubGroupId.Value).ToList();
        else
            FilteredDesigns = AllDesigns;
        StateHasChanged();
    }

    protected async Task LoadFilteredItems()
    {
        await BuildGridItemsAsync();
    }

    /// <summary>
    /// Row checkbox. The item is only selected once the exclusivity check has passed;
    /// on "No" the selection is reverted and nothing changes.
    /// </summary>
    protected async Task OnItemCheckedChanged(DiscountGridItem gridItem, bool isChecked)
    {
        if (!isChecked)
        {
            gridItem.Selected = false;
            gridItem.DiscountValue = 0;
            gridItem.Source = "";
            gridItem.MovedFromCampaigns.Clear();
            ForgetAcknowledgements(gridItem.Item.Id);
            UpdateCampaignDetails();
            return;
        }

        var (allowed, conflicts) = await ConfirmConflictsAsync(new[] { gridItem.Item.Id });
        if (!allowed)
        {
            gridItem.Selected = false;
            StateHasChanged();
            return;
        }

        gridItem.Selected = true;
        if (string.IsNullOrEmpty(gridItem.Source))
            gridItem.Source = "manual";
        gridItem.MovedFromCampaigns = CampaignsFor(conflicts, gridItem.Item.Id);
        UpdateCampaignDetails();
    }

    /// <summary>
    /// "Select all" header checkbox. Every product is checked first: on "No" the whole
    /// bulk action is reverted, so a single conflict can never half-apply a selection.
    /// </summary>
    protected async Task ToggleAllAsync(bool select)
    {
        if (select)
        {
            var newly = GridItems.Where(x => !x.Selected).ToList();
            if (newly.Count > 0)
            {
                var (allowed, conflicts) = await ConfirmConflictsAsync(newly.Select(x => x.Item.Id));
                if (!allowed)
                {
                    StateHasChanged();
                    return;
                }

                foreach (var gi in newly)
                    gi.MovedFromCampaigns = CampaignsFor(conflicts, gi.Item.Id);
            }

            foreach (var gi in GridItems)
            {
                gi.Selected = true;
                if (string.IsNullOrEmpty(gi.Source))
                    gi.Source = "all";
            }
        }
        else
        {
            foreach (var gi in GridItems)
            {
                gi.Selected = false;
                gi.DiscountValue = 0;
                gi.Source = "";
                gi.MovedFromCampaigns.Clear();
            }
            _acknowledgedConflicts.Clear();
            BulkDiscountValue = 0;
        }
        UpdateCampaignDetails();
    }

    /// <summary>
    /// Asks the server which of these products already sit in another active campaign
    /// (same store, dates sharing a day). Conflicts the user already accepted are not
    /// asked about again. Returns (true, allConflicts) when it is safe to continue.
    /// </summary>
    private async Task<(bool allowed, List<CampaignConflictDTO> conflicts)> ConfirmConflictsAsync(IEnumerable<int> itemIds)
    {
        var all = new List<CampaignConflictDTO>();

        if (SelectedStoreIds.Count == 0)
            return (true, all);

        if (Campaign.StartDate == default || Campaign.EndDate == default)
            return (true, all);

        var ids = itemIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count == 0)
            return (true, all);

        all = (await _serviceUnitOfWork.DiscountManagerService.GetConflictsAsync(
                    ids, SelectedStoreIds, Campaign.StartDate, Campaign.EndDate, Campaign.Id))
                .ToList();

        if (all.Count == 0)
            return (true, all);

        var pending = all.Where(c => !_acknowledgedConflicts.Contains(ConflictKey(c))).ToList();
        if (pending.Count == 0)
            return (true, all);   // everything was already confirmed on add

        var answer = await dialogService.OpenAsync<CampaignConflictDialog>(
            "Item already in another campaign",
            new Dictionary<string, object>
            {
                { nameof(CampaignConflictDialog.Conflicts), pending }
            },
            new DialogOptions
            {
                Width = "700px",
                Resizable = true,
                Draggable = true,
                ShowClose = false,
                CloseDialogOnEsc = false
            });

        if (answer is bool yes && yes)
        {
            foreach (var c in pending)
                _acknowledgedConflicts.Add(ConflictKey(c));
            return (true, all);
        }

        return (false, all);
    }

    private void ForgetAcknowledgements(int itemId)
    {
        var prefix = itemId + ":";
        _acknowledgedConflicts.RemoveWhere(k => k.StartsWith(prefix, StringComparison.Ordinal));
    }

    /// <summary>
    /// The store set or the dates changed, so every previous answer is stale: clear the
    /// tags and re-ask on Save against the new scope.
    /// </summary>
    private void InvalidateConflictAcknowledgements()
    {
        _acknowledgedConflicts.Clear();
        foreach (var gi in GridItems)
            gi.MovedFromCampaigns.Clear();
    }

    protected void OnDiscountValueChanged(DiscountGridItem gridItem, decimal val)
    {
        gridItem.DiscountValue = val;
        UpdateCampaignDetails();
    }

    protected void OnDiscountTypeChanged(DiscountGridItem gridItem, int typeId)
    {
        var dt = DiscountTypes.FirstOrDefault(t => t.Id == typeId);
        gridItem.DiscountTypeId = typeId;
        gridItem.DiscountTypeName = dt?.TypeName ?? "";
        gridItem.DiscountValue = 0;
        UpdateCampaignDetails();
    }

    protected void ApplyBulkDiscount()
    {
        if (BulkDiscountValue <= 0) return;
        foreach (var gi in GridItems.Where(x => x.Selected))
        {
            gi.DiscountValue = BulkDiscountValue;
            gi.DiscountTypeId = BulkDiscountTypeId;
            var dt = DiscountTypes.FirstOrDefault(t => t.Id == BulkDiscountTypeId);
            gi.DiscountTypeName = dt?.TypeName ?? "";
        }
        UpdateCampaignDetails();
    }

    protected void ApplyGlobalDiscount(decimal val)
    {
        var typeId = DiscountTypes.FirstOrDefault()?.Id ?? 1;
        foreach (var gi in GridItems.Where(x => x.Selected && x.Source == "all"))
        {
            gi.DiscountValue = val;
            gi.DiscountTypeId = typeId;
            gi.DiscountTypeName = DiscountTypes.FirstOrDefault(t => t.Id == typeId)?.TypeName ?? "";
        }
        UpdateCampaignDetails();
    }

    protected void OnGlobalDiscTypeChange(int typeId)
    {
        var dt = DiscountTypes.FirstOrDefault(t => t.Id == typeId);
        foreach (var gi in GridItems.Where(x => x.Selected && x.Source == "all"))
        {
            gi.DiscountTypeId = typeId;
            gi.DiscountTypeName = dt?.TypeName ?? "";
            gi.DiscountValue = 0;
        }
        UpdateCampaignDetails();
    }

    protected async Task OnSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            SearchByBarcode();
        }
    }

    protected async Task SearchByBarcode()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        var term = SearchText.Trim();

        // Fast path: an already-loaded grid row matches the typed barcode exactly.
        var match = GridItems.FirstOrDefault(x =>
            (x.Item.Barcode ?? "").Equals(term, StringComparison.OrdinalIgnoreCase));
        if (match != null)
        {
            if (!match.Selected)
            {
                var (allowed, conflicts) = await ConfirmConflictsAsync(new[] { match.Item.Id });
                if (!allowed) return;

                match.Selected = true;
                match.Source = "scan";
                match.MovedFromCampaigns = CampaignsFor(conflicts, match.Item.Id);
            }

            Scanning = true;
            InvokeAsync(async () =>
            {
                await Task.Delay(800);
                Scanning = false;
                StateHasChanged();
            });
            notificationService.Notify(NotificationSeverity.Success, "Found", $"✓ {match.Item.Name}");
            SearchText = "";
            UpdateCampaignDetails();
            return;
        }

        if (SelectedStoreIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Store required",
                "⚠ Select at least one store before scanning products.");
            return;
        }

        // Exact scan across every store the campaign covers (same matching rules as
        // SalesPOS, but scoped to all selected stores at once).
        var rows = (await _serviceUnitOfWork.DiscountManagerService.SearchCampaignBarcodeAsync(
            term, SelectedStoreIds)).ToList();

        if (rows.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Not found",
                $"⚠ Barcode '{term}' was not found in the selected stores.");
            return;
        }

        // A single hit -> confirm conflicts and add directly, exactly like the POS
        // flow. Multiple hits (same code in several products) -> let the user pick.
        if (rows.Count == 1)
        {
            await AddCampaignProductAsync(rows[0]);
        }
        else
        {
            var picked = await OpenProductSearch(term);
            if (picked != null)
                await AddCampaignProductAsync(picked);
        }

        SearchText = "";
    }

    /// <summary>
    /// Opens the SalesPOS-style product search dialog (prefilled with <paramref name="term"/>).
    /// </summary>
    private async Task<ProductSearchDTO?> OpenProductSearch(string? term)
    {
        return await dialogService.OpenAsync<DiscountProductSearchDialog>("Product Search",
            new Dictionary<string, object>
            {
                { "Term", term ?? "" },
                { "StoreIds", SelectedStoreIds.ToList() }
            },
            new DialogOptions { Width = "1000px", Height = "550px" });
    }

    /// <summary>
    /// Adds (or selects) a product coming from the scan/search flow. Runs the
    /// campaign-exclusivity check first; on "No" nothing changes.
    /// </summary>
    private async Task AddCampaignProductAsync(ProductSearchDTO p)
    {
        var (allowed, conflicts) = await ConfirmConflictsAsync(new[] { p.ItemId });
        if (!allowed)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Not added",
                $"{p.ProductName} was not added because it is already in another campaign.");
            return;
        }

        var existing = GridItems.FirstOrDefault(x => x.Item.Id == p.ItemId);
        if (existing != null)
        {
            if (!existing.Selected)
            {
                existing.Selected = true;
                existing.Source = "scan";
                existing.MovedFromCampaigns = CampaignsFor(conflicts, p.ItemId);
                UpdateCampaignDetails();
            }
            notificationService.Notify(NotificationSeverity.Success, "Found", $"✓ {p.ProductName} (already in list)");
            return;
        }

        var item = new ItemDTO
        {
            Id = p.ItemId,
            Name = p.ProductName ?? p.ItemName ?? "",
            Barcode = p.Barcode,
            GroupName = p.GroupName,
            ProductType = p.ProductType,
            SalePrice = p.UnitPrice,
            CurrentStock = p.StockQuantity,
            MesurementUnitName = p.UomName
        };

        AllItems.Add(item);
        GridItems.Add(new DiscountGridItem
        {
            Item = item,
            Selected = true,
            Source = "scan",
            CurrentSalePrice = p.UnitPrice,
            MovedFromCampaigns = CampaignsFor(conflicts, p.ItemId)
        });

        Scanning = true;
        InvokeAsync(async () =>
        {
            await Task.Delay(800);
            Scanning = false;
            StateHasChanged();
        });
        notificationService.Notify(NotificationSeverity.Success, "Found", $"✓ {item.Name} (from server)");
        UpdateCampaignDetails();
    }

    private void UpdateCampaignDetails()
    {
        var selectedStoreNames = Stores
            .Where(s => SelectedStoreIds.Contains(s.Id))
            .Select(s => s.Name)
            .ToList();
        var storeNameJoined = selectedStoreNames.Count > 0 ? string.Join(", ", selectedStoreNames) : null;

        // Backward-compatible primary store (first selected).
        Campaign.StoreId = SelectedStoreIds.FirstOrDefault();
        Campaign.StoreName = storeNameJoined;
        Campaign.StoreIds = SelectedStoreIds.ToList();

        // One detail row per (item x selected store) so the same discount applies to
        // every selected store. POS already filters by StoreId.
        var details = new List<DiscountManagerDetailsDTO>();

        void AddRows(int itemId, string? itemName, decimal value, int typeId, string? typeName, decimal salePrice)
        {
            foreach (var storeId in SelectedStoreIds)
            {
                details.Add(new DiscountManagerDetailsDTO
                {
                    ItemId = itemId,
                    ItemName = itemName,
                    StoreId = storeId,
                    StoreName = Stores.FirstOrDefault(s => s.Id == storeId)?.Name,
                    DiscountValue = value,
                    DiscountTypeId = typeId,
                    DiscountTypeName = typeName,
                    CurrentSalePrice = salePrice,
                    CreatedDate = Campaign.CreatedDate == default ? DateTime.Now : Campaign.CreatedDate
                });
            }
        }

        // 1. What the user currently has selected in the grid.
        foreach (var gi in GridItems.Where(x => x.Selected && x.DiscountValue > 0))
        {
            AddRows(gi.Item.Id, gi.Item.Name, gi.DiscountValue, gi.DiscountTypeId, gi.DiscountTypeName, gi.CurrentSalePrice);
        }

        // 2. Saved products that the current filter hides are NOT on screen, so their
        //    selection state is unknown — keep them exactly as saved (re-fanned across
        //    whatever stores are selected now). Without this, opening a campaign with a
        //    filter active and pressing Update silently deleted every other product.
        var visibleItemIds = GridItems.Select(g => g.Item.Id).ToHashSet();
        foreach (var group in _baselineDetails
                     .Where(d => !visibleItemIds.Contains(d.ItemId))
                     .GroupBy(d => d.ItemId))
        {
            var template = group.First();
            AddRows(template.ItemId, template.ItemName, template.DiscountValue,
                    template.DiscountTypeId, template.DiscountTypeName, template.CurrentSalePrice);
        }

        Campaign.DiscountDetails = details;

        // Re-baseline so repeated updates (toggle, type change, store change) stay
        // idempotent and keep the hidden rows alive.
        _baselineDetails = details
            .Select(d => new DiscountManagerDetailsDTO
            {
                Id = d.Id,
                DiscountManagerId = d.DiscountManagerId,
                ItemId = d.ItemId,
                ItemName = d.ItemName,
                StoreId = d.StoreId,
                StoreName = d.StoreName,
                DiscountValue = d.DiscountValue,
                DiscountTypeId = d.DiscountTypeId,
                DiscountTypeName = d.DiscountTypeName,
                CurrentSalePrice = d.CurrentSalePrice,
                CreatedDate = d.CreatedDate
            })
            .ToList();

        StateHasChanged();
    }

    protected async Task Save()
    {
        if (SelectedStoreIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                "Please select at least one store for which this campaign discount will be valid.");
            return;
        }

        UpdateCampaignDetails();

        // ── Exclusivity check ────────────────────────────────────────────────
        // Only conflicts the user has not already accepted are asked about here; this
        // also catches a stale page (someone else saved a conflicting campaign while
        // this one was open). The server re-checks and performs the move on save.
        if (Campaign.IsActive)
        {
            var detailItemIds = Campaign.DiscountDetails.Select(d => d.ItemId).Distinct().ToList();
            var (allowed, conflicts) = await ConfirmConflictsAsync(detailItemIds);
            if (!allowed)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Cancelled",
                    "The campaign was not saved.");
                return;
            }
            Campaign.AcknowledgeConflicts = conflicts.Count > 0;
        }
        else
        {
            // An inactive campaign never blocks (and never moves) anything.
            Campaign.AcknowledgeConflicts = false;
        }

        var userObj = await sessionStorage.GetAsync<string>("UserId");
        if (!string.IsNullOrEmpty(userObj.Value))
            Campaign.CreatedBy = userObj.Value;

        var validation = await _serviceUnitOfWork.DiscountManagerService.Validate(Campaign);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.DiscountManagerService.SaveUpdate(Campaign);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Campaign updated successfully!" : "Campaign created successfully!");
                NavigationManager.NavigateTo("/DiscountManagerList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save campaign: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void OnStoresChanged(object? value)
    {
        _allStoresSelected = Stores.Count > 0 && SelectedStoreIds.Count == Stores.Count;
        InvalidateConflictAcknowledgements();
        UpdateCampaignDetails();
    }

    /// <summary>Changing the validity window invalidates every previous conflict answer.</summary>
    protected void OnCampaignDatesChanged()
    {
        InvalidateConflictAcknowledgements();
        UpdateCampaignDetails();
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/DiscountManagerList");
    }

    protected async Task Reset()
    {
        BulkDiscountValue = 0;
        SearchText = "";
        SelectedCampaignId = null;
        Id = null;
        SelectedStoreIds = new();
        _allStoresSelected = false;
        _acknowledgedConflicts.Clear();
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedColorId = null;
        SelectedSizeId = null;
        SelectedCatalogueId = null;
        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;
        Campaign = _serviceUnitOfWork.DiscountManagerService.CreateNew();
        await LoadData();
    }

    protected decimal CalcFinalPrice(DiscountGridItem gi)
    {
        if (gi.DiscountValue <= 0) return gi.CurrentSalePrice;
        return gi.DiscountTypeId == 2
            ? Math.Max(0, gi.CurrentSalePrice - gi.DiscountValue)
            : gi.CurrentSalePrice * (1 - Math.Min(gi.DiscountValue, 100) / 100);
    }

    protected string SourceBadgeHtml(DiscountGridItem gi)
    {
        if (gi.Source == "scan")
            return "<span class='badge badge-scan'><i class='ti ti-barcode' style='font-size:11px'></i> Scanned</span>";
        if (gi.Source == "all")
            return "<span class='badge badge-all'><i class='ti ti-check' style='font-size:11px'></i> All</span>";
        return "";
    }

    public void Dispose()
    {
        NavigationGuard.IsGuardActive = false;
    }
}
