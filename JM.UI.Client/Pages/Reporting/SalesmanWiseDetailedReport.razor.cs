using JM.UI.Entities.Model.Employees;
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
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class SalesmanWiseDetailedReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public SalesmanWiseDetailedReportService SalesmanWiseDetailedReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<SalesmanWiseDetailedDTO> Rows { get; set; } = new();
    protected List<Salesman> Salesmen { get; set; } = new();

    // ── Filters ──
    protected List<StoreDTO> Stores { get; set; } = new();
    protected int? SelectedStoreId { get; set; }

    // Salesman list comes from the same source the SalesPOS page uses
    // (EmployeeService) so both pages offer identical names.
    protected List<EmployeeModelDTO> Employees { get; set; } = new();
    protected int? SelectedSalesmanId { get; set; }
    protected string? SelectedSalesmanName { get; set; }

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }
    protected bool IsExporting { get; set; }

    protected string? SelectedStoreName =>
        SelectedStoreId.HasValue ? Stores.FirstOrDefault(s => s.Id == SelectedStoreId.Value)?.Name : null;

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class Line
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CampDisc { get; set; }
    }

    protected class Invoice
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public decimal InvDisc { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Dues { get; set; }
        public List<Line> Lines { get; set; } = new();
    }

    protected class Salesman
    {
        public string Name { get; set; } = string.Empty;
        public List<Invoice> Invoices { get; set; } = new();

        public decimal GrossTotal => Invoices.Sum(i => i.Lines.Sum(l => l.GrossAmount));
        public decimal CampDiscTotal => Invoices.Sum(i => i.Lines.Sum(l => l.CampDisc));
        public decimal InvDiscTotal => Invoices.Sum(i => i.InvDisc);
        public decimal TotalAmtTotal => Invoices.Sum(i => i.TotalAmt);
        public decimal ExchangeTotal => Invoices.Sum(i => i.ExchangeAmt);
        public decimal NetPaidTotal => Invoices.Sum(i => i.NetPaid);
        public decimal DuesTotal => Invoices.Sum(i => i.Dues);
    }

    protected static string Fmt(decimal value) => value == 0 ? "-" : value.ToString("N2");

    protected static string ExchangeFmt(decimal value)
        => value == 0 ? "-" : value < 0 ? $"({Math.Abs(value):N2})" : value.ToString("N2");

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected static string QtyDisplay(decimal qty, string uom) => $"{qty:N2} {Dash(uom)}";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadStores();
        await LoadEmployees();
        await LoadReport();
    }

    protected async Task LoadStores()
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

    protected async Task LoadEmployees()
    {
        try
        {
            if (SelectedStoreId.HasValue && SelectedStoreId.Value > 0)
                Employees = (await _serviceUnitOfWork.EmployeeService
                                .GetEmployeesByStoreId(SelectedStoreId.Value))?.ToList()
                            ?? new List<EmployeeModelDTO>();
            else
                Employees = (await _serviceUnitOfWork.EmployeeService.GetEmployees())?.ToList()
                            ?? new List<EmployeeModelDTO>();
        }
        catch
        {
            Employees = new List<EmployeeModelDTO>();
        }
    }

    protected async Task OnBranchChanged(object? value)
    {
        // Branch drives which salesmen are offered, same as the POS employee picker.
        SelectedStoreId = value is int id ? id : (int?)null;
        SelectedSalesmanId = null;
        SelectedSalesmanName = null;
        await LoadEmployees();
        await LoadReport();
    }

    protected async Task OnSalesmanChanged(object? value)
    {
        SelectedSalesmanId = value is int id ? id : (int?)null;
        SelectedSalesmanName = Employees
            .FirstOrDefault(e => e.Id == SelectedSalesmanId)?.Name;
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetSalesmanWiseDetailed(
                        FromDate, ToDate, SelectedStoreId, SelectedSalesmanName))?.ToList()
                    ?? new List<SalesmanWiseDetailedDTO>();

            Salesmen = BuildSalesmen(Rows);
        }
        catch (Exception ex)
        {
            Rows = new List<SalesmanWiseDetailedDTO>();
            Salesmen = new List<Salesman>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task ClearFilters()
    {
        SelectedStoreId = null;
        SelectedSalesmanId = null;
        SelectedSalesmanName = null;
        await LoadEmployees();
        await LoadReport();
    }

    private static List<Salesman> BuildSalesmen(List<SalesmanWiseDetailedDTO> data)
    {
        var salesmen = new List<Salesman>();

        foreach (var row in data)
        {
            var name = row.SalesmanName ?? string.Empty;
            if (salesmen.Count == 0 || salesmen[^1].Name != name)
                salesmen.Add(new Salesman { Name = name });

            var salesman = salesmen[^1];
            if (salesman.Invoices.Count == 0 || salesman.Invoices[^1].InvoiceNo != (row.InvoiceNo ?? string.Empty))
            {
                salesman.Invoices.Add(new Invoice
                {
                    InvoiceNo = row.InvoiceNo,
                    SaleDate = row.SaleDate,
                    Remarks = row.Remarks,
                    InvDisc = row.InvDisc,
                    TotalAmt = row.TotalAmt,
                    ExchangeAmt = row.ExchangeAmt,
                    NetPaid = row.NetPaid,
                    Dues = row.Dues
                });
            }

            salesman.Invoices[^1].Lines.Add(new Line
            {
                ItemCode = row.ItemCode,
                ItemName = row.ItemName,
                Quantity = row.Quantity,
                Uom = row.Uom,
                Rate = row.Rate,
                GrossAmount = row.GrossAmount,
                CampDisc = row.CampDisc
            });
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

            var pdfBytes = SalesmanWiseDetailedReportService.GenerateSalesmanWiseDetailedReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"SalesmanWiseDetailed_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Salesman-wise detailed report generated — {Salesmen.Count} salesman.");
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

    protected async Task ExportCsv()
    {
        if (!Salesmen.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "There are no sales records to download.");
            return;
        }

        try
        {
            IsExporting = true;
            StateHasChanged();

            var sb = new StringBuilder();
            sb.AppendLine("Salesman,Branch,Invoice No,Date,Remarks,Item Code,Item Name,Qty,UOM,Rate,Gross,Camp.Disc,Inv.Disc,Total Amt,Exchange,Net Paid,Dues");

            foreach (var salesman in Salesmen)
            {
                foreach (var invoice in salesman.Invoices)
                {
                    var first = true;
                    foreach (var line in invoice.Lines)
                    {
                        sb.Append(Csv(salesman.Name)).Append(',')
                          .Append(Csv(SelectedStoreName ?? string.Empty)).Append(',')
                          .Append(Csv(invoice.InvoiceNo)).Append(',')
                          .Append(invoice.SaleDate.ToString("dd-MMM-yyyy")).Append(',')
                          .Append(Csv(invoice.Remarks)).Append(',')
                          .Append(Csv(line.ItemCode)).Append(',')
                          .Append(Csv(line.ItemName)).Append(',')
                          .Append(line.Quantity.ToString("0.##")).Append(',')
                          .Append(Csv(line.Uom)).Append(',')
                          .Append(line.Rate.ToString("0.00")).Append(',')
                          .Append(line.GrossAmount.ToString("0.00")).Append(',')
                          .Append(line.CampDisc.ToString("0.00")).Append(',');

                        // Invoice-level amounts are written once, on the invoice's first line.
                        if (first)
                        {
                            sb.Append(invoice.InvDisc.ToString("0.00")).Append(',')
                              .Append(invoice.TotalAmt.ToString("0.00")).Append(',')
                              .Append(invoice.ExchangeAmt.ToString("0.00")).Append(',')
                              .Append(invoice.NetPaid.ToString("0.00")).Append(',')
                              .Append(invoice.Dues.ToString("0.00"));
                            first = false;
                        }
                        else
                        {
                            sb.Append(",,,,");
                        }
                        sb.AppendLine();
                    }
                }
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            var fileName = $"SalesmanWiseDetailed_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "text/csv", bytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"CSV downloaded — {Salesmen.Count} salesman.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed", $"Could not export report: {ex.Message}");
        }
        finally
        {
            IsExporting = false;
            StateHasChanged();
        }
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
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
