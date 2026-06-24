using JM.UI.Entities.Model.Coupon;
using JM.UI.Entities.Model.Items;
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

    // Product selector
    protected List<CouponProductRow> ProductRows { get; set; } = new();
    protected string SearchText { get; set; } = "";

    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Coupon" : "New Coupon Discount";

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
        ProductRows = new List<CouponProductRow>();

        foreach (var item in AllItems)
        {
            var existing = Coupon.CouponItems?.FirstOrDefault(ci => ci.ItemId == item.Id);
            ProductRows.Add(new CouponProductRow
            {
                Item = item,
                Selected = existing != null,
                MinQty = existing?.MinQty,
                DiscountOverride = existing?.ItemDiscountOverride
            });
        }
    }

    protected void ToggleProduct(CouponProductRow row)
    {
        row.Selected = !row.Selected;
        if (!row.Selected)
        {
            row.MinQty = null;
            row.DiscountOverride = null;
        }
    }

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
                MinQty = r.MinQty,
                ItemDiscountOverride = r.DiscountOverride,
                AssignedDate = DateTime.Now
            })
            .ToList();

        if (!Coupon.CouponItems.Any())
            Coupon.ApplicableToAll = true;
    }

    protected void Cancel() => NavigationManager.NavigateTo("/CouponList");

    protected async Task Reset()
    {
        SearchText = string.Empty;
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
    public int? MinQty { get; set; }
    public decimal? DiscountOverride { get; set; }
}
