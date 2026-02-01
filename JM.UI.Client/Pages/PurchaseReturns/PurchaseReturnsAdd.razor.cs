using JM.UI.Entities.Model.PurchaseReturns;
using JM.UI.Entities.Model.PurchaseReturnItems;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Entities.Model.Vouchers;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Users;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Sizes;
using Newtonsoft.Json;

namespace JM.UI.Client.Pages.PurchaseReturns
{
    public partial class PurchaseReturnsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected PurchaseReturnModelDTO PurchaseReturn { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> Suppliers { get; set; } = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<VoucherModelDTO> Vouchers { get; set; } = new List<VoucherModelDTO>();
        
        // Line Item Lookups
        protected IEnumerable<ItemDTO> ItemsList = new List<ItemDTO>();
        protected IEnumerable<ColorsDTO> ColorsList = new List<ColorsDTO>();
        protected IEnumerable<SizesDTO> SizesList = new List<SizesDTO>();

        protected PurchaseReturnItemModelDTO NewItem { get; set; } = new();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Purchase Return" : "New Purchase Return";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadInitialData();
            await SetUserInfo();

            if (IsEditMode)
            {
                await LoadPurchaseReturn();
            }
        }

        private async Task SetUserInfo()
        {
            try
            {
                var userInfoResult = await sessionStorage.GetAsync<string>("UserInfo");
                if (userInfoResult.Success && !string.IsNullOrEmpty(userInfoResult.Value))
                {
                    var userInfo = JsonConvert.DeserializeObject<AuthenticatedUserResponse>(userInfoResult.Value);
                    if (userInfo != null)
                    {
                        PurchaseReturn.UserName = userInfo.Username;
                        StateHasChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user info: {ex.Message}");
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                var suppliersTask = _serviceUnitOfWork.SupplierService.GetSuppliers();
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
                var vouchersTask = _serviceUnitOfWork.VoucherService.GetVouchers();
                var itemsTask = _serviceUnitOfWork.ItemService.GetItems();
                var colorsTask = _serviceUnitOfWork.ColorsService.GetColorss();
                var sizesTask = _serviceUnitOfWork.SizesService.GetSizess();

                await Task.WhenAll(suppliersTask, storesTask, vouchersTask, itemsTask, colorsTask, sizesTask);

                Suppliers = await suppliersTask;
                Stores = await storesTask;
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

        private async Task LoadPurchaseReturn()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.PurchaseReturnService.GetPurchaseReturnById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Purchase Return not found.");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                    return;
                }

                PurchaseReturn = result;
                
                // Load line items
                var items = await _serviceUnitOfWork.PurchaseReturnItemService.GetItemsByReturnId(Id.Value);
                PurchaseReturn.Items = items.ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load purchase return: {ex.Message}");
                NavigationManager.NavigateTo("/PurchaseReturnsList");
            }
            finally
            {
                IsLoading = false;
            }
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

            PurchaseReturn.Items.Add(NewItem);
            NewItem = new PurchaseReturnItemModelDTO(); // Reset for next item
        }

        protected void RemoveLineItem(PurchaseReturnItemModelDTO item)
        {
            PurchaseReturn.Items.Remove(item);
        }

        protected async Task Save()
        {
            if (!PurchaseReturn.Items.Any())
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty Items", "Please add at least one item to return.");
                return;
            }

            try
            {
                IsProcessing = true;
                
                // Populate UserName from session storage before saving
                var userInfoResult = await sessionStorage.GetAsync<string>("UserInfo");
                if (userInfoResult.Success && !string.IsNullOrEmpty(userInfoResult.Value))
                {
                    var userInfo = JsonConvert.DeserializeObject<AuthenticatedUserResponse>(userInfoResult.Value);
                    if (userInfo != null)
                    {
                        PurchaseReturn.UserName = userInfo.Username;
                    }
                }

                var result = await _serviceUnitOfWork.PurchaseReturnService.SaveUpdatePurchaseReturn(PurchaseReturn);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Purchase Return updated successfully!" : "Purchase Return created successfully!");
                    NavigationManager.NavigateTo("/PurchaseReturnsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save purchase return: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/PurchaseReturnsList");
        }
    }
}
