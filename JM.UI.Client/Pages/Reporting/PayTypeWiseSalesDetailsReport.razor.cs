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

public partial class PayTypeWiseSalesDetailsReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public PayTypeWiseSalesDetailsReportService PayTypeWiseSalesDetailsReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    protected List<PayTypeWiseSalesDetailsDTO> Rows { get; set; } = new();
    protected List<Group> Groups { get; set; } = new();

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class Group
    {
        public DateTime Date { get; set; }
        public List<PayTypeWiseSalesDetailsDTO> Rows { get; set; } = new();
    }

    protected static string Fmt(decimal value) => value.ToString("N2");

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
            Rows = (await _serviceUnitOfWork.ReportingService.GetPayTypeWiseSalesDetails(
                        FromDate, ToDate))?.ToList()
                    ?? new List<PayTypeWiseSalesDetailsDTO>();

            Groups = BuildGroups(Rows);
        }
        catch (Exception ex)
        {
            Rows = new List<PayTypeWiseSalesDetailsDTO>();
            Groups = new List<Group>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<Group> BuildGroups(List<PayTypeWiseSalesDetailsDTO> data)
    {
        var groups = new List<Group>();

        foreach (var row in data)
        {
            if (groups.Count == 0 || groups[^1].Date.Date != row.SaleDate.Date)
                groups.Add(new Group { Date = row.SaleDate.Date });

            groups[^1].Rows.Add(row);
        }

        return groups;
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

            var pdfBytes = PayTypeWiseSalesDetailsReportService.GeneratePayTypeWiseSalesDetailsReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate);

            var fileName = $"PayTypeWiseSalesDetails_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Pay type-wise sales details generated — {Groups.Count} day(s).");
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
