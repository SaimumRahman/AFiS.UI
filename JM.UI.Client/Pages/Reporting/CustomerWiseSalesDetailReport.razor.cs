using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class CustomerWiseSalesDetailReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public CustomerWiseSalesDetailReportService CustomerWiseSalesDetailReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<CustomerWiseSalesDetailDTO> Rows { get; set; } = new();
    protected List<StoreDTO> Stores { get; set; } = new();
    protected List<CustomerDetailsDTO> Customers { get; set; } = new();

    protected int? SelectedStoreId { get; set; }
    protected int? SelectedCustomerId { get; set; }
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} To {ToDate:dd-MMM-yyyy}"
            : "ALL TIME";

    // ── Preview grouping mirrors the PDF layout ───────────────────────────

    protected CustomerDetailsDTO? CustomerById(int id)
        => id <= 0 ? null : Customers?.FirstOrDefault(c => c.Id == id);

    protected static decimal QtyOf(IEnumerable<CustomerWiseSalesDetailDTO> rows)
        => rows.Sum(r => r.Quantity);

    protected static decimal GrossOf(IEnumerable<CustomerWiseSalesDetailDTO> rows)
        => rows.Sum(r => r.Rate * r.Quantity);

    // Invoice level amounts are repeated on every line, so sum them once per invoice.
    protected static decimal InvoiceSum(IEnumerable<CustomerWiseSalesDetailDTO> rows, Func<CustomerWiseSalesDetailDTO, decimal> selector)
        => rows.GroupBy(r => r.SaleMasterId).Sum(g => selector(g.First()));

    protected static decimal InvDiscOf(IEnumerable<CustomerWiseSalesDetailDTO> rows) => InvoiceSum(rows, r => r.InvDisc);
    protected static decimal CampDiscOf(IEnumerable<CustomerWiseSalesDetailDTO> rows) => InvoiceSum(rows, r => r.CampaignDisc);
    protected static decimal TotalDiscOf(IEnumerable<CustomerWiseSalesDetailDTO> rows) => InvoiceSum(rows, r => r.TotalDiscount);
    protected static decimal NetPaidOf(IEnumerable<CustomerWiseSalesDetailDTO> rows) => InvoiceSum(rows, r => r.NetPaid);
    protected static decimal DueOf(IEnumerable<CustomerWiseSalesDetailDTO> rows) => InvoiceSum(rows, r => r.Due);

    protected static string Fmt(decimal value) => value == 0 ? "-" : value.ToString("N2");
    protected static string FmtQty(decimal qty, string unit)
        => qty == 0 ? "-" : (string.IsNullOrWhiteSpace(unit) ? qty.ToString("N2") : $"{qty:N2} {unit}");

    protected static string FmtDate(DateTime value)
        => $"{value:dd-MMM-yyyy} {value:hh\\.mm}{value:tt}";

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadStores();
        await LoadCustomers();
        await LoadReport();
    }

    private async Task LoadStores()
    {
        try
        {
            Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new List<StoreDTO>();
        }
        catch
        {
            Stores = new List<StoreDTO>();
        }
    }

    private async Task LoadCustomers()
    {
        try
        {
            Customers = (await _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers())?.ToList()
                        ?? new List<CustomerDetailsDTO>();
        }
        catch
        {
            Customers = new List<CustomerDetailsDTO>();
        }
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetCustomerWiseSalesDetail(
                        SelectedStoreId, SelectedCustomerId, FromDate, ToDate))?.ToList()
                    ?? new List<CustomerWiseSalesDetailDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<CustomerWiseSalesDetailDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task PrintReport()
    {
        if (!Rows.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "There are no sales records to print.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            var pdfBytes = CustomerWiseSalesDetailReportService.GenerateCustomerWiseSalesDetailReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"CustomerWiseSalesDetail_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Customer-wise sales detail generated — {Rows.Count} line(s).");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed", $"Could not generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
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
}
