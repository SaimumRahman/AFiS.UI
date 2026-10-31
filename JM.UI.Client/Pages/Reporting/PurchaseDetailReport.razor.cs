using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class PurchaseDetailReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public PurchaseDetailReportService PurchaseDetailReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    protected List<PurchaseDetailDTO> Rows { get; set; } = new();
    protected List<Challan> Challans { get; set; } = new();

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class Section
    {
        public string BillNo { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public List<PurchaseDetailDTO> Rows { get; set; } = new();
    }

    protected class Challan
    {
        public int PurchaseId { get; set; }
        public string ChallanNo { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public List<Section> Sections { get; set; } = new();

        public decimal TotalPur => Sections.Sum(s => s.Rows.Sum(r => r.PurTotal));
        public decimal TotalSale => Sections.Sum(s => s.Rows.Sum(r => r.SaleTotal));
    }

    protected static string Fmt(decimal value) => value.ToString("N2");

    protected static string FmtQty(decimal value) => value.ToString("N2");

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected static string Breakdown(PurchaseDetailDTO r)
        => $"Pp-{r.ProductPrice:N0} · CC-{r.CarryingCost:N0} · TR-{r.OtherCost:N0} · OC-{r.OperationalCost:N0} · VAT-{r.VatAmount:N0}";

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
            Rows = (await _serviceUnitOfWork.ReportingService.GetPurchaseDetail(
                        FromDate, ToDate))?.ToList()
                    ?? new List<PurchaseDetailDTO>();

            Challans = BuildChallans(Rows);
        }
        catch (Exception ex)
        {
            Rows = new List<PurchaseDetailDTO>();
            Challans = new List<Challan>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<Challan> BuildChallans(List<PurchaseDetailDTO> data)
    {
        var challans = new List<Challan>();

        foreach (var row in data)
        {
            if (challans.Count == 0 || challans[^1].PurchaseId != row.PurchaseId)
            {
                challans.Add(new Challan
                {
                    PurchaseId = row.PurchaseId,
                    ChallanNo = row.ChallanNo,
                    SupplierName = row.SupplierName,
                    BillDate = row.BillDate
                });
            }

            var challan = challans[^1];
            var key = (Bill: row.BillNo ?? string.Empty, Group: row.GroupName ?? string.Empty);

            if (challan.Sections.Count == 0 ||
                challan.Sections[^1].BillNo != key.Bill ||
                challan.Sections[^1].GroupName != key.Group)
            {
                challan.Sections.Add(new Section { BillNo = key.Bill, GroupName = key.Group });
            }

            challan.Sections[^1].Rows.Add(row);
        }

        return challans;
    }

    protected async Task PrintReport()
    {
        if (!Rows.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "There are no purchase records to print.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            var pdfBytes = PurchaseDetailReportService.GeneratePurchaseDetailReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate);

            var fileName = $"PurchaseDetail_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Purchase detail report generated — {Challans.Count} challan(s).");
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
}
