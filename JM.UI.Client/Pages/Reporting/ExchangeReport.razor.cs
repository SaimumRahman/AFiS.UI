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

public partial class ExchangeReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public ExchangeReportService ExchangeReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<StoreDTO> Stores { get; set; } = new();

    protected int? SelectedStoreId { get; set; }
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    /// <summary>
    /// One entry per exchange. The API returns a row per stacking position, so
    /// the rows are collapsed back into a single printed row whose Slots are
    /// stacked line-for-line in the old/new item and price cells.
    /// </summary>
    protected List<ExchangeGroup> Exchanges { get; set; } = new();

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class ExchangeGroup
    {
        public ExchangeReportDTO Head { get; set; } = default!;
        public List<ExchangeReportDTO> Slots { get; set; } = new();
    }

    // ── Preview grouping mirrors the PDF layout ───────────────────────────

    protected static List<ExchangeGroup> BuildGroups(IEnumerable<ExchangeReportDTO> rows)
        => rows
            .GroupBy(r => r.SaleMasterId)
            .Select(g => new ExchangeGroup
            {
                Head = g.First(),
                Slots = g.OrderBy(r => r.Position).ToList()
            })
            .OrderByDescending(e => e.Head.ExchangeDate)
            .ThenByDescending(e => e.Head.SaleMasterId)
            .ToList();

    // Exchange level amounts are repeated on every stacked row, so take them
    // once per exchange (Head) rather than summing the slots.
    protected static decimal OldTotalOf(IEnumerable<ExchangeGroup> groups)
        => groups.Sum(g => g.Head.OldTotalValue);

    protected static decimal NewTotalOf(IEnumerable<ExchangeGroup> groups)
        => groups.Sum(g => g.Head.NewTotalValue);

    protected static decimal AdjustmentOf(IEnumerable<ExchangeGroup> groups)
        => groups.Sum(g => g.Head.ExchangeAdjustment);

    protected static decimal PaidOf(IEnumerable<ExchangeGroup> groups)
        => groups.Sum(g => g.Head.PaidAmount);

    protected static string Fmt(decimal value) => value == 0 ? "-" : value.ToString("N2");

    protected static string FmtQtyRate(decimal qty, decimal rate, string? unit)
    {
        if (qty == 0 && rate == 0)
            return "";

        string text = $"{qty:0.##} x {rate:N2}";
        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    protected static string FmtExchangeDate(DateTime value) => value.ToString("dd-MMM-yy");

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadStores();
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

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            var rows = (await _serviceUnitOfWork.ReportingService.GetExchangeReport(
                            SelectedStoreId, FromDate, ToDate))?.ToList()
                        ?? new List<ExchangeReportDTO>();

            Exchanges = BuildGroups(rows);
        }
        catch (Exception ex)
        {
            Exchanges = new List<ExchangeGroup>();
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
        if (!Exchanges.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "There are no exchange records to print.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            // The PDF generator does its own grouping, so hand it the flat rows.
            var flatRows = Exchanges.SelectMany(e => e.Slots).ToList();

            var pdfBytes = ExchangeReportService.GenerateExchangeReport(
                flatRows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"ExchangeReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Exchange report generated — {Exchanges.Count} exchange(s).");
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
