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

public partial class GroupWiseSalesDetailsReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public GroupWiseSalesDetailsReportService GroupWiseSalesDetailsReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected List<GroupWiseSalesDetailsDTO> Rows { get; set; } = new();
    protected List<Block> Blocks { get; set; } = new();

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy} TO {ToDate:dd-MMM-yyyy}".ToUpperInvariant()
            : "ALL TIME";

    protected class Block
    {
        public string GroupName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public List<GroupWiseSalesDetailsDTO> Rows { get; set; } = new();
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
            Rows = (await _serviceUnitOfWork.ReportingService.GetGroupWiseSalesDetails(
                        FromDate, ToDate))?.ToList()
                    ?? new List<GroupWiseSalesDetailsDTO>();

            Blocks = BuildBlocks(Rows);
        }
        catch (Exception ex)
        {
            Rows = new List<GroupWiseSalesDetailsDTO>();
            Blocks = new List<Block>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<Block> BuildBlocks(List<GroupWiseSalesDetailsDTO> data)
    {
        var blocks = new List<Block>();

        foreach (var row in data)
        {
            var key = (Group: row.GroupName ?? string.Empty, Product: row.ProductName ?? string.Empty);

            if (blocks.Count == 0 || blocks[^1].GroupName != key.Group || blocks[^1].ProductName != key.Product)
                blocks.Add(new Block { GroupName = key.Group, ProductName = key.Product });

            blocks[^1].Rows.Add(row);
        }

        return blocks;
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

            var pdfBytes = GroupWiseSalesDetailsReportService.GenerateGroupWiseSalesDetailsReport(
                Rows,
                dateFrom: FromDate,
                dateTo: ToDate,
                printedBy: await ResolvePrintedByAsync());

            var fileName = $"GroupWiseSalesDetails_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                $"Group-wise sales details generated — {Blocks.Count} block(s).");
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
