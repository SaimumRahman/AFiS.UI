using JM.Infrastructure.Models;
using JM.UI.Client.Pages.Dialog.SalesPOS;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.MembershipType;
using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Shift;
using JM.UI.Entities.Model.Bank;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.SalesPOS
{
    public partial class SalesPOSComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        // ── Mode ──
        protected string _currentMode = "Sale";
        protected bool IsSaleMode => _currentMode == "Sale";
        protected bool IsBookingMode => _currentMode == "Booking";
        protected bool IsInvoicesMode => _currentMode == "Invoices";

        // ── Lookup Data ──
        protected List<StoreDTO> Stores { get; set; } = new();
        protected List<ColorsDTO> Colors { get; set; } = new();
        protected List<SizesDTO> Sizes { get; set; } = new();
        protected List<CustomerDetailsDTO> Customers { get; set; } = new();
        protected List<MembershipTypeDTO> MembershipTypes { get; set; } = new();
        protected List<ShiftDTO> Shifts { get; set; } = new();

        // ── Sale State ──
        protected SaleMasterDTO Sale { get; set; } = new();
        protected List<SaleDetailDTO> CartItems { get; set; } = new();
        protected RadzenDataGrid<SaleDetailDTO> CartGrid = default!;

        // ── Barcode / Product Search ──
        protected string BarcodeInput { get; set; } = "";
        protected decimal QtyInput { get; set; } = 1;
        protected ProductSearchDTO? SearchedProduct { get; set; }

        // ── Customer ──
        protected string CustomerSearchText { get; set; } = "";
        protected List<CustomerDetailsDTO> CustomerSuggestions { get; set; } = new();
        protected CustomerDetailsDTO? SelectedCustomer { get; set; }

        // ── Computed Values ──
        protected decimal SubTotal => CartItems.Sum(c => c.TotalPrice);
        protected decimal CalculatedVat => SubTotal * ((Sale.VatPercentage ?? 5) / 100m);
        protected decimal InvoiceDiscountAmount
        {
            get
            {
                if (Sale.InvoiceDiscountType == "Percentage" && Sale.InvoiceDiscount.HasValue)
                    return SubTotal * (Sale.InvoiceDiscount.Value / 100m);
                return Sale.InvoiceDiscount ?? 0;
            }
        }
        protected decimal MembershipDiscountAmount => SelectedCustomer?.DiscountRate.HasValue == true
            ? SubTotal * (SelectedCustomer.DiscountRate.Value / 100m)
            : 0;
        protected decimal CampaignDiscountAmount => Sale.CampaignDiscount ?? 0;
        protected decimal NetPayable
        {
            get
            {
                var net = SubTotal;
                if (Sale.InvoiceDiscountType == "Percentage" && Sale.InvoiceDiscount.HasValue)
                    net -= net * (Sale.InvoiceDiscount.Value / 100m);
                else
                    net -= Sale.InvoiceDiscount ?? 0;
                net -= CampaignDiscountAmount;
                net -= MembershipDiscountAmount;
                net -= Sale.ExchangeAmount ?? 0;
                net += CalculatedVat;
                net += Sale.RoundingAmount ?? 0;
                return Math.Max(net, 0);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Sale = _serviceUnitOfWork.SaleService.CreateNew();
            await LoadLookupData();
            Sale.StoreId = Stores.FirstOrDefault()?.Id;
            Sale.ShiftId = Shifts.FirstOrDefault()?.Id;
            Sale.InvoiceNo = await _serviceUnitOfWork.SaleService.GetNewInvoiceNo();
        }

        protected List<string> PaymentTypeOptions { get; set; } = new() { "Cash", "MFS", "Card" };
        protected List<string> DiscountTypeOptions { get; set; } = new() { "Percentage", "Flat" };

        protected async Task LoadLookupData()
        {
            var storeTask = _serviceUnitOfWork.StoreService.GetStores();
            var colorTask = _serviceUnitOfWork.ColorsService.GetColorss();
            var sizeTask = _serviceUnitOfWork.SizesService.GetSizess();
            var shiftTask = _serviceUnitOfWork.ShiftService.GetShift();

            await Task.WhenAll(storeTask, colorTask, sizeTask, shiftTask);

            Stores = (storeTask.Result ?? new List<StoreDTO>()).ToList();
            Colors = (colorTask.Result ?? new List<ColorsDTO>()).ToList();
            Sizes = (sizeTask.Result ?? new List<SizesDTO>()).ToList();
            Shifts = (shiftTask.Result ?? new List<ShiftDTO>()).ToList();
        }

        // Tab classes
        protected string SaleTabClass => $"mode-tab{(IsSaleMode ? " active" : "")}";
        protected string BookingTabClass => $"mode-tab{(IsBookingMode ? " active" : "")}";
        protected string InvoicesTabClass => $"mode-tab{(IsInvoicesMode ? " active" : "")}";
        protected string CartTabClass => $"tab-btn{(IsSaleMode ? " active" : "")}";
        protected string BookingListTabClass => $"tab-btn{(IsBookingMode ? " active" : "")}";
        protected string InvoiceListTabClass => $"tab-btn{(IsInvoicesMode ? " active" : "")}";

        protected void SwitchMode(string mode)
        {
            _currentMode = mode;
            if (mode == "Invoices")
                _ = LoadInvoices();
        }

        protected void SwitchToSale() => SwitchMode("Sale");
        protected void SwitchToBooking() => SwitchMode("Booking");
        protected void SwitchToInvoices() => SwitchMode("Invoices");
        protected void SetSaleMode() { _currentMode = "Sale"; }
        protected void SetBookingMode() { _currentMode = "Booking"; }
        protected void SetInvoicesMode() { _currentMode = "Invoices"; }

        protected async Task LoadInvoices()
        {
            // TODO: Load invoices list for today
        }

        // ── Barcode Scanning ──
        protected async Task OnBarcodeKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(BarcodeInput))
            {
                await AddItemByBarcode(BarcodeInput.Trim());
                BarcodeInput = "";
            }
        }

        protected async Task AddItemByBarcode(string barcode)
        {
            var product = await _serviceUnitOfWork.SaleService.SearchByBarcode(barcode);
            if (product == null || product.ItemId == 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Not Found",
                    $"No product found for barcode: {barcode}", 3000);
                return;
            }

            var existing = CartItems.FirstOrDefault(c =>
                c.ItemId == product.ItemId &&
                c.ColorId == product.ColorId &&
                c.SizeId == product.SizeId);

            if (existing != null)
            {
                existing.Quantity += (QtyInput > 0 ? QtyInput : 1);
                existing.TotalPrice = existing.Quantity * existing.SalePrice;
            }
            else
            {
                CartItems.Add(new SaleDetailDTO
                {
                    ItemId = product.ItemId,
                    ItemName = product.ItemName,
                    Barcode = product.Barcode,
                    Quantity = QtyInput > 0 ? QtyInput : 1,
                    SalePrice = product.SalePrice ?? 0,
                    TotalPrice = (QtyInput > 0 ? QtyInput : 1) * (product.SalePrice ?? 0),
                    ColorId = product.ColorId,
                    ColorName = product.ColorName,
                    SizeId = product.SizeId,
                    SizeName = product.SizeName,
                    StockQuantity = product.StockQuantity,
                    StoreId = Sale.StoreId
                });
            }

            QtyInput = 1;
            await CartGrid.Reload();
            StateHasChanged();
        }

        // ── Search Product Modal ──
        protected async Task OpenProductSearch()
        {
            var product = await dialogService.OpenAsync<ProductSearchDialogComponent>("Product Search",
                new Dictionary<string, object>());
            if (product is ProductSearchDTO selected)
            {
                var existing = CartItems.FirstOrDefault(c =>
                    c.ItemId == selected.ItemId &&
                    c.ColorId == selected.ColorId &&
                    c.SizeId == selected.SizeId);

                if (existing != null)
                {
                    existing.Quantity += 1;
                    existing.TotalPrice = existing.Quantity * existing.SalePrice;
                }
                else
                {
                    CartItems.Add(new SaleDetailDTO
                    {
                        ItemId = selected.ItemId,
                        ItemName = selected.ItemName,
                        Barcode = selected.Barcode,
                        Quantity = 1,
                        SalePrice = selected.SalePrice ?? 0,
                        TotalPrice = selected.SalePrice ?? 0,
                        ColorId = selected.ColorId,
                        ColorName = selected.ColorName,
                        SizeId = selected.SizeId,
                        SizeName = selected.SizeName,
                        StockQuantity = selected.StockQuantity,
                        StoreId = Sale.StoreId
                    });
                }

                await CartGrid.Reload();
                StateHasChanged();
            }
        }

        // ── Cart Operations ──
        protected void UpdateCartItemQuantity(SaleDetailDTO item, decimal newQty)
        {
            if (newQty <= 0)
            {
                CartItems.Remove(item);
            }
            else
            {
                item.Quantity = newQty;
                item.TotalPrice = item.Quantity * item.SalePrice;
            }
            CartGrid.Reload();
            StateHasChanged();
        }

        protected void RemoveCartItem(SaleDetailDTO item)
        {
            CartItems.Remove(item);
            CartGrid.Reload();
            StateHasChanged();
        }

        protected void ClearCart()
        {
            CartItems.Clear();
            SearchedProduct = null;
            SelectedCustomer = null;
            CustomerSearchText = "";
            Sale.InvoiceDiscount = null;
            Sale.CampaignDiscount = null;
            Sale.ExchangeAmount = null;
            Sale.VatPercentage = 5;
            CartGrid.Reload();
            StateHasChanged();
        }

        // ── Customer ──
        protected async Task OnCustomerSearch(ChangeEventArgs e)
        {
            CustomerSearchText = e.Value?.ToString() ?? "";
            if (CustomerSearchText.Length >= 2)
            {
                var all = await _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers();
                CustomerSuggestions = all.Where(c =>
                    (c.Name?.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(CustomerSearchText) ?? false))
                    .Take(10).ToList();
            }
            else
            {
                CustomerSuggestions.Clear();
            }
        }

        protected void SelectCustomer(CustomerDetailsDTO customer)
        {
            SelectedCustomer = customer;
            Sale.CustomerId = customer.Id;
            Sale.CustomerName = customer.Name;
            Sale.CustomerPhone = customer.Phone;
            Sale.CustomerAddress = customer.Address;
            Sale.MembershipTypeId = customer.MemberTypeId;
            Sale.DiscountRate = customer.DiscountRate;
            CustomerSearchText = customer.Name ?? customer.Phone ?? "";
            CustomerSuggestions.Clear();
            StateHasChanged();
        }

        protected void ClearCustomer()
        {
            SelectedCustomer = null;
            Sale.CustomerId = null;
            Sale.CustomerName = null;
            Sale.CustomerPhone = null;
            Sale.CustomerAddress = null;
            Sale.MembershipTypeId = null;
            Sale.DiscountRate = null;
            Sale.MembershipDiscount = null;
            CustomerSearchText = "";
            StateHasChanged();
        }

        // ── Add Customer Modal ──
        protected async Task OpenAddCustomerModal()
        {
            var customer = await dialogService.OpenAsync<AddCustomerDialogComponent>("New Customer",
                new Dictionary<string, object>());
            if (customer is CustomerDetailsDTO saved && saved.Id > 0)
            {
                notificationService.Notify(NotificationSeverity.Success, "Customer Saved",
                    "Customer created successfully", 3000);
                SelectCustomer(saved);
            }
        }

        // ── Exchange Modal ──
        protected async Task OpenExchangeModal()
        {
            var result = await dialogService.OpenAsync<ExchangeDialogComponent>("Return / Exchange",
                new Dictionary<string, object>());
            if (result is ExchangeResultDTO exchange)
            {
                Sale.ExchangeAmount = (Sale.ExchangeAmount ?? 0) + exchange.ExchangeAmount;
                Sale.ReturnInvoiceNo = exchange.InvoiceNo;
                Sale.IsReturnExchange = exchange.IsReturnExchange;
                notificationService.Notify(NotificationSeverity.Success, "Added",
                    $"Exchange amount: {exchange.ExchangeAmount:N2}", 2000);
            }
        }

        // ── Payment Modal ──
        protected async Task OpenPaymentModal()
        {
            var result = await dialogService.OpenAsync<PaymentDialogComponent>("Payment",
                new Dictionary<string, object> { { "NetPayable", NetPayable } });
            if (result is PaymentResultDTO paymentResult && paymentResult.Payments.Count > 0)
            {
                Sale.SubTotal = SubTotal;
                Sale.VatAmount = CalculatedVat;
                Sale.CampaignDiscount = CampaignDiscountAmount > 0 ? CampaignDiscountAmount : null;
                Sale.MembershipDiscount = MembershipDiscountAmount > 0 ? MembershipDiscountAmount : null;
                Sale.NetAmount = NetPayable;
                Sale.PaidAmount = paymentResult.Payments.Sum(p => p.Amount);
                Sale.DueAmount = Math.Max(0, Sale.NetAmount - (Sale.PaidAmount ?? 0));
                Sale.PaymentStatus = Sale.DueAmount <= 0 ? "Paid" :
                    (Sale.PaidAmount > 0 ? "Partial" : "Due");
                Sale.SaleDetails = CartItems.ToList();
                Sale.Payments = paymentResult.Payments.ToList();

                var saveResult = await _serviceUnitOfWork.SaleService.SaveSale(Sale);
                if (saveResult.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Sale Saved",
                        $"Invoice: {Sale.InvoiceNo}, Amount: {Sale.NetAmount:N2}", 5000);
                    await ResetForNewSale();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", saveResult.Message, 4000);
                }
            }
        }

        // ── Hold Draft ──
        protected async Task HoldAsDraft()
        {
            Sale.SubTotal = SubTotal;
            Sale.VatAmount = CalculatedVat;
            Sale.NetAmount = NetPayable;
            Sale.SaleDetails = CartItems.ToList();

            var result = await _serviceUnitOfWork.SaleService.SaveSale(Sale);
            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Draft Saved",
                    $"Draft saved as Invoice: {Sale.InvoiceNo}", 4000);
                await ResetForNewSale();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message, 4000);
            }
        }

        protected async Task ResetForNewSale()
        {
            CartItems.Clear();
            SelectedCustomer = null;
            CustomerSearchText = "";
            SearchedProduct = null;
            Sale = _serviceUnitOfWork.SaleService.CreateNew();
            Sale.StoreId = Stores.FirstOrDefault()?.Id;
            Sale.ShiftId = Shifts.FirstOrDefault()?.Id;
            Sale.InvoiceNo = await _serviceUnitOfWork.SaleService.GetNewInvoiceNo();
            StateHasChanged();
        }
    }
}
