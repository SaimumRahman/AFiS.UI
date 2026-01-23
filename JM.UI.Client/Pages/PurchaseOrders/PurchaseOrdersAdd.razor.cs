using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.PurchaseOrders
{
    public partial class PurchaseOrdersAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseOrderModelDTO PurchaseOrder { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        
        // Line Item Lookups
        protected IEnumerable<JM.UI.Entities.Model.Items.ItemModelDTO> ItemsList = new List<JM.UI.Entities.Model.Items.ItemModelDTO>();
        protected IEnumerable<JM.UI.Entities.Model.Colors.ColorsDTO> ColorsList = new List<JM.UI.Entities.Model.Colors.ColorsDTO>();
        protected IEnumerable<JM.UI.Entities.Model.Sizes.SizesDTO> SizesList = new List<JM.UI.Entities.Model.Sizes.SizesDTO>();

        protected JM.UI.Entities.Model.PurchaseOrderItems.PurchaseOrderItemsDTO NewItem { get; set; } = new();
        
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Order" : "New Purchase Order";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadPurchaseOrder();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                // Fetch Suppliers and Stores in parallel
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
                var itemsTask = _serviceUnitOfWork.ItemService.GetItems();
                var colorsTask = _serviceUnitOfWork.ColorsService.GetColorss();
                var sizesTask = _serviceUnitOfWork.SizesService.GetSizess();

                await Task.WhenAll(suppliersTask, storesTask, itemsTask, colorsTask, sizesTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
                ItemsList = await itemsTask;
                ColorsList = await colorsTask;
                SizesList = await sizesTask;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load lookup data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPurchaseOrder()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseOrderService.GetPurchaseOrderById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase Order not found.");
                    NavigationManager.NavigateTo("/PurchaseOrdersList");
                    return;
                }

                PurchaseOrder = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase order: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseOrdersList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void CalculateTotals()
        {
            // Auto-calculate total from items (not stored in PurchaseOrder table, but useful for UI display)
            var itemsTotal = PurchaseOrder.PurchaseOrderItems.Sum(i => i.Quantity * i.TradePrice);
            
            // Calculate Discount Amount from Discount Percentage
            decimal discountAmount = 0;
            if (PurchaseOrder.DiscountPercentage > 0)
            {
                discountAmount = (itemsTotal * PurchaseOrder.DiscountPercentage) / 100;
                PurchaseOrder.Discount = discountAmount;
            }
            
            // Calculate VAT Amount from VAT Percentage
            if (PurchaseOrder.VATPercentage > 0)
            {
                PurchaseOrder.VAT = (itemsTotal * PurchaseOrder.VATPercentage) / 100;
            }
            
            // Note: PurchaseOrder doesn't have NetTotal field, but calculations are ready for items
        }

        protected void AddLineItem()
        {
            if (NewItem.ItemId == 0 || NewItem.Quantity <= 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Warning", "Please select an item and enter quantity.");
                return;
            }

            var item = ItemsList.FirstOrDefault(i => i.Id == NewItem.ItemId);
            if (item != null)
            {
                NewItem.ItemName = item.Name;
            }

            if (NewItem.ColorId.HasValue)
            {
                NewItem.ColorName = ColorsList.FirstOrDefault(c => c.Id == NewItem.ColorId.Value)?.Name;
            }

            if (NewItem.SizeId.HasValue)
            {
                NewItem.SizeName = SizesList.FirstOrDefault(s => s.Id == NewItem.SizeId.Value)?.Name;
            }

            PurchaseOrder.PurchaseOrderItems.Add(NewItem);
            NewItem = new JM.UI.Entities.Model.PurchaseOrderItems.PurchaseOrderItemsDTO(); // Reset for next item
            
            // Recalculate totals after adding item
            CalculateTotals();
        }

        protected void RemoveLineItem(JM.UI.Entities.Model.PurchaseOrderItems.PurchaseOrderItemsDTO item)
        {
            PurchaseOrder.PurchaseOrderItems.Remove(item);
            
            // Recalculate totals after removing item
            CalculateTotals();
        }

        protected async Task Save()
        {
            if (!PurchaseOrder.PurchaseOrderItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty Items", "Please add at least one item to the purchase order.");
                return;
            }

            try
            {
                IsProcessing = true;
                
                // Calculate VAT amount if percentage is present but amount is 0 (optional logic, can be enhanced)
                if (PurchaseOrder.VATPercentage > 0 && PurchaseOrder.VAT == 0)
                {
                    // Logic would depend on total amount which isn't in header directly usually, 
                    // but assumes backend might calculate or user enters it manually.
                }

                var result = await _serviceUnitOfWork.PurchaseOrderService.SaveUpdatePurchaseOrder(PurchaseOrder);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase Order updated successfully!" : "Purchase Order created successfully!");
                    NavigationManager.NavigateTo("/PurchaseOrdersList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase order: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseOrdersList");
        }
    }
}
