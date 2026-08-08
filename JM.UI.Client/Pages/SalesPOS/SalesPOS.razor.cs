using JM.Infrastructure.Models;
using JM.UI.Client.Pages.Dialog;
using JM.UI.Client.Pages.Dialog.SalesPOS;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Model.Employees;
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
    public partial class SalesPOSComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        // ── Mode ──
        protected string _currentMode = "Sale";
        protected bool IsSaleMode => _currentMode == "Sale";
        protected bool IsBookingMode => _currentMode == "Booking";
        protected bool IsInvoicesMode => _currentMode == "Invoices";
        protected bool IsDraftMode => _currentMode == "Draft";
        protected string PageTitle => $"Sales POS - {_currentMode}";

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
        protected ProductSearchDTO? SearchedProduct { get; set; }

        // ── Invoices / Bookings / Drafts ──
        protected List<SaleSummaryDTO> Invoices { get; set; } = new();
        protected bool IsInvoicesLoading { get; set; }
        protected List<SaleSummaryDTO> Bookings { get; set; } = new();
        protected bool IsBookingsLoading { get; set; }
        protected List<SaleSummaryDTO> Drafts { get; set; } = new();
        protected bool IsDraftsLoading { get; set; }
        protected Dictionary<int, List<SaleDetailDTO>> ExpandedInvoiceDetails { get; set; } = new();
        protected Dictionary<int, bool> ExpandedInvoiceLoading { get; set; } = new();

        protected List<SaleSummaryDTO> CurrentSalesList =>
            IsDraftMode ? Drafts : (IsBookingMode ? Bookings : Invoices);
        protected bool IsCurrentSalesListLoading =>
            IsDraftMode ? IsDraftsLoading : (IsBookingMode ? IsBookingsLoading : IsInvoicesLoading);

        // ── Customer ──
        protected List<CustomerDetailsDTO> AllCustomers { get; set; } = new();
        protected int SelectedCustomerId { get; set; }
        protected CustomerDetailsDTO? SelectedCustomer { get; set; }

        // ── Employee ──
        protected List<EmployeeModelDTO> Employees { get; set; } = new();
        protected int SelectedEmployeeId { get; set; }
        protected string? SelectedEmployeeName { get; set; }

        // ── Computed Values ──
        protected decimal SubTotal => CartItems.Sum(c => c.TotalAmount);
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
        protected decimal MembershipDiscountAmount => SelectedCustomer != null
            ? SubTotal * (GetCustomerDiscountRate(SelectedCustomer) / 100m)
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
            NavigationGuard.IsGuardActive = true;
            await TokenService.InitializeTokenAsync();
            Sale = _serviceUnitOfWork.SaleService.CreateNew();
            await LoadLookupData();
            int localStoreId = await GetLocalStorageInt("StoreId");
            Sale.StoreId = localStoreId > 0 ? localStoreId : Stores.FirstOrDefault()?.Id;
            Sale.CreatedBy = await GetLocalStorageInt("UserId");
            Sale.InvoiceNo = await _serviceUnitOfWork.SaleService.GetNewInvoiceNo();
        }
        private async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var result = await _localStorage.GetAsync<string>(key);
                if (result.Success && !string.IsNullOrEmpty(result.Value))
                {
                    if (int.TryParse(result.Value, out int parsed) && parsed > 0)
                        return parsed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] GetLocalStorageInt('{key}') failed: {ex.Message}");
            }

            return 0;
        }
        protected List<string> PaymentTypeOptions { get; set; } = new() { "Cash", "MFS", "Card" };
        protected List<string> DiscountTypeOptions { get; set; } = new() { "Percentage", "Flat" };

        protected async Task LoadLookupData()
        {
            try
            {
                var storeTask = _serviceUnitOfWork.StoreService.GetStores();
                var colorTask = _serviceUnitOfWork.ColorsService.GetColorss();
                var sizeTask = _serviceUnitOfWork.SizesService.GetSizess();
                var shiftTask = _serviceUnitOfWork.ShiftService.GetShift();
                var allCustomers = _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers();
                var employeeTask = _serviceUnitOfWork.EmployeeService.GetEmployees();
                var membershipTask = _serviceUnitOfWork.MembershipTypeService.GetAll();
                await Task.WhenAll(storeTask, colorTask, sizeTask, shiftTask, allCustomers, employeeTask, membershipTask);

                Stores = (storeTask.Result ?? new List<StoreDTO>()).ToList();
                Colors = (colorTask.Result ?? new List<ColorsDTO>()).ToList();
                Sizes = (sizeTask.Result ?? new List<SizesDTO>()).ToList();
                Shifts = (shiftTask.Result ?? new List<ShiftDTO>()).ToList();
                AllCustomers = (allCustomers.Result ?? new List<CustomerDetailsDTO>()).ToList();
                Employees = (employeeTask.Result ?? new List<EmployeeModelDTO>()).ToList();
                MembershipTypes = (membershipTask.Result ?? new List<MembershipTypeDTO>()).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Failed to load lookup data: {ex.Message}", 4000);
            }
        }

        // Tab classes
        protected string SaleTabClass => $"mode-tab{(IsSaleMode ? " active" : "")}";
        protected string BookingTabClass => $"mode-tab{(IsBookingMode ? " active" : "")}";
        protected string InvoicesTabClass => $"mode-tab{(IsInvoicesMode ? " active" : "")}";
        protected string DraftTabClass => $"mode-tab{(IsDraftMode ? " active" : "")}";
        protected string CartTabClass => $"tab-btn{(IsSaleMode ? " active" : "")}";
        protected string BookingListTabClass => $"tab-btn{(IsBookingMode ? " active" : "")}";
        protected string InvoiceListTabClass => $"tab-btn{(IsInvoicesMode ? " active" : "")}";
        protected string DraftListTabClass => $"tab-btn{(IsDraftMode ? " active" : "")}";

        protected async Task SwitchMode(string mode)
        {
            _currentMode = mode;
            switch (mode)
            {
                case "Invoices":
                    await LoadInvoices();
                    break;
                case "Booking":
                    await LoadBookings();
                    break;
                case "Draft":
                    await LoadDrafts();
                    break;
            }
        }

        protected Task SwitchToSale() => SwitchMode("Sale");
        protected Task SwitchToBooking() => SwitchMode("Booking");
        protected Task SwitchToInvoices() => SwitchMode("Invoices");
        protected Task SwitchToDraft() => SwitchMode("Draft");
        protected void SetSaleMode() { _currentMode = "Sale"; }
        protected async Task SetBookingMode()
        {
            if (_currentMode != "Booking")
            {
                _currentMode = "Booking";
                await LoadBookings();
            }
            else
            {
                _currentMode = "Booking";
            }
        }
        protected async Task SetInvoicesMode()
        {
            if (_currentMode != "Invoices")
            {
                _currentMode = "Invoices";
                await LoadInvoices();
            }
            else
            {
                _currentMode = "Invoices";
            }
        }
        protected async Task SetDraftMode()
        {
            if (_currentMode != "Draft")
            {
                _currentMode = "Draft";
                await LoadDrafts();
            }
            else
            {
                _currentMode = "Draft";
            }
        }

        protected async Task LoadInvoices()
        {
            try
            {
                IsInvoicesLoading = true;
                var all = await _serviceUnitOfWork.SaleService.GetAllSales();
                Invoices = (all ?? new List<SaleSummaryDTO>()).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading invoices: {ex.Message}", 4000);
            }
            finally
            {
                IsInvoicesLoading = false;
            }
        }

        protected async Task LoadBookings()
        {
            try
            {
                IsBookingsLoading = true;
                var all = await _serviceUnitOfWork.SaleService.GetBookingSales();
                Bookings = (all ?? new List<SaleSummaryDTO>()).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading bookings: {ex.Message}", 4000);
            }
            finally
            {
                IsBookingsLoading = false;
            }
        }

        protected async Task LoadDrafts()
        {
            try
            {
                IsDraftsLoading = true;
                var all = await _serviceUnitOfWork.SaleService.GetDraftSales();
                Drafts = (all ?? new List<SaleSummaryDTO>()).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading drafts: {ex.Message}", 4000);
            }
            finally
            {
                IsDraftsLoading = false;
            }
        }

        protected async Task OnInvoiceRowExpand(SaleSummaryDTO invoice)
        {
            if (invoice == null) return;
            if (ExpandedInvoiceDetails.ContainsKey(invoice.SaleMasterId)) return;

            ExpandedInvoiceLoading[invoice.SaleMasterId] = true;
            try
            {
                var sale = await _serviceUnitOfWork.SaleService.GetSaleById(invoice.SaleMasterId);
                ExpandedInvoiceDetails[invoice.SaleMasterId] = sale?.SaleDetails ?? new List<SaleDetailDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading invoice details: {ex.Message}", 4000);
            }
            finally
            {
                ExpandedInvoiceLoading[invoice.SaleMasterId] = false;
            }
        }

        protected async Task EditInvoice(SaleSummaryDTO invoice)
        {
            if (invoice == null) return;
            try
            {
                bool editingFromDraft = IsDraftMode;

                var sale = await _serviceUnitOfWork.SaleService.GetSaleById(invoice.SaleMasterId);
                if (sale == null)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Not Found",
                        $"Invoice {invoice.InvoiceNo} not found", 3000);
                    return;
                }

                _currentMode = "Sale";
                CartItems.Clear();
                if (sale.SaleDetails != null)
                {
                    foreach (var d in sale.SaleDetails)
                    {
                        // The price already on the invoice is the floor the user may edit up from.
                        if (d.BaseUnitPrice == 0)
                            d.BaseUnitPrice = d.UnitPrice;
                    }
                    CartItems.AddRange(sale.SaleDetails);
                }

                Sale = sale;
                SelectedCustomer = null;
                SelectedCustomerId = 0;

                var customer = AllCustomers.FirstOrDefault(c => c.Id == sale.CustomerId);
                if (customer != null) SelectCustomer(customer);

                // When editing from the Draft list, the draft is now converted to a sale.
                if (editingFromDraft)
                {
                    var unmarkResult = await _serviceUnitOfWork.SaleService.UnmarkDraftSale(sale.SaleMasterId);
                    if (unmarkResult.IsSuccessStatus)
                    {
                        sale.IsDraft = false;
                    }
                    else
                    {
                        notificationService.Notify(NotificationSeverity.Warning, "Draft Status",
                            $"Could not unmark draft: {unmarkResult.Message}", 4000);
                    }
                }

                notificationService.Notify(NotificationSeverity.Info, "Editing",
                    $"Invoice {sale.InvoiceNo} loaded for editing", 3000);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading invoice: {ex.Message}", 4000);
            }
        }

        // ── Barcode Scanning ──
        protected async Task OnBarcodeKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(BarcodeInput))
            {
                await AddItemByBarcode(BarcodeInput.Trim());
            }
        }

        protected async Task AddItemByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Scan Barcode",
                    "Please scan or type a barcode first.", 3000);
                return;
            }

            if (SelectedEmployeeId <= 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Salesman Required",
                    "Please select a salesman before adding items.", 3000);
                return;
            }

            try
            {
                if (Sale.StoreId == null || Sale.StoreId <= 0)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Store Required",
                        "No store is assigned to your profile. Please contact an administrator.", 4000);
                    return;
                }

                var product = await _serviceUnitOfWork.SaleService.SearchByBarcode(barcode, Sale.StoreId);
                if (product == null || product.ItemId == 0)
                {
                    var storeName = Stores.FirstOrDefault(s => s.Id == Sale.StoreId)?.Name ?? "current store";
                    var confirmed = await dialogService.Confirm(
                        $"Barcode '{barcode}' not found in {storeName}. Do you want to search in other stores?",
                        "Not Found",
                        new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

                    if (confirmed == true)
                    {
                        await OpenProductSearchWithTerm(barcode);
                    }
                    return;
                }

                SearchedProduct = product;
                await PromptQuantityAndAdd(product);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Scan Failed",
                    $"Error scanning barcode: {ex.Message}", 4000);
            }
            finally
            {
                BarcodeInput = "";
                StateHasChanged();
            }
        }

        // Opens a popup showing the item name, stock quantity and a quantity input
        // (defaults to 1), then adds the chosen quantity to the cart.
        protected async Task PromptQuantityAndAdd(ProductSearchDTO product)
        {
            var qty = await dialogService.OpenAsync<ItemQuantityDialog>("Item Quantity",
                new Dictionary<string, object> { { "Product", product } },
                new DialogOptions { Width = "420px" });

            if (qty is decimal selectedQty && selectedQty > 0)
            {
                AddProductToCart(product, selectedQty);
                if (CartGrid != null)
                    await CartGrid.Reload();
                StateHasChanged();
            }
        }

        protected void AddProductToCart(ProductSearchDTO product, decimal qty)
        {
            var existing = CartItems.FirstOrDefault(c => c.ItemId == product.ItemId);

            if (existing != null)
            {
                existing.Qty += qty;
                existing.TotalAmount = existing.Qty * existing.UnitPrice;
            }
            else
            {
                var detail = SaleDetailDTO.FromProductSearch(product, qty);
                if (detail.StoreId == null)
                    detail.StoreId = Sale.StoreId;
                if (SelectedEmployeeId > 0)
                {
                    detail.SalesPersonId = SelectedEmployeeId;
                    detail.SalesPersonName = SelectedEmployeeName;
                }
                CartItems.Add(detail);
            }

            DistributeCustomerDiscount();
        }

        // ── Search Product Modal ──
        protected async Task OpenProductSearch()
        {
            var product = await dialogService.OpenAsync<ProductSearchDialog>("Product Search",
                new Dictionary<string, object>(),
                new DialogOptions { Width = "1000px", Height = "700px" });
            if (product is ProductSearchDTO selected && selected.ItemId > 0)
            {
                await PromptQuantityAndAdd(selected);
            }
        }

        protected async Task OpenProductSearchWithTerm(string term)
        {
            var product = await dialogService.OpenAsync<ProductSearchDialog>("Product Search",
                new Dictionary<string, object> { { "Barcode", term } },
                new DialogOptions { Width = "700px", Height = "550px" });
            if (product is ProductSearchDTO selected && selected.ItemId > 0)
            {
                await PromptQuantityAndAdd(selected);
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
                item.Qty = newQty;
                item.TotalAmount = item.Qty * item.UnitPrice;
            }
            DistributeCustomerDiscount();
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        // The sale price may be raised in the cart, but never reduced below the
        // originally loaded price (BaseUnitPrice).
        protected void UpdateCartItemPrice(SaleDetailDTO item, decimal newPrice)
        {
            if (newPrice < item.BaseUnitPrice)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Price Restriction",
                    $"Sale price cannot be set below the loaded price ({item.BaseUnitPrice:N2}).", 3000);
                item.UnitPrice = item.BaseUnitPrice;
            }
            else
            {
                item.UnitPrice = newPrice;
            }
            item.TotalAmount = item.Qty * item.UnitPrice;
            DistributeCustomerDiscount();
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        protected void RemoveCartItem(SaleDetailDTO item)
        {
            CartItems.Remove(item);
            DistributeCustomerDiscount();
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        protected void ClearCart()
        {
            CartItems.Clear();
            SearchedProduct = null;
            SelectedCustomer = null;
            SelectedCustomerId = 0;
            SelectedEmployeeId = 0;
            SelectedEmployeeName = null;
            Sale.InvoiceDiscount = null;
            Sale.CampaignDiscount = null;
            Sale.ExchangeAmount = null;
            Sale.VatPercentage = 5;
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        // ── Discount Distribution ──
        protected void DistributeInvoiceDiscount()
        {
            if (!CartItems.Any()) return;

            // Only distribute across items that don't already have a product-level discount.
            var eligible = CartItems.Where(i => !i.HasDiscount).ToList();
            if (!eligible.Any()) return;

            decimal totalDiscountAmount = Sale.InvoiceDiscountType == "Percentage" && Sale.InvoiceDiscount.HasValue
                ? SubTotal * (Sale.InvoiceDiscount.Value / 100m)
                : Sale.InvoiceDiscount ?? 0;

            if (totalDiscountAmount <= 0)
            {
                foreach (var item in eligible) item.Discount = 0;
                if (CartGrid != null)
                    CartGrid.Reload();
                StateHasChanged();
                return;
            }

            // Divide the discount equally across eligible items.
            var share = Math.Floor(totalDiscountAmount / eligible.Count * 100m) / 100m;
            var remaining = totalDiscountAmount;

            for (int i = 0; i < eligible.Count; i++)
            {
                var item = eligible[i];
                if (i == eligible.Count - 1)
                {
                    item.Discount = Math.Round(remaining, 2);
                }
                else
                {
                    item.Discount = share;
                    remaining -= share;
                }
            }

            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        protected async Task LoadCustomers()
        {
            try
            {
                var all = await _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers();
                AllCustomers = all.ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Load Failed",
                    $"Error loading customers: {ex.Message}", 4000);
            }
        }

        protected void SelectCustomerById(int customerId)
        {
            if (customerId <= 0)
            {
                ClearCustomer();
                return;
            }
            var customer = AllCustomers.FirstOrDefault(c => c.Id == customerId);
            if (customer != null) SelectCustomer(customer);
        }

        protected void SelectCustomer(CustomerDetailsDTO customer)
        {
            SelectedCustomer = customer;
            SelectedCustomerId = customer.Id;
            Sale.CustomerId = customer.Id;
            Sale.CustomerName = customer.Name;
            Sale.CustomerPhone = customer.Phone;
            Sale.CustomerAddress = customer.Address;
            Sale.MembershipTypeId = customer.MemberTypeId;
            var discountRate = GetCustomerDiscountRate(customer);
            Sale.DiscountRate = (int)discountRate;
            Sale.MembershipDiscount = SubTotal * (discountRate / 100m);
            DistributeCustomerDiscount();
            StateHasChanged();
        }

        protected void ClearCustomer()
        {
            SelectedCustomer = null;
            SelectedCustomerId = 0;
            Sale.CustomerId = null;
            Sale.CustomerName = null;
            Sale.CustomerPhone = null;
            Sale.CustomerAddress = null;
            Sale.MembershipTypeId = null;
            Sale.DiscountRate = null;
            Sale.MembershipDiscount = null;

            // Remove the customer-discount shares from cart items.
            foreach (var item in CartItems.Where(i => !i.HasDiscount))
            {
                item.Discount = 0;
            }
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        // ── Customer / Membership Discount Distribution ──
        // The customer's discount is equally divided across cart items that do not
        // already have their own (product-level) discount.
        // Resolves the discount rate from the customer's DTO, falling back to the
        // MembershipType table (looked up by MemberTypeId) when the DTO value is missing.
        protected decimal GetCustomerDiscountRate(CustomerDetailsDTO customer)
        {
            if (customer?.DiscountRate.HasValue == true && customer.DiscountRate.Value > 0)
                return customer.DiscountRate.Value;

            if (customer?.MemberTypeId.HasValue == true)
            {
                var membership = MembershipTypes.FirstOrDefault(m => m.Id == customer.MemberTypeId.Value);
                if (membership?.DiscountRate.HasValue == true && membership.DiscountRate.Value > 0)
                    return membership.DiscountRate.Value;
            }
            return 0;
        }

        protected void DistributeCustomerDiscount()
        {
            if (!CartItems.Any() || SelectedCustomer == null)
            {
                return;
            }

            var discountRate = GetCustomerDiscountRate(SelectedCustomer);
            if (discountRate <= 0)
            {
                return;
            }

            // Only distribute across items that don't already have a product-level discount.
            var eligible = CartItems.Where(i => !i.HasDiscount).ToList();
            if (!eligible.Any()) return;

            decimal totalDiscountAmount = SubTotal * (discountRate / 100m);
            if (totalDiscountAmount <= 0)
            {
                foreach (var item in eligible) item.Discount = 0;
                if (CartGrid != null)
                    CartGrid.Reload();
                StateHasChanged();
                return;
            }

            // Divide the discount equally across eligible items.
            var share = Math.Floor(totalDiscountAmount / eligible.Count * 100m) / 100m;
            var remaining = totalDiscountAmount;

            for (int i = 0; i < eligible.Count; i++)
            {
                var item = eligible[i];
                if (i == eligible.Count - 1)
                {
                    item.Discount = Math.Round(remaining, 2);
                }
                else
                {
                    item.Discount = share;
                    remaining -= share;
                }
            }

            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        // ── Employee Selection ──
        protected void OnEmployeeChanged(int employeeId)
        {
            SelectedEmployeeId = employeeId;
            var emp = Employees.FirstOrDefault(e => e.Id == employeeId);
            SelectedEmployeeName = emp?.Name;

            foreach (var item in CartItems)
            {
                item.SalesPersonId = employeeId > 0 ? employeeId : null;
                item.SalesPersonName = SelectedEmployeeName;
            }
            if (CartGrid != null)
                CartGrid.Reload();
            StateHasChanged();
        }

        // ── Add Customer Modal ──
        protected async Task OpenAddCustomerModal()
        {
            var customer = await dialogService.OpenAsync<AddCustomerDialog>("New Customer",
                new Dictionary<string, object>());
            if (customer is CustomerDetailsDTO saved && saved.Id > 0)
            {
                notificationService.Notify(NotificationSeverity.Success, "Customer Saved",
                    "Customer created successfully", 3000);
                if (AllCustomers.All(c => c.Id != saved.Id))
                    AllCustomers.Add(saved);
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
            var result = await dialogService.OpenAsync<PaymentDialog>("Payment",
                new Dictionary<string, object> { { "NetPayable", NetPayable } });
            if (result is PaymentResultDTO paymentResult && paymentResult.Payments.Count > 0)
            {
                Sale.SubTotal = SubTotal;
                Sale.VatAmount = CalculatedVat;
                Sale.CampaignDiscount = CampaignDiscountAmount > 0 ? CampaignDiscountAmount : null;
                Sale.MembershipDiscount = MembershipDiscountAmount > 0 ? MembershipDiscountAmount : null;
                Sale.NetAmount = NetPayable;
                Sale.PaidAmount = paymentResult.Payments.Sum(p => p.PaidAmount ?? 0);
                Sale.DueAmount = Math.Max(0, Sale.NetAmount - (Sale.PaidAmount ?? 0));
                Sale.PaymentStatus = Sale.DueAmount <= 0 ? "Paid" :
                    (Sale.PaidAmount > 0 ? "Partial" : "Due");

                // ── Validation: a due requires a customer ──
                if (Sale.DueAmount > 0 && SelectedCustomer == null)
                {
                    notificationService.Notify(NotificationSeverity.Warning, "Customer Required",
                        "Customer is mandatory when the invoice has a due. Please select a customer first.", 4500);
                    return;
                }

                Sale.SaleDetails = CartItems.ToList();
                Sale.PaymentTransactions = paymentResult.Payments.ToList();

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

        // ── Booking Due Payment Modal ──
        protected async Task OpenBookingPayment(SaleSummaryDTO booking)
        {
            if (booking == null) return;

            try
            {
                var sale = await _serviceUnitOfWork.SaleService.GetSaleById(booking.SaleMasterId);
                decimal due = sale?.TotalDue ?? booking.TotalDue;
                if (due <= 0)
                {
                    notificationService.Notify(NotificationSeverity.Info, "No Due",
                        $"Invoice {booking.InvoiceNo} has no pending due", 3000);
                    return;
                }

                var result = await dialogService.OpenAsync<PaymentDialog>("Payment",
                    new Dictionary<string, object>
                    {
                        { "NetPayable", due },
                        { "AllowBookingOption", false }
                    });

                if (result is PaymentResultDTO paymentResult && paymentResult.Payments.Count > 0)
                {
                    int storeId = Sale.StoreId ?? 0;
                    int userId = await GetLocalStorageInt("UserId");

                    var saveResult = await _serviceUnitOfWork.SaleService.SaveDuePayment(
                        booking.SaleMasterId, storeId, paymentResult.Payments.ToList(), userId);

                    if (saveResult.IsSuccessStatus)
                    {
                        notificationService.Notify(NotificationSeverity.Success, "Payment Saved",
                            saveResult.Message, 4000);
                        ExpandedInvoiceDetails.Remove(booking.SaleMasterId);
                        await LoadBookings();
                        StateHasChanged();
                    }
                    else
                    {
                        notificationService.Notify(NotificationSeverity.Error, "Error",
                            saveResult.Message, 4000);
                    }
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Payment Failed",
                    $"Error processing payment: {ex.Message}", 4000);
            }
        }

        // ── Cancel Booking ──
        protected async Task CancelBooking(SaleSummaryDTO booking)
        {
            if (booking == null) return;

            var confirmed = await dialogService.Confirm(
                $"Cancel booking {booking.InvoiceNo}? This will revert all stock, payments and requisitions created for this booking.",
                "Cancel Booking",
                new ConfirmOptions { OkButtonText = "Yes, Cancel", CancelButtonText = "No" });
            if (confirmed != true) return;

            try
            {
                int storeId = Sale.StoreId ?? 0;
                int userId = await GetLocalStorageInt("UserId");

                var result = await _serviceUnitOfWork.SaleService.CancelBooking(
                    booking.SaleMasterId, storeId, userId);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Booking Cancelled",
                        result.Message, 4000);
                    ExpandedInvoiceDetails.Remove(booking.SaleMasterId);
                    await LoadBookings();
                    StateHasChanged();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error",
                        result.Message, 4000);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Cancel Failed",
                    $"Error cancelling booking: {ex.Message}", 4000);
            }
        }

        // ── Hold Draft ──
        protected async Task HoldAsDraft()
        {
            try
            {
                Sale.SubTotal = SubTotal;
                Sale.VatAmount = CalculatedVat;
                Sale.NetAmount = NetPayable;
                Sale.SaleDetails = CartItems.ToList();
                Sale.IsDraft = true;

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
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Draft Failed",
                    $"Error saving draft: {ex.Message}", 4000);
            }
        }

        protected async Task ResetForNewSale()
        {
            try
            {
                int? lastStoreId = Sale.StoreId;
                int? lastShiftId = Sale.ShiftId;

                CartItems.Clear();
                SelectedCustomer = null;
                SelectedCustomerId = 0;
                SelectedEmployeeId = 0;
                SelectedEmployeeName = null;
                SearchedProduct = null;
                Sale = _serviceUnitOfWork.SaleService.CreateNew();
                Sale.StoreId = lastStoreId > 0 ? lastStoreId : Stores.FirstOrDefault()?.Id;
                Sale.ShiftId = lastShiftId > 0 ? lastShiftId : Shifts.FirstOrDefault()?.Id;
                Sale.CreatedBy = await GetLocalStorageInt("UserId");
                Sale.InvoiceNo = await _serviceUnitOfWork.SaleService.GetNewInvoiceNo();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Reset Failed",
                    $"Error resetting sale: {ex.Message}", 4000);
            }
            StateHasChanged();
        }

        public void Dispose()
        {
            CartGrid?.Dispose();
            NavigationGuard.IsGuardActive = false;
        }
    }
}
