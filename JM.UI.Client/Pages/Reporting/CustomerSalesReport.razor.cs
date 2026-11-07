using JM.UI.Client.Pages.Dialog.Reporting;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemBrand;
using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

/// <summary>
/// Customer-wise sales report: Radzen filter bar, customer summary grid, and a
/// per-customer drill-down with an itemized purchase grid plus a Radzen receipt
/// dialog. All figures are derived from real SalesMaster/SalesDetails rows
/// returned by the CustomerWiseSalesDetail API endpoint.
/// </summary>
public partial class CustomerSalesReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public CustomerSalesReportPdfService CustomerSalesReportPdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    // ── Raw data ──────────────────────────────────────────────────────
    protected List<CustomerWiseSalesDetailDTO> Rows { get; set; } = new();
    protected List<CustomerSalesSummaryRow> Summaries { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    // ── Filter state ──────────────────────────────────────────────────
    protected string? SearchText { get; set; }
    protected string? FilterBarcode { get; set; }
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected decimal? MinAmount { get; set; }
    protected decimal? MaxAmount { get; set; }
    protected int? SelectedStoreId { get; set; }
    protected int? SelectedGroupId { get; set; }
    protected int? SelectedSubGroupId { get; set; }
    protected int? SelectedDesignId { get; set; }
    protected int? SelectedBrandId { get; set; }

    // ── Lookups ───────────────────────────────────────────────────────
    protected List<StoreDTO> Stores { get; set; } = new();
    protected List<GroupModelDTO> Groups { get; set; } = new();
    protected List<SubGroupModelDTO> AllSubGroups { get; set; } = new();
    protected List<DesignModelDTO> AllDesigns { get; set; } = new();
    protected List<ItemBrandDTO> Brands { get; set; } = new();
    protected IEnumerable<SubGroupModelDTO> FilteredSubGroups { get; set; } = new List<SubGroupModelDTO>();
    protected IEnumerable<DesignModelDTO> FilteredDesigns { get; set; } = new List<DesignModelDTO>();

    // ── Drill-down state ──────────────────────────────────────────────
    protected CustomerSalesSummaryRow? SelectedCustomer { get; set; }
    protected List<CustomerSalesInvoiceGroup> CustomerInvoices { get; set; } = new();
    protected List<CustomerWiseSalesDetailDTO> CustomerLines { get; set; } = new();

    protected bool IsDetailView => SelectedCustomer != null;

    // ──────────────────────────────────────────────────────────────────
    //  Lifecycle / loading
    // ──────────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadLookups();
        await LoadReport();
    }

    private async Task LoadLookups()
    {
        try { Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new(); }
        catch (Exception ex) { Console.WriteLine($"[CustomerSalesReport] Stores: {ex.Message}"); }

        try { Groups = (await _serviceUnitOfWork.GroupService.GetGroups())?.ToList() ?? new(); }
        catch (Exception ex) { Console.WriteLine($"[CustomerSalesReport] Groups: {ex.Message}"); }

        try { AllSubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups())?.ToList() ?? new(); }
        catch (Exception ex) { Console.WriteLine($"[CustomerSalesReport] SubGroups: {ex.Message}"); }

        try { AllDesigns = (await _serviceUnitOfWork.DesignService.GetDesigns())?.ToList() ?? new(); }
        catch (Exception ex) { Console.WriteLine($"[CustomerSalesReport] Designs: {ex.Message}"); }

        try { Brands = (await _serviceUnitOfWork.ItemBrandService.GetItemBrands())?.ToList() ?? new(); }
        catch (Exception ex) { Console.WriteLine($"[CustomerSalesReport] Brands: {ex.Message}"); }

        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetCustomerWiseSalesDetail(
                        SelectedStoreId,
                        null,
                        FromDate,
                        ToDate,
                        SearchText,
                        SelectedGroupId,
                        SelectedSubGroupId,
                        SelectedDesignId,
                        SelectedBrandId,
                        FilterBarcode,
                        MinAmount,
                        MaxAmount))?.ToList() ?? new();

            BuildSummaries();

            // Refresh the open drill-down so it reflects the same filters.
            if (SelectedCustomer != null)
            {
                var refreshed = Summaries.FirstOrDefault(s => s.CustomerId == SelectedCustomer.CustomerId);
                if (refreshed == null)
                    CloseDetail();
                else
                    OpenCustomer(refreshed);
            }
        }
        catch (Exception ex)
        {
            Rows = new();
            Summaries = new();
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load report: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private void BuildSummaries()
    {
        Summaries = Rows
            .GroupBy(r => r.CustomerId)
            .Select(g =>
            {
                var invoices = g.GroupBy(r => r.SaleMasterId).Select(x => x.First()).ToList();

                return new CustomerSalesSummaryRow
                {
                    CustomerId = g.Key,
                    CustomerCode = g.Select(r => r.CustomerCode).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty,
                    CustomerName = g.Select(r => r.CustomerName).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty,
                    CustomerPhone = g.Select(r => r.CustomerPhone).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty,
                    MembershipType = g.Select(r => r.CustomerStatus).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty,
                    InvoiceCount = invoices.Count,
                    TotalPurchase = invoices.Sum(i => i.InvoiceNet),
                    LastPurchaseDate = invoices.Count > 0 ? invoices.Max(i => i.SaleDate) : null,
                    PrimaryBranch = g.Where(r => !string.IsNullOrWhiteSpace(r.StoreName))
                                     .GroupBy(r => r.StoreName)
                                     .OrderByDescending(x => x.Count())
                                     .Select(x => x.Key)
                                     .FirstOrDefault() ?? string.Empty
                };
            })
            .ToList();
    }

    // ──────────────────────────────────────────────────────────────────
    //  Summary totals
    // ──────────────────────────────────────────────────────────────────

    protected decimal GrandTotal => Summaries.Sum(s => s.TotalPurchase);
    protected int TotalInvoiceCount => Summaries.Sum(s => s.InvoiceCount);
    protected int CustomerCount => Summaries.Count;

    // ──────────────────────────────────────────────────────────────────
    //  Filter handlers
    // ──────────────────────────────────────────────────────────────────

    protected void OnGroupChanged(int? groupId)
    {
        SelectedGroupId = groupId;
        SelectedSubGroupId = null;
        SelectedDesignId = null;

        FilteredSubGroups = groupId.HasValue
            ? AllSubGroups.Where(sg => sg.GroupId == groupId.Value).ToList()
            : AllSubGroups;

        FilteredDesigns = AllDesigns;
    }

    protected void OnSubGroupChanged(int? subGroupId)
    {
        SelectedSubGroupId = subGroupId;
        SelectedDesignId = null;

        FilteredDesigns = subGroupId.HasValue
            ? AllDesigns.Where(d => d.SubGroupId == subGroupId.Value).ToList()
            : AllDesigns;
    }

    protected void OnDesignChanged(int? designId) => SelectedDesignId = designId;

    protected void OnBrandChanged(int? brandId) => SelectedBrandId = brandId;

    protected void OnStoreChanged(int? storeId) => SelectedStoreId = storeId;

    protected async Task OnSearchKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await LoadReport();
    }

    /// <summary>Clears every filter and reloads the unfiltered report.</summary>
    protected async Task ResetFilters()
    {
        SearchText = null;
        FilterBarcode = null;
        FromDate = null;
        ToDate = null;
        MinAmount = null;
        MaxAmount = null;
        SelectedStoreId = null;
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedBrandId = null;

        FilteredSubGroups = AllSubGroups;
        FilteredDesigns = AllDesigns;

        await LoadReport();
    }

    // ──────────────────────────────────────────────────────────────────
    //  Drill-down
    // ──────────────────────────────────────────────────────────────────

    protected void OpenCustomer(CustomerSalesSummaryRow row)
    {
        SelectedCustomer = row;
        CustomerLines = Rows.Where(r => r.CustomerId == row.CustomerId).ToList();

        CustomerInvoices = CustomerLines
            .GroupBy(r => r.SaleMasterId)
            .Select(g =>
            {
                var first = g.First();
                return new CustomerSalesInvoiceGroup
                {
                    SaleMasterId = first.SaleMasterId,
                    InvoiceNo = first.InvoiceNo,
                    SaleDate = first.SaleDate,
                    StoreName = first.StoreName,
                    IsExchange = first.IsExchange,
                    IsReturn = first.IsReturn,
                    ReturnInvoiceNo = first.ReturnInvoiceNo,
                    TotalDiscount = first.TotalDiscount,
                    InvoiceNet = first.InvoiceNet,
                    NetPaid = first.NetPaid,
                    Due = first.Due,
                    Lines = g.OrderBy(l => l.SalesDetailsId).ToList()
                };
            })
            .OrderByDescending(i => i.SaleDate)
            .ToList();
    }

    protected void CloseDetail()
    {
        SelectedCustomer = null;
        CustomerInvoices = new();
        CustomerLines = new();
    }

    // Per-line status comes from its invoice (the invoice, not the line, carries
    // the exchange/return markers).
    protected CustomerSalesInvoiceGroup? InvoiceOf(CustomerWiseSalesDetailDTO line)
        => CustomerInvoices.FirstOrDefault(i => i.SaleMasterId == line.SaleMasterId);

    protected string LineStatus(CustomerWiseSalesDetailDTO line)
        => InvoiceOf(line)?.StatusText ?? "Regular Purchase";

    protected bool LineIsSpecial(CustomerWiseSalesDetailDTO line)
        => InvoiceOf(line)?.IsSpecial ?? false;

    // ── KPI values for the selected customer ─────────────────────────
    protected decimal DetailTotalSpent => CustomerInvoices.Sum(i => i.InvoiceNet);
    protected int DetailInvoiceCount => CustomerInvoices.Count;
    protected decimal DetailAvgInvoice => DetailInvoiceCount > 0 ? DetailTotalSpent / DetailInvoiceCount : 0;
    protected DateTime? DetailFirstDate => CustomerInvoices.Count > 0 ? CustomerInvoices.Min(i => i.SaleDate) : null;
    protected DateTime? DetailLastDate => CustomerInvoices.Count > 0 ? CustomerInvoices.Max(i => i.SaleDate) : null;

    protected decimal DetailMonthsSpanned
    {
        get
        {
            if (DetailFirstDate == null || DetailLastDate == null) return 0;
            var first = DetailFirstDate.Value;
            var last = DetailLastDate.Value;
            return Math.Max(1, ((last.Year - first.Year) * 12) + last.Month - first.Month + 1);
        }
    }

    protected decimal DetailFrequency => DetailMonthsSpanned > 0
        ? DetailInvoiceCount / DetailMonthsSpanned
        : 0;

    // Same loyalty rule the mock report showed (3% of net spend, whole points).
    protected int DetailLoyaltyPoints => (int)Math.Floor(Math.Max(0, DetailTotalSpent) * 0.03m);

    protected decimal DetailTotalQty => CustomerLines.Sum(l => l.Quantity);
    protected decimal DetailTotalGross => CustomerLines.Sum(l => l.GrossAmount);
    protected decimal DetailTotalLineDiscount => CustomerLines.Sum(l => l.LineDiscount);
    protected decimal DetailTotalVat => CustomerLines.Sum(l => l.Vat);
    protected decimal DetailTotalNet => CustomerLines.Sum(l => l.LineNet);

    // ──────────────────────────────────────────────────────────────────
    //  Receipt (Radzen dialog)
    // ──────────────────────────────────────────────────────────────────

    protected async Task OpenReceipt(string invoiceNo)
    {
        if (SelectedCustomer == null)
            return;

        var invoice = CustomerInvoices.FirstOrDefault(i => i.InvoiceNo == invoiceNo);
        if (invoice == null)
            return;

        await dialogService.OpenAsync<CustomerSalesReceiptDialog>(
            "Invoice Slip Preview",
            new Dictionary<string, object>
            {
                { nameof(CustomerSalesReceiptDialog.Invoice), invoice },
                { nameof(CustomerSalesReceiptDialog.CustomerName), SelectedCustomer.DisplayName },
                { nameof(CustomerSalesReceiptDialog.CustomerPhone), SelectedCustomer.DisplayPhone }
            },
            new DialogOptions
            {
                Width = "600px",
                Resizable = true,
                Draggable = true,
                ShowClose = true
            });
    }

    // ──────────────────────────────────────────────────────────────────
    //  Print / PDF export (QuestPDF, same pattern as the detail report)
    // ──────────────────────────────────────────────────────────────────

    protected async Task PrintReport()
    {
        try
        {
            IsPrinting = true;
            StateHasChanged();

            var printedBy = await ResolvePrintedByAsync();
            var filterText = BuildFilterText();

            if (IsDetailView && SelectedCustomer != null)
            {
                if (!CustomerInvoices.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "No Data",
                        "There is nothing to print for this customer.");
                    return;
                }

                var pdfBytes = CustomerSalesReportPdfService.GenerateCustomerStatement(
                    SelectedCustomer,
                    CustomerInvoices,
                    dateFrom: FromDate,
                    dateTo: ToDate,
                    printedBy: printedBy,
                    filterText: filterText);

                var fileName = $"CustomerStatement_{SelectedCustomer.CustomerId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"Customer statement generated — {CustomerInvoices.Count} invoice(s).");
            }
            else
            {
                if (!Summaries.Any())
                {
                    notificationService.Notify(NotificationSeverity.Warning, "No Data",
                        "There are no customer sales records to print.");
                    return;
                }

                var pdfBytes = CustomerSalesReportPdfService.GenerateSummaryReport(
                    Summaries,
                    dateFrom: FromDate,
                    dateTo: ToDate,
                    printedBy: printedBy,
                    filterText: filterText);

                var fileName = $"CustomerSalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

                notificationService.Notify(NotificationSeverity.Success, "Success",
                    $"Customer sales summary generated — {Summaries.Count} customer(s).");
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed",
                $"Could not generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Human-readable summary of the active filters, printed under the report
    /// title so a PDF on file can be traced back to the query that produced it.
    /// </summary>
    private string BuildFilterText()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(SearchText)) parts.Add($"Search: {SearchText.Trim()}");
        if (!string.IsNullOrWhiteSpace(FilterBarcode)) parts.Add($"Barcode: {FilterBarcode.Trim()}");

        if (SelectedGroupId.HasValue)
        {
            var name = Groups.FirstOrDefault(g => g.Id == SelectedGroupId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(name)) parts.Add($"Group: {name}");
        }

        if (SelectedSubGroupId.HasValue)
        {
            var name = AllSubGroups.FirstOrDefault(g => g.Id == SelectedSubGroupId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(name)) parts.Add($"Product: {name}");
        }

        if (SelectedDesignId.HasValue)
        {
            var name = AllDesigns.FirstOrDefault(d => d.Id == SelectedDesignId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(name)) parts.Add($"Sub Product: {name}");
        }

        if (SelectedBrandId.HasValue)
        {
            var name = Brands.FirstOrDefault(b => b.BrandId == SelectedBrandId.Value)?.BrandName;
            if (!string.IsNullOrWhiteSpace(name)) parts.Add($"Brand: {name}");
        }

        if (SelectedStoreId.HasValue)
        {
            var name = Stores.FirstOrDefault(s => s.Id == SelectedStoreId.Value)?.Name;
            if (!string.IsNullOrWhiteSpace(name)) parts.Add($"Branch: {name}");
        }

        if (MinAmount.HasValue) parts.Add($"Min Amount: {MinAmount.Value:N2}");
        if (MaxAmount.HasValue) parts.Add($"Max Amount: {MaxAmount.Value:N2}");

        return parts.Count == 0 ? string.Empty : "Filters — " + string.Join("  |  ", parts);
    }

    private async Task<string> ResolvePrintedByAsync()
    {
        try
        {
            var state = await AuthStateTask;
            var name = state.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }
        catch
        {
            // fall through to the default
        }

        return "-";
    }

    // ──────────────────────────────────────────────────────────────────
    //  Formatting helpers
    // ──────────────────────────────────────────────────────────────────

    protected static string Money(decimal value) => value.ToString("N2");
    protected static string MoneyOrDash(decimal value) => value == 0 ? "-" : value.ToString("N2");

    protected static string FmtDate(DateTime value) => value.ToString("dd-MMM-yyyy");
    protected static string FmtDateTime(DateTime value) => value.ToString("dd-MMM-yyyy hh:mm tt");

    protected static string FmtQty(decimal qty, string? unit)
        => string.IsNullOrWhiteSpace(unit) ? qty.ToString("N2") : $"{qty:N2} {unit}";

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected static BadgeStyle TierBadge(string? tier)
    {
        var value = (tier ?? string.Empty).Trim().ToUpperInvariant();
        return value switch
        {
            "PLATINUM" => BadgeStyle.Primary,
            "VIP" or "GOLD" => BadgeStyle.Warning,
            "SILVER" => BadgeStyle.Light,
            _ => BadgeStyle.Secondary
        };
    }
}
