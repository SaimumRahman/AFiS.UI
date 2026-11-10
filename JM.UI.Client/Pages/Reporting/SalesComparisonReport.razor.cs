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

public partial class SalesComparisonReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public SalesComparisonReportService SalesComparisonReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    // ── Filters ──
    protected string ReportType { get; set; } = "month"; // "date" | "month"
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }

    protected List<StoreDTO> Stores { get; set; } = new();
    protected List<int> SelectedStoreIds { get; set; } = new();
    protected bool CanSelectAnyStore { get; set; }
    protected int CurrentUserId { get; set; }
    protected int UserStoreId { get; set; }

    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    // ── Pivot state ──
    protected List<SalesComparisonStoreGroup> StoreGroups { get; set; } = new();
    protected List<SalesComparisonPivotRow> PivotRows { get; set; } = new();

    protected int PivotPeriodCount => PivotRows.Count;

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy}  TO  {ToDate:dd-MMM-yyyy}"
            : "ALL TIME";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        UserStoreId = await GetLocalStorageInt("StoreId");

        CanSelectAnyStore = CurrentUserId == 1;
        if (!CanSelectAnyStore)
        {
            try
            {
                CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                                        .GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch { CanSelectAnyStore = false; }
        }

        await LoadStores();
        // Default range: current year, month view.
        var now = DateTime.Now;
        FromDate = new DateTime(now.Year, 1, 1);
        ToDate = new DateTime(now.Year, 12, 31);
    }

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var r = await _localStorage.GetAsync<string>(key);
            if (r.Success && int.TryParse(r.Value, out var v)) return v;
        }
        catch { }
        return 0;
    }

    protected async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
            if (CanSelectAnyStore)
            {
                Stores = stores.ToList();
            }
            else
            {
                Stores = stores.Where(s => s.Id == UserStoreId).ToList();
            }
            SelectedStoreIds = Stores.Select(s => s.Id).ToList();
        }
        catch
        {
            Stores = new List<StoreDTO>();
            SelectedStoreIds = new List<int>();
        }
    }

    protected void ToggleStore(int storeId, bool isChecked)
    {
        if (isChecked)
        {
            if (!SelectedStoreIds.Contains(storeId)) SelectedStoreIds.Add(storeId);
        }
        else
        {
            SelectedStoreIds.Remove(storeId);
        }
    }

    protected void ToggleAllStores(bool isChecked)
    {
        if (isChecked) SelectedStoreIds = Stores.Select(s => s.Id).ToList();
        else SelectedStoreIds.Clear();
    }

    protected bool AllStoresSelected =>
        Stores.Count > 0 && Stores.All(s => SelectedStoreIds.Contains(s.Id));

    protected async Task Generate()
    {
        if (SelectedStoreIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Branch",
                "Please select at least one branch.");
            return;
        }

        IsLoading = true;
        try
        {
            var (from, to, groupBy) = NormalizeRange();
            var storeIds = string.Join(",", SelectedStoreIds);
            var rows = await _serviceUnitOfWork.ReportingService.GetSalesComparison(
                from, to, groupBy, storeIds);

            BuildPivot(rows?.ToList() ?? new List<SalesComparisonDTO>());
        }
        catch (Exception ex)
        {
            PivotRows = new List<SalesComparisonPivotRow>();
            StoreGroups = new List<SalesComparisonStoreGroup>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private (DateTime? from, DateTime? to, string groupBy) NormalizeRange()
    {
        if (ReportType == "month")
        {
            var from = FromDate.HasValue
                ? new DateTime(FromDate.Value.Year, FromDate.Value.Month, 1)
                : (DateTime?)null;
            var to = ToDate.HasValue
                ? new DateTime(ToDate.Value.Year, ToDate.Value.Month,
                               DateTime.DaysInMonth(ToDate.Value.Year, ToDate.Value.Month))
                : (DateTime?)null;
            return (from, to, "month");
        }
        return (FromDate, ToDate, "day");
    }

    private void BuildPivot(List<SalesComparisonDTO> rows)
    {
        var selectedNames = Stores
            .Where(s => SelectedStoreIds.Contains(s.Id))
            .Select(s => s.Name.Trim())
            .ToList();

        StoreGroups = selectedNames
            .Select(name => new SalesComparisonStoreGroup
            {
                StoreName = name,
                StoreId = Stores.First(s => s.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)).Id
            })
            .ToList();

        PivotRows = rows
            .GroupBy(r => r.PeriodKey)
            .Select(g =>
            {
                var first = g.First();
                var row = new SalesComparisonPivotRow
                {
                    PeriodKey = g.Key,
                    PeriodLabel = first.PeriodLabel
                };
                foreach (var r in g)
                {
                    var name = (r.StoreName ?? string.Empty).Trim();
                    if (!selectedNames.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                    row.Cells[name] = new SalesComparisonCell
                    {
                        Total = r.TotalSales,
                        Discount = r.TotalDiscount,
                        Net = r.NetSales
                    };
                }
                row.CombinedTotal = row.Cells.Values.Sum(c => c.Total);
                row.CombinedDiscount = row.Cells.Values.Sum(c => c.Discount);
                row.CombinedNet = row.Cells.Values.Sum(c => c.Net);
                return row;
            })
            .OrderBy(r => r.PeriodKey)
            .ToList();
    }

    protected async Task PrintReport()
    {
        if (!PivotRows.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data",
                "Generate the report first.");
            return;
        }
        try
        {
            IsPrinting = true;
            StateHasChanged();

            var pdfBytes = SalesComparisonReportService.GenerateSalesComparisonReport(
                PivotRows, StoreGroups,
                FromDate, ToDate, ReportType == "month" ? "Month Range" : "Date Range",
                await ResolvePrintedByAsync());

            var fileName = $"SalesComparison_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                "Sales comparison report generated.");
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

    private async Task<string> ResolvePrintedByAsync()
    {
        try
        {
            var state = await AuthStateTask;
            var name = state.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch { }
        return "-";
    }
}
