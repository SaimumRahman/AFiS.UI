using JM.UI.Entities.Model.Discount;
using JM.UI.Entities.Model.Items;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Discount;

public partial class DiscountManagerAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected DiscountManagerDTO Campaign { get; set; } = new();
    protected List<ItemDTO> AllItems { get; set; } = new();
    protected List<DiscountTypeDTO> DiscountTypes { get; set; } = new();

    // Grid display items — built from AllItems + Campaign.DiscountDetails
    protected List<DiscountGridItem> GridItems { get; set; } = new();

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

            // Load discount types
            DiscountTypes = (await _serviceUnitOfWork.DiscountManagerService.GetDiscountTypes()).ToList();

            // Load all items
            var items = await _serviceUnitOfWork.ItemService.GetItems();
            AllItems = items.ToList();

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

            BuildGridItems();
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

    private void BuildGridItems()
    {
        GridItems = new List<DiscountGridItem>();

        foreach (var item in AllItems)
        {
            var detail = Campaign.DiscountDetails.FirstOrDefault(d => d.ItemId == item.Id);
            GridItems.Add(new DiscountGridItem
            {
                Item = item,
                Selected = detail != null,
                DiscountValue = detail?.DiscountValue ?? 0,
                DiscountTypeId = detail?.DiscountTypeId ?? 1,
                DiscountTypeName = detail?.DiscountTypeName ?? DiscountTypes.FirstOrDefault()?.TypeName ?? "Percentage",
                Source = detail != null ? "loaded" : "",
                CurrentSalePrice = item.SalePrice ?? 0
            });
        }
    }

    protected void ToggleItem(DiscountGridItem gridItem)
    {
        gridItem.Selected = !gridItem.Selected;
        if (!gridItem.Selected)
        {
            gridItem.DiscountValue = 0;
            gridItem.Source = "";
        }
        else if (string.IsNullOrEmpty(gridItem.Source))
        {
            gridItem.Source = "manual";
        }
        UpdateCampaignDetails();
    }

    protected void ToggleAll()
    {
        var select = !AllSelected;
        foreach (var gi in GridItems)
        {
            gi.Selected = select;
            if (!select)
            {
                gi.DiscountValue = 0;
                gi.Source = "";
            }
            else if (string.IsNullOrEmpty(gi.Source))
            {
                gi.Source = "all";
            }
        }
        if (!select)
            BulkDiscountValue = 0;
        UpdateCampaignDetails();
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

    protected void SimulateScan()
    {
        var unscanned = GridItems.Where(x => !x.Selected).ToList();
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
        UpdateCampaignDetails();
    }

    protected async Task OnSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            SearchByBarcode();
        }
    }

    protected void SearchByBarcode()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        var text = SearchText.Trim().ToLower();
        var match = GridItems.FirstOrDefault(x =>
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
            UpdateCampaignDetails();
        }
        else
        {
            notificationService.Notify(NotificationSeverity.Warning, "Not found", "⚠ No product matched");
        }
    }

    private void UpdateCampaignDetails()
    {
        Campaign.DiscountDetails = GridItems
            .Where(x => x.Selected && x.DiscountValue > 0)
            .Select(gi => new DiscountManagerDetailsDTO
            {
                ItemId = gi.Item.Id,
                ItemName = gi.Item.Name,
                StoreName = gi.Item.StoreName,
                DiscountValue = gi.DiscountValue,
                DiscountTypeId = gi.DiscountTypeId,
                DiscountTypeName = gi.DiscountTypeName,
                CurrentSalePrice = gi.CurrentSalePrice,
                CreatedDate = Campaign.CreatedDate == default ? DateTime.Now : Campaign.CreatedDate
            })
            .ToList();
        StateHasChanged();
    }

    protected async Task Save()
    {
        UpdateCampaignDetails();

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

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/DiscountManagerList");
    }

    protected async Task Reset()
    {
        BulkDiscountValue = 0;
        SearchText = "";
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
        // Deactivate when leaving the page
        NavigationGuard.IsGuardActive = false;
    }
    
}
