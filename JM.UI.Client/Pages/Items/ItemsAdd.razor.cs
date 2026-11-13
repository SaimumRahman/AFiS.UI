using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.ItemFeatures;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Service.ItemCatalogue;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Items
{
    public partial class ItemsAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public IItemCatalogueService _catalogueService { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoadingByComponent { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Item" : "Edit Product Information";

        // Fetch
        protected string BaseBarcodeInput { get; set; } = string.Empty;

        // Locked
        protected string LockedBaseBarcode { get; set; } = string.Empty;
        protected string LockedGroupName { get; set; } = string.Empty;
        protected string LockedSubGroupName { get; set; } = string.Empty;
        protected string LockedColorName { get; set; } = string.Empty;

        // Features multi-select
        protected IEnumerable<ItemFeatureDTO> Features { get; set; } = new List<ItemFeatureDTO>();
        protected IEnumerable<int> SelectedFeatureIds { get; set; } = new List<int>();
        protected List<string> NewFeatureNames { get; set; } = new();
        protected string NewFeatureInput { get; set; } = string.Empty;

        // Global metadata
        protected string GlobalName { get; set; } = string.Empty;
        protected int? GlobalBrandId { get; set; }
        protected int? GlobalOriginId { get; set; }
        protected int? GlobalUnitId { get; set; }
        protected int? GlobalCatalogueId { get; set; }
        protected string GlobalFeatures { get; set; } = string.Empty;
        protected string? GlobalImageBase64 { get; set; }
        protected string? GlobalOldBarcode { get; set; }

        // Lookups
        protected IEnumerable<ColorsDTO> Colors { get; set; } = new List<ColorsDTO>();
        protected IEnumerable<ItemBrandDTO> Brands { get; set; } = new List<ItemBrandDTO>();
        protected IEnumerable<ItemOriginDTO> Origins { get; set; } = new List<ItemOriginDTO>();
        protected IEnumerable<MesurementUnitModelDTO> MeasurementUnitsList { get; set; } = new List<MesurementUnitModelDTO>();
        protected IEnumerable<ItemCatalogueDTO> Catalogues { get; set; } = new List<ItemCatalogueDTO>();

        // Variants
        protected List<ItemDTO> Variants { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            await LoadLookups();

            if (IsEditMode)
            {
                var item = await _serviceUnitOfWork.ItemService.GetItemById(Id!.Value);
                if (item == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Item not found.");
                    NavigationManager.NavigateTo("/ItemsList");
                    return;
                }
                var baseBc = DeriveBaseBarcode(item.Barcode);
                await LoadVariants(baseBc);
            }
        }

        private async Task LoadLookups()
        {
            try
            {
                var brandsTask = _serviceUnitOfWork.ItemBrandService.GetItemBrands();
                var originsTask = _serviceUnitOfWork.ItemOriginService.GetItemOrigins();
                var unitsTask = _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits();
                var cataloguesTask = _catalogueService.GetItemCatalogues();
                var featuresTask = _serviceUnitOfWork.ItemFeatureService.GetItemFeatures();
                var colorsTask = _serviceUnitOfWork.ColorsService.GetColorss();
                await Task.WhenAll(brandsTask, originsTask, unitsTask, cataloguesTask, featuresTask, colorsTask);
                Brands = brandsTask.Result ?? new List<ItemBrandDTO>();
                Origins = originsTask.Result ?? new List<ItemOriginDTO>();
                MeasurementUnitsList = unitsTask.Result ?? new List<MesurementUnitModelDTO>();
                Catalogues = cataloguesTask.Result ?? new List<ItemCatalogueDTO>();
                Features = featuresTask.Result ?? new List<ItemFeatureDTO>();
                Colors = colorsTask.Result ?? new List<ColorsDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookups: {ex.Message}");
            }
        }

        private string DeriveBaseBarcode(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return string.Empty;
            var idx = barcode.LastIndexOf('-');
            if (idx > 0) return barcode.Substring(0, idx);
            return barcode;
        }

        private async Task LoadVariants(string baseBarcode)
        {
            try
            {
                IsLoadingByComponent = true;
                var items = await _serviceUnitOfWork.ItemService.GetItemsByBaseBarcode(baseBarcode);
                var list = (items ?? new List<ItemDTO>()).ToList();
                if (!list.Any())
                {
                    // fallback: treat the single item as the only variant
                    var single = await _serviceUnitOfWork.ItemService.GetItemById(Id!.Value);
                    if (single != null) list.Add(single);
                }
                Variants = list;
                if (Variants.Any())
                {
                    var first = Variants.First();
                    LockedBaseBarcode = DeriveBaseBarcode(first.Barcode);
                    LockedGroupName = first.GroupName ?? string.Empty;
                    LockedSubGroupName = first.SubGroupName ?? string.Empty;
                    // color name not in DTO; use ColorId string
                    LockedColorName = Colors.FirstOrDefault(c => c.Id == first.ColorId)?.Name ?? first.ColorId?.ToString() ?? string.Empty;
                    GlobalName = first.Name;
                    GlobalBrandId = first.BrandId;
                    GlobalOriginId = first.OriginId;
                    GlobalUnitId = first.MesurementUnitId;
                    GlobalCatalogueId = first.CatalogueId;
                    GlobalFeatures = first.FeaturesDisplay ?? first.Features ?? string.Empty;
                    // Resolve selected feature ids from feature names
                    var names = (GlobalFeatures ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();
                    SelectedFeatureIds = Features
                        .Where(f => names.Contains(f.FeatureName, StringComparer.OrdinalIgnoreCase))
                        .Select(f => f.FeatureId)
                        .ToList();
                    NewFeatureNames = new List<string>();
                    GlobalImageBase64 = first.ImageBase64;
                    GlobalOldBarcode = first.OldBarcode; // need OldBarcode on ItemDTO? not present; use null
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load variants: {ex.Message}");
            }
            finally
            {
                IsLoadingByComponent = false;
            }
        }

        protected async Task FetchByBarcode()
        {
            if (string.IsNullOrWhiteSpace(BaseBarcodeInput)) return;
            await LoadVariants(BaseBarcodeInput.Trim());
        }

        protected async Task OnImageSelected(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file == null) return;
                using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                GlobalImageBase64 = $"data:{file.ContentType};base64,{base64}";
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Image upload failed: {ex.Message}");
            }
        }

        protected void AddNewFeature()
        {
            var trimmed = NewFeatureInput?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return;
            if (Features.Any(f => f.FeatureName.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                var existing = Features.First(f => f.FeatureName.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
                var ids = SelectedFeatureIds.ToList();
                if (!ids.Contains(existing.FeatureId))
                {
                    ids.Add(existing.FeatureId);
                    SelectedFeatureIds = ids;
                }
                notificationService.Notify(NotificationSeverity.Info, "Feature exists", $"'{trimmed}' is already in the list.");
            }
            else if (!NewFeatureNames.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            {
                NewFeatureNames.Add(trimmed);
            }
            NewFeatureInput = string.Empty;
        }

        protected void RemoveNewFeature(string name)
        {
            NewFeatureNames.Remove(name);
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                // Resolve features (create new ones)
                var allFeatureIds = SelectedFeatureIds.ToList();
                foreach (var fname in NewFeatureNames.ToList())
                {
                    var featureResult = await _serviceUnitOfWork.ItemFeatureService.SaveItemFeature(
                        new ItemFeatureDTO { FeatureName = fname });
                    if (featureResult != null && featureResult.IsSuccessStatus && featureResult.Id != null)
                    {
                        allFeatureIds.Add(Convert.ToInt32(featureResult.Id));
                    }
                }
                if (NewFeatureNames.Any())
                {
                    Features = await _serviceUnitOfWork.ItemFeatureService.GetItemFeatures() ?? new List<ItemFeatureDTO>();
                    NewFeatureNames.Clear();
                }
                var featuresDisplay = string.Join(", ", Features.Where(f => allFeatureIds.Contains(f.FeatureId)).Select(f => f.FeatureName));

                var currentUserId = await GetLocalStorageInt("UserId");

                foreach (var v in Variants)
                {
                    var update = new UpdateItemDTO
                    {
                        Id = v.Id,
                        Name = GlobalName,
                        SalePrice = v.SalePrice,
                        MesurementUnitId = GlobalUnitId,
                        AlarmLevel = v.AlarmLevel,
                        BrandId = GlobalBrandId,
                        OriginId = GlobalOriginId,
                        Origin = v.Origin,
                        Features = featuresDisplay,
                        FeatureIds = allFeatureIds,
                        FeaturesDisplay = featuresDisplay,
                        ItemWiseFeatureId = allFeatureIds.FirstOrDefault(),
                        PurchasePrice = v.PurchasePrice,
                        ImageBase64 = GlobalImageBase64,
                        OldBarcode = GlobalOldBarcode,
                        LastModifiedBy = currentUserId,
                        LastModifiedDate = DateTime.UtcNow
                    };
                    var result = await _serviceUnitOfWork.ItemService.UpdateItem(update);
                    if (!result.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                        return;
                    }
                }

                notificationService.Notify(NotificationSeverity.Success, "Success", "Item updated successfully!");
                NavigationManager.NavigateTo("/ItemsList");
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save item: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/ItemsList");
        }

        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && int.TryParse(result.Value, out int value))
                    return value;
            }
            catch { }
            return 0;
        }
    }
}
