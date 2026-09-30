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

public partial class GroupWiseSalesSummaryReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public GroupWiseSalesSummaryReportService GroupWiseSalesSummaryReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<GroupWiseSalesSummaryDTO> Rows { get; set; } = new();

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

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
            Rows = (await _serviceUnitOfWork.ReportingService.GetGroupWiseSalesSummary(
                        FromDate, ToDate))?.ToList()
                    ?? new List<GroupWiseSalesSummaryDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<GroupWiseSalesSummaryDTO>();
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

            var pdfBytes = GroupWiseSalesSummaryReportService.GenerateGroupWiseSalesSummaryReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"GroupWiseSalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Group-wise sales summary generated — {Rows.Count} group(s).");
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
