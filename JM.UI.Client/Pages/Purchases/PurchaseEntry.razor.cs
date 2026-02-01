using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Purchases
{
    public partial class PurchaseEntryComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        // Purchase Data
        protected PurchaseDTO Purchase { get; set; } = new();
        protected List<PurchaseItemDTO> PurchaseItems { get; set; } = new();
        protected PurchaseItemDTO CurrentItem { get; set; } = new();

        // Lookup Data
        protected IEnumerable<dynamic> Suppliers { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> Stores { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> Groups { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> SubGroups { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> Items { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> Colors { get; set; } = new List<dynamic>();
        protected IEnumerable<dynamic> Sizes { get; set; } = new List<dynamic>();

        // UI State
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Entry" : "New Purchase Entry";

        // Grid Reference
        protected RadzenDataGrid<PurchaseItemDTO> ItemsGrid = default!;

        // Product Type Options
        protected List<string> ProductTypes = new()
        {
            "Sell Product",
            "Raw Material",
            "Both",
            "Consume",
            "Combo Package"
        };

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookupData();

            if (IsEditMode)
            {
                await LoadPurchase();
            }
            else
            {
                InitializePurchase();
            }
        }

        // =============================================
        // Initialization Methods
        // =============================================
        private void InitializePurchase()
        {
            Purchase = _serviceUnitOfWork.PurchaseService.CreateNewPurchase();
            PurchaseItems = new List<PurchaseItemDTO>();
            CurrentItem = CreateNewItem();
        }

        private PurchaseItemDTO CreateNewItem()
        {
            return new PurchaseItemDTO
            {
                IsSaleable = true,
                ProductType = "Sell Product",
                Quantity = 1,
                IsActive = true
            };
        }

        // =============================================
        // Data Loading Methods
        // =============================================
        private async Task LoadLookupData()
        {
            try
            {
                // Load all lookup data (you'll need to implement these methods)
                Suppliers = await LoadSuppliers();
                Stores = await LoadStores();
                Groups = await LoadGroups();
                Colors = await LoadColors();
                Sizes = await LoadSizes();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
        }

        private async Task LoadPurchase()
        {
            try
            {
                IsLoading = true;
                var purchase = await _serviceUnitOfWork.PurchaseService.GetPurchaseById(Id!.Value);

                if (purchase == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase not found.");
                    NavigationManager.NavigateTo("/PurchaseList");
                    return;
                }

                Purchase = purchase;
                PurchaseItems = purchase.PurchaseItems;
                CurrentItem = CreateNewItem();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // =============================================
        // Lookup Load Methods (Implement based on your services)
        // =============================================
        private async Task<IEnumerable<dynamic>> LoadSuppliers()
        {
            // Implement supplier loading
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> LoadStores()
        {
            // Implement store loading
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> LoadGroups()
        {
            // Implement group loading
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> LoadColors()
        {
            // Implement color loading
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> LoadSizes()
        {
            // Implement size loading
            return new List<dynamic>();
        }

        // =============================================
        // Item Management Methods
        // =============================================
        protected async Task OnGroupChange(object value)
        {
            if (value != null)
            {
                var groupId = Convert.ToInt32(value);
                SubGroups = await LoadSubGroupsByGroup(groupId);
                Items = new List<dynamic>();
            }
        }

        protected async Task OnSubGroupChange(object value)
        {
            if (value != null)
            {
                var subGroupId = Convert.ToInt32(value);
                Items = await LoadItemsBySubGroup(subGroupId);
            }
        }

        protected async Task OnItemChange(object value)
        {
            if (value != null)
            {
                var itemId = Convert.ToInt32(value);
                var item = await LoadItemDetails(itemId);

                if (item != null)
                {
                    CurrentItem.ShadeNo = item.ShadeNo;
                    CurrentItem.ProductPricePercentage = item.ProductPricePercentage;
                }
            }
        }

        private async Task<IEnumerable<dynamic>> LoadSubGroupsByGroup(int groupId)
        {
            // Implement subgroup loading by group
            return new List<dynamic>();
        }

        private async Task<IEnumerable<dynamic>> LoadItemsBySubGroup(int subGroupId)
        {
            // Implement items loading by subgroup
            return new List<dynamic>();
        }

        private async Task<dynamic?> LoadItemDetails(int itemId)
        {
            // Implement item details loading
            return null;
        }

        // =============================================
        // Barcode Methods
        // =============================================
        protected async Task GenerateBarcode()
        {
            try
            {
                if (CurrentItem.ItemId <= 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Warning", "Please select an item first.");
                    return;
                }

                var request = new BarcodeGenerationRequestDTO
                {
                    ShadeNo = CurrentItem.ShadeNo,
                    ColorName = Colors.FirstOrDefault(c => c.Id == CurrentItem.ColorId)?.Name,
                    SizeName = Sizes.FirstOrDefault(s => s.Id == CurrentItem.SizeId)?.Name,
                    ItemId = CurrentItem.ItemId
                };

                var barcode = await _serviceUnitOfWork.PurchaseService.GenerateBarcode(request);
                CurrentItem.Barcode = barcode;

                notificationService.Notify(NotificationSeverity.Success, "Success", "Barcode generated successfully!");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to generate barcode: {ex.Message}");
            }
        }

        protected async Task SearchBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;

            try
            {
                var result = await _serviceUnitOfWork.PurchaseService.SearchByBarcode(barcode);

                if (result.Found && result.Item != null)
                {
                    CurrentItem = result.Item;
                    notificationService.Notify(NotificationSeverity.Success, "Found", "Item found and loaded!");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Not Found", result.Message ?? "No item found with this barcode.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Error searching barcode: {ex.Message}");
            }
        }

        // =============================================
        // Item Grid Methods
        // =============================================
        protected void AddItemToGrid()
        {
            var validation = ValidateCurrentItem();
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            // Calculate item total
            _serviceUnitOfWork.PurchaseService.CalculateItemTotal(CurrentItem);

            // Add to grid
            PurchaseItems.Add(CurrentItem);
            CurrentItem = CreateNewItem();

            // Recalculate purchase totals
            CalculateTotals();

            notificationService.Notify(NotificationSeverity.Success, "Success", "Item added to purchase!");
            StateHasChanged();
        }

        protected void EditItem(PurchaseItemDTO item)
        {
            CurrentItem = item;
            PurchaseItems.Remove(item);
            StateHasChanged();
        }

        protected void DeleteItem(PurchaseItemDTO item)
        {
            PurchaseItems.Remove(item);
            CalculateTotals();
            notificationService.Notify(NotificationSeverity.Success, "Success", "Item removed from purchase!");
            StateHasChanged();
        }

        private (bool IsValid, string ErrorMessage) ValidateCurrentItem()
        {
            if (CurrentItem.ItemId <= 0)
                return (false, "Please select an item.");

            if (CurrentItem.Quantity <= 0)
                return (false, "Quantity must be greater than 0.");

            if (CurrentItem.PurchasePrice <= 0)
                return (false, "Purchase price must be greater than 0.");

            if (string.IsNullOrWhiteSpace(CurrentItem.Barcode))
                return (false, "Barcode is required.");

            if (CurrentItem.IsSaleable && (!CurrentItem.SalePrice.HasValue || CurrentItem.SalePrice.Value <= 0))
                return (false, "Saleable items must have a sale price.");

            if (CurrentItem.IsSaleable && CurrentItem.SalePrice.HasValue && CurrentItem.SalePrice.Value <= CurrentItem.PurchasePrice)
                return (false, "Sale price must be greater than purchase price.");

            return (true, string.Empty);
        }

        // =============================================
        // Calculation Methods
        // =============================================
        protected void CalculateItemTotal(PurchaseItemDTO? _)
        {
            _serviceUnitOfWork.PurchaseService.CalculateItemTotal(CurrentItem);
            StateHasChanged();
        }


        protected void CalculateTotals(PurchaseItemDTO? _)
        {
            Purchase.TotalAmount = _serviceUnitOfWork.PurchaseService.CalculatePurchaseTotal(PurchaseItems);

            Purchase.OtherCostTotal = PurchaseItems.Sum(x => x.OtherCost ?? 0);
            Purchase.CarryingCostTotal = PurchaseItems.Sum(x => x.CarryingCost ?? 0);
            Purchase.OperationalCostTotal = PurchaseItems.Sum(x => x.OperationalCost ?? 0);
            Purchase.VatAmount = PurchaseItems.Sum(x => x.VatAmount ?? 0);

            Purchase.NetAmount = Purchase.TotalAmount - (Purchase.DiscountAmount ?? 0);
            Purchase.DueAmount = Purchase.NetAmount - (Purchase.PaidAmount ?? 0);

            StateHasChanged();
        }

        // =============================================
        // Save Methods
        // =============================================
        protected async Task Save()
        {
            if (!PurchaseItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please add at least one item.");
                return;
            }

            var userObj = await sessionStorage.GetAsync<string>("UserId");
            int? userId = null;
            if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            if (IsEditMode)
            {
                Purchase.LastModifiedBy = userId;
            }
            else
            {
                Purchase.CreatedBy = userId;
            }

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.PurchaseService.SaveUpdatePurchase(Purchase, PurchaseItems);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase updated successfully!" : "Purchase created successfully!");
                    NavigationManager.NavigateTo("/PurchaseList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseList");
        }

        protected async Task Reset()
        {
            if (IsEditMode)
            {
                await LoadPurchase();
            }
            else
            {
                InitializePurchase();
            }
            StateHasChanged();
        }
    }
}
