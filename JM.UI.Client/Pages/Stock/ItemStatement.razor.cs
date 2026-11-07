using JM.UI.Entities.Model.Stock;
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

namespace JM.UI.Client.Pages.Stock;

public partial class ItemStatementComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public ItemStatementPdfService ItemStatementPdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected ItemStatementDTO Statement { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected string BaseBarcode { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
    }

    protected async Task LoadStatement()
    {
        if (string.IsNullOrWhiteSpace(BaseBarcode))
        {
            notificationService.Notify(NotificationSeverity.Warning, "Missing barcode",
                "Please enter a base barcode.");
            return;
        }

        IsLoading = true;
        try
        {
            Statement = await _serviceUnitOfWork.StockService
                .GetItemStatement(BaseBarcode.Trim(), FromDate, ToDate) ?? new ItemStatementDTO();
        }
        catch (Exception ex)
        {
            Statement = new ItemStatementDTO();
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load item statement: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task ClearFilters()
    {
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
        BaseBarcode = string.Empty;
        Statement = new ItemStatementDTO();
    }

    protected async Task PrintPdf()
    {
        if (Statement == null || Statement.Sizes == null || Statement.Sizes.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data",
                "There is nothing to print.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            var printedBy = "-";
            try
            {
                var state = await AuthStateTask;
                if (!string.IsNullOrWhiteSpace(state.User.Identity?.Name))
                    printedBy = state.User.Identity!.Name!;
            }
            catch { }

            var pdfBytes = ItemStatementPdfService.Generate(Statement, FromDate, ToDate, printedBy);
            var fileName = $"ItemStatement_{Statement.BaseBarcode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                "Item statement PDF generated.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed",
                $"Could not generate PDF: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }

    protected static string Qty(decimal value) => value == 0 ? "-" : value.ToString("N2");
    protected static string Money(decimal value) => value.ToString("N2");
    protected static string FmtDate(DateTime value) => value.ToString("dd-MMM-yyyy");

    protected static string Dash(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    protected List<ItemStatementTxnRow> GetTxnRows(ItemStatementSizeDTO size)
    {
        var rows = new List<ItemStatementTxnRow>();

        // Opening balance row first.
        var openingDelta = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in size.Branches)
            openingDelta[b] = size.OpeningByBranch.TryGetValue(b, out var v) ? v : 0m;

        rows.Add(new ItemStatementTxnRow
        {
            Date = size.Transactions.Count > 0 ? size.Transactions[0].Date : (FromDate ?? DateTime.Today),
            Description = "Opening Balance",
            BranchDelta = openingDelta,
            RunningTotal = size.TotalOpening,
            IsOpening = true
        });

        // Then the real transactions (skip the synthetic opening row produced by the API).
        foreach (var txn in size.Transactions.Skip(1))
        {
            rows.Add(new ItemStatementTxnRow
            {
                Date = txn.Date,
                Description = txn.Description,
                BranchDelta = txn.BranchDelta ?? new Dictionary<string, decimal>(),
                RunningTotal = txn.RunningTotal,
                IsOpening = false
            });
        }

        return rows;
    }

    // Local row model for the Radzen grid.
    public class ItemStatementTxnRow
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, decimal> BranchDelta { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public decimal RunningTotal { get; set; }
        public bool IsOpening { get; set; }
    }
}
