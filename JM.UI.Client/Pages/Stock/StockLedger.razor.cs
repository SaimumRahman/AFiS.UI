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

    protected RadzenDataGrid<StockLedgerDTO> StockLedgerGrid = default!;
    protected IEnumerable<StockLedgerDTO> StockLedgerList = new List<StockLedgerDTO>();
    protected IEnumerable<StoreDTO> StoreList = new List<StoreDTO>();
    protected bool IsLoading;

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
                                SelectedDesignId, SelectedBrandId);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load stock ledger: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        StockLedgerGrid?.Dispose();
    }
}
