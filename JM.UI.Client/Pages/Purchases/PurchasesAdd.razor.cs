using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Purchases
{
    public partial class PurchasesAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseModelDTO Purchase { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<PurchaseOrderModelDTO> PurchaseOrders { get; set; } = new List<PurchaseOrderModelDTO>();
        protected IEnumerable<VoucherModelDTO> Vouchers { get; set; } = new List<VoucherModelDTO>();

        // Line Item Lookups
        protected IEnumerable<JM.UI.Entities.Model.Items.ItemModelDTO> ItemsList = new List<JM.UI.Entities.Model.Items.ItemModelDTO>();
        protected IEnumerable<JM.UI.Entities.Model.Colors.ColorsDTO> ColorsList = new List<JM.UI.Entities.Model.Colors.ColorsDTO>();
        protected IEnumerable<JM.UI.Entities.Model.Sizes.SizesDTO> SizesList = new List<JM.UI.Entities.Model.Sizes.SizesDTO>();

        protected JM.UI.Entities.Model.PurchaseItems.PurchaseItemsDTO NewItem { get; set; } = new();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase" : "New Purchase";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();

            if (IsEditMode)
            {
                await LoadPurchase();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
                var poTask = _serviceUnitOfWork.PurchaseOrderService.GetPurchaseOrders();
                var vouchersTask = _serviceUnitOfWork.VoucherService.GetVouchers();
                var itemsTask = _serviceUnitOfWork.ItemService.GetItems();
                var colorsTask = _serviceUnitOfWork.ColorsService.GetColorss();
                var sizesTask = _serviceUnitOfWork.SizesService.GetSizess();

                await Task.WhenAll(suppliersTask, storesTask, poTask, vouchersTask, itemsTask, colorsTask, sizesTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
                PurchaseOrders = await poTask;
                Vouchers = await vouchersTask;
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

        private async Task LoadPurchase()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseService.GetPurchaseById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase not found.");
                    NavigationManager.NavigateTo("/PurchasesList");
                    return;
                }

                Purchase = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase: {ex.Message}");
                NavigationManager.NavigateTo("/PurchasesList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void CalculateTotals()
        {
            // Auto-calculate Total (Sub Total) from items
            Purchase.Total = Purchase.PurchaseItems.Sum(i => i.Quantity * i.TradePrice);
            
            // Calculate Discount Amount from Discount Percentage
            if (Purchase.DiscountPercentage > 0)
            {
                Purchase.Discount = (Purchase.Total * Purchase.DiscountPercentage) / 100;
            }
            
            // Calculate VAT Amount from VAT Percentage (applied to Total)
            if (Purchase.VATPercentage > 0)
            {
                Purchase.VAT = (Purchase.Total * Purchase.VATPercentage) / 100;
            }

            // Calculate Net Total = SubTotal + VAT + Labour + Transport - Discount
            Purchase.NetTotal = (Purchase.Total + Purchase.LabourCharge + Purchase.TransportCost + Purchase.VAT) - Purchase.Discount;
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

            Purchase.PurchaseItems.Add(NewItem);
            NewItem = new JM.UI.Entities.Model.PurchaseItems.PurchaseItemsDTO(); // Reset for next item
            
            // Recalculate totals after adding item
            CalculateTotals();
        }

        protected void RemoveLineItem(JM.UI.Entities.Model.PurchaseItems.PurchaseItemsDTO item)
        {
            Purchase.PurchaseItems.Remove(item);
            
            // Recalculate totals after removing item
            CalculateTotals();
        }

        protected async Task Save()
        {
            if (!Purchase.PurchaseItems.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty Items", "Please add at least one item to the purchase.");
                return;
            }

            try
            {
                IsProcessing = true;
                
                // Ensure calculations are fresh
                CalculateTotals();

                // Set edit info
                if (IsEditMode)
                {
                    Purchase.EditedDate = DateTime.Now;
                    // Purchase.EditedBy = ... (get from auth context if needed)
                }

                var result = await _serviceUnitOfWork.PurchaseService.SaveUpdatePurchase(Purchase);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase updated successfully!" : "Purchase created successfully!");
                    NavigationManager.NavigateTo("/PurchasesList");
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
            NavigationManager.NavigateTo("/PurchasesList");
        }
    }
}
