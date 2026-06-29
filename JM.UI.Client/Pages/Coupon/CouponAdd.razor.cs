using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Coupon;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Coupon;

public partial class CouponAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CouponDTO Coupon { get; set; } = new();
    protected IEnumerable<CouponTypeDTO> CouponTypes { get; set; } = new List<CouponTypeDTO>();
    protected List<ItemDTO> AllItems { get; set; } = new();

    // Product grid
    protected List<CouponProductRow> ProductRows { get; set; } = new();

    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Coupon" : "New Coupon Discount";

    // Scan / search
    protected string SearchText { get; set; } = "";
    protected bool Scanning { get; set; }

    // Bulk
    protected bool AllSelected => ProductRows.Count > 0 && ProductRows.All(x => x.Selected);
    protected int SelectedCount => ProductRows.Count(x => x.Selected);

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

            CouponTypes = await _serviceUnitOfWork.CouponTypeService.GetAll();

            var items = await _serviceUnitOfWork.ItemService.GetItems();
            AllItems = items.ToList();

            // Load filter lookup data
            Groups = (await _serviceUnitOfWork.GroupService.GetGroups()).ToList();
            AllSubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups()).ToList();
            AllDesigns = (await _serviceUnitOfWork.DesignService.GetDesigns()).ToList();
            Colors = (await _serviceUnitOfWork.ColorsService.GetColorss()).ToList();
            Sizes = (await _serviceUnitOfWork.SizesService.GetSizess()).ToList();
            Catalogues = (await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues()).ToList();
            FilteredSubGroups = AllSubGroups;
            FilteredDesigns = AllDesigns;

            if (IsEditMode)
            {
                Coupon = await _serviceUnitOfWork.CouponService.GetById(Id!.Value);
                if (Coupon == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Coupon not found.");
                    NavigationManager.NavigateTo("/CouponList");
                    return;
                }
            }
            else
            {
                Coupon = _serviceUnitOfWork.CouponService.CreateNew();
            }

            BuildProductRows();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load data: {ex.Message}");
            if (IsEditMode)
                NavigationManager.NavigateTo("/CouponList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildProductRows()
    {
        var filtered = FilterItems();
        ProductRows = new List<CouponProductRow>();

        foreach (var item in filtered)
        {
            var existing = Coupon.CouponItems?.FirstOrDefault(ci => ci.ItemId == item.Id);
            ProductRows.Add(new CouponProductRow
            {
                Item = item,
                Selected = existing != null,
                Source = existing != null ? "loaded" : ""
            });
        }
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

    // ── Filter handlers ──

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

        BuildProductRows();
        StateHasChanged();
    }

    protected void OnSubGroupChanged(int? subGroupId)
    {
        SelectedSubGroupId = subGroupId;
        SelectedDesignId = null;

        FilteredDesigns = subGroupId.HasValue
            ? AllDesigns.Where(d => d.SubGroupId == subGroupId.Value).ToList()
            : AllDesigns;

        BuildProductRows();
        StateHasChanged();
    }

    protected void OnDesignChanged(int? designId)
    {
        SelectedDesignId = designId;
        BuildProductRows();
        StateHasChanged();
    }

    protected void OnColorChanged(int? colorId)
    {
        SelectedColorId = colorId;
        BuildProductRows();
        StateHasChanged();
    }

    protected void OnSizeChanged(int? sizeId)
    {
        SelectedSizeId = sizeId;
        BuildProductRows();
        StateHasChanged();
    }

    protected void OnCatalogueChanged(int? catalogueId)
    {
        SelectedCatalogueId = catalogueId;
        BuildProductRows();
        StateHasChanged();
    }

    // ── Scan / Search ──

    protected async Task OnSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await SearchByBarcode();
    }

    protected async Task SearchByBarcode()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        var text = SearchText.Trim().ToLower();
        var match = ProductRows.FirstOrDefault(x =>
            (x.Item.Barcode ?? "").ToLower() == text ||
            x.Item.Name.ToLower().Contains(text));
        if (match != null)
        {
            match.Selected = true;
            match.Source = "scan";
            Scanning = true;
            InvokeAsync(async () =>
            {
                await Task.Delay(800);
                Scanning = false;
                StateHasChanged();
            });
            notificationService.Notify(NotificationSeverity.Success, "Found", $"✓ {match.Item.Name}");
            SearchText = "";
        }
        else
        {
            var apiItem = await _serviceUnitOfWork.TransferService.SearchByBarcodeExact(SearchText.Trim(), 4);
            if (apiItem != null)
            {
                var row = new CouponProductRow
                {
                    Item = apiItem,
                    Selected = true,
                    Source = "scan"
                };
                ProductRows.Add(row);
                Scanning = true;
                InvokeAsync(async () =>
                {
                    await Task.Delay(800);
                    Scanning = false;
                    StateHasChanged();
                });
                notificationService.Notify(NotificationSeverity.Success, "Found", $"✓ {apiItem.Name} (from server)");
                SearchText = "";
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Warning, "Not found", "⚠ No product matched");
            }
        }
    }

    protected void SimulateScan()
    {
        var unscanned = ProductRows.Where(x => !x.Selected).ToList();
        if (unscanned.Count == 0) return;

        var target = unscanned[new Random().Next(unscanned.Count)];
        target.Selected = true;
        target.Source = "scan";
        Scanning = true;
        InvokeAsync(async () =>
        {
            await Task.Delay(800);
            Scanning = false;
            StateHasChanged();
        });
        SearchText = target.Item.Barcode ?? target.Item.Name;
        notificationService.Notify(NotificationSeverity.Success, "Scanned", $"✓ {target.Item.Name}");
    }

    // ── Product selection ──

    protected void ToggleItem(CouponProductRow row)
    {
        row.Selected = !row.Selected;
        if (!row.Selected)
        {
            row.Source = "";
        }
        else if (string.IsNullOrEmpty(row.Source))
        {
            row.Source = "manual";
        }
    }

    protected void ToggleAll()
    {
        var select = !AllSelected;
        foreach (var row in ProductRows)
        {
            row.Selected = select;
            if (!select)
            {
                row.Source = "";
            }
            else if (string.IsNullOrEmpty(row.Source))
            {
                row.Source = "all";
            }
        }
    }

    // ── Save ──

    protected async Task Save()
    {
        UpdateCouponItems();

        if (string.IsNullOrWhiteSpace(Coupon.CouponCode))
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Coupon code is required.");
            return;
        }

        if (Coupon.CouponTypeId <= 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select a coupon type.");
            return;
        }

        if (Coupon.DiscountValue <= 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Discount value must be greater than 0.");
            return;
        }

        try
        {
            IsProcessing = true;

            var userObj = await sessionStorage.GetAsync<string>("UserId");
            if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out var uid))
                Coupon.CreatedBy = uid;

            var result = await _serviceUnitOfWork.CouponService.SaveUpdate(Coupon);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Coupon updated successfully!" : "Coupon created successfully!");
                NavigationManager.NavigateTo("/CouponList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save coupon: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void UpdateCouponItems()
    {
            Coupon.CouponItems = ProductRows
            .Where(r => r.Selected)
            .Select(r => new CouponItemDTO
            {
                ItemId = r.Item.Id,
                ItemName = r.Item.Name,
                Barcode = r.Item.Barcode,
                AssignedDate = DateTime.Now
            })
            .ToList();

        if (!Coupon.CouponItems.Any())
            Coupon.ApplicableToAll = false;
    }

    protected void Cancel() => NavigationManager.NavigateTo("/CouponList");

    protected async Task Reset()
    {
        SearchText = string.Empty;
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedColorId = null;
        SelectedSizeId = null;
        SelectedCatalogueId = null;
        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;

        if (IsEditMode)
            await LoadData();
        else
        {
            Coupon = _serviceUnitOfWork.CouponService.CreateNew();
            BuildProductRows();
        }
        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationGuard.IsGuardActive = false;
    }
}

public class CouponProductRow
{
    public ItemDTO Item { get; set; } = new();
    public bool Selected { get; set; }
    public string Source { get; set; } = "";
}
