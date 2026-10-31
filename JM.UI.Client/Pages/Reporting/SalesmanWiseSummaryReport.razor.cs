using JM.UI.Entities.Model.Reporting_D;
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

public partial class SalesmanWiseSummaryReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public SalesmanWiseSummaryReportService SalesmanWiseSummaryReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<SalesmanWiseSummaryDTO> Rows { get; set; } = new();
    protected List<Salesman> Salesmen { get; set; } = new();

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class Row
    {
        public string ProductSold { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal NetPaid { get; set; }
    }

    protected class DateGroup
    {
        public DateTime Date { get; set; }
        public bool IsNoSale { get; set; }
        public List<Row> Rows { get; set; } = new();
    }

    protected class Salesman
    {
        public string Name { get; set; } = string.Empty;
        public List<DateGroup> Dates { get; set; } = new();

        public List<Row> AllRows => Dates.Where(d => !d.IsNoSale).SelectMany(d => d.Rows).ToList();
    }

    protected static string Fmt(decimal value) => value == 0 ? "-" : value.ToString("N2");

    protected static string FmtQty(decimal value) => value.ToString("N2");

    protected static string Pct(decimal discAmt, decimal gross)
        => (discAmt == 0 || gross == 0) ? "-" : (discAmt / gross * 100m).ToString("N2");

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetSalesmanWiseSummary(
                        FromDate, ToDate))?.ToList()
                    ?? new List<SalesmanWiseSummaryDTO>();

            Salesmen = BuildSalesmen(Rows);
        }
        catch (Exception ex)
        {
            Rows = new List<SalesmanWiseSummaryDTO>();
            Salesmen = new List<Salesman>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<Salesman> BuildSalesmen(List<SalesmanWiseSummaryDTO> data)
    {
        var distinctDates = data.Select(d => d.SaleDate.Date).Distinct().OrderBy(d => d).ToList();
        var names = data.Select(d => d.SalesmanName ?? string.Empty).Distinct().OrderBy(n => n).ToList();

        var salesmen = new List<Salesman>();

        foreach (var name in names)
        {
            var salesman = new Salesman { Name = name };

            foreach (var date in distinctDates)
            {
                var rows = data
                    .Where(d => (d.SalesmanName ?? string.Empty) == name && d.SaleDate.Date == date)
                    .Select(d => new Row
                    {
                        ProductSold = d.ProductSold,
                        Quantity = d.Quantity,
                        GrossAmount = d.GrossAmount,
                        DiscAmt = d.DiscAmt,
                        ExchangeAmt = d.ExchangeAmt,
                        TotalAmt = d.TotalAmt,
                        NetPaid = d.NetPaid
                    })
                    .ToList();

                salesman.Dates.Add(new DateGroup { Date = date, IsNoSale = rows.Count == 0, Rows = rows });
            }

            salesmen.Add(salesman);
        }

        return salesmen;
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

            var pdfBytes = SalesmanWiseSummaryReportService.GenerateSalesmanWiseSummaryReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"SalesmanWiseSummary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Salesman-wise summary generated — {Salesmen.Count} salesman.");
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
