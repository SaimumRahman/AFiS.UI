using JM.UI.Client.Pages.Dialog;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

public partial class PurchaseListComponent : PosComponentBase, IDisposable
{
    [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;
    [Inject] public DialogService dialogService { get; set; } = default!;
    [Inject] public NotificationService NotificationService { get; set; } = default!;
    [Inject] public DialogService DialogService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public PurchaseReportService PurchaseReportService { get; set; } = default!;

    protected RadzenDataGrid<PurchaseSummaryDTO>? PurchasesGrid;

    // All purchases loaded from server
    protected IEnumerable<PurchaseSummaryDTO> Purchases = new List<PurchaseSummaryDTO>();

    // What the grid actually shows (after date filter)
    protected IEnumerable<PurchaseSummaryDTO> FilteredPurchases = new List<PurchaseSummaryDTO>();

    protected Dictionary<int, List<PurchaseItemDTO>> PurchaseItemsCache = new();

    protected bool IsLoading;
    protected bool IsPrinting;

    // ── Date filter state ─────────────────────────────────────────────────────
    protected DateTime? FilterDateFrom { get; set; }= DateTime.UtcNow;
    protected DateTime? FilterDateTo { get; set; } = DateTime.UtcNow;
    protected string ReferenceNo;
    protected bool IsFiltered => FilterDateFrom.HasValue || FilterDateTo.HasValue;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        await LoadPurchases();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    protected async Task LoadPurchases()
    {
        try
        {
            IsLoading = true;
            Purchases = await ServiceUnitOfWork.PurchaseService.GetAllPurchases();
            ApplyFilter();  // respect any active filter after refresh
        }
        catch (Exception ex)
        {
            NotifyError($"Failed to load purchases: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Date filter ───────────────────────────────────────────────────────────

    protected async Task ApplyDateFilter()
    {
        if (FilterDateFrom.HasValue && FilterDateTo.HasValue
            && FilterDateFrom > FilterDateTo)
        {
            NotifyError("'From Date' cannot be later than 'To Date'.");
            return;
        }

        // If searching by ReturnRefNo, we must pre-load ALL purchase items
        // so the in-memory filter can inspect them.
        if (!string.IsNullOrWhiteSpace(ReferenceNo))
        {
            IsLoading = true;
            StateHasChanged();

            foreach (var purchase in Purchases)
            {
                if (!PurchaseItemsCache.ContainsKey(purchase.Id))
                {
                    try
                    {
                        var items = await ServiceUnitOfWork.PurchaseService.GetPurchaseItems(purchase.Id);
                        PurchaseItemsCache[purchase.Id] = items?.ToList() ?? new List<PurchaseItemDTO>();
                    }
                    catch
                    {
                        PurchaseItemsCache[purchase.Id] = new List<PurchaseItemDTO>();
                    }
                }
            }

            IsLoading = false;
        }

        ApplyFilter();
        StateHasChanged();
    }

    protected void ClearDateFilter()
    {
        FilterDateFrom = null;
        FilterDateTo = null;
        FilteredPurchases = Purchases;
        ReferenceNo = string.Empty;
        PurchaseItemsCache.Clear();
        StateHasChanged();
    }

    /// <summary>Filters Purchases into FilteredPurchases based on the current date range.</summary>
    private void ApplyFilter()
    {
        var query = Purchases.AsEnumerable();

        if (FilterDateFrom.HasValue)
            query = query.Where(p => p.PurchaseDate.Date >= FilterDateFrom.Value.Date);

        if (FilterDateTo.HasValue)
            query = query.Where(p => p.PurchaseDate.Date <= FilterDateTo.Value.Date);

        // ReferenceNo filter — match against purchase items' ReturnRefNo
        if (!string.IsNullOrWhiteSpace(ReferenceNo))
        {
            var refTrimmed = ReferenceNo.Trim();

            // We need to check cached items. For purchases not yet loaded,
            // we load them synchronously via a helper list built during search.
            query = query.Where(p =>
                PurchaseItemsCache.TryGetValue(p.Id, out var items) &&
                items.Any(i => i.ReturnRefNo != null &&
                               i.ReturnRefNo.Contains(refTrimmed, StringComparison.OrdinalIgnoreCase)));
        }

        FilteredPurchases = query.ToList();
    }

    protected string GetActiveFilterLabel()
    {
        if (FilterDateFrom.HasValue && FilterDateTo.HasValue)
            return $"{FilterDateFrom:dd-MMM-yyyy}  →  {FilterDateTo:dd-MMM-yyyy}";

        if (FilterDateFrom.HasValue)
            return $"From {FilterDateFrom:dd-MMM-yyyy}";

        if (FilterDateTo.HasValue)
            return $"Up to {FilterDateTo:dd-MMM-yyyy}";

        return string.Empty;
    }

    // ── Row expand ────────────────────────────────────────────────────────────

    protected List<PurchaseItemDTO> GetPurchaseItems(PurchaseSummaryDTO purchase)
        => PurchaseItemsCache.GetValueOrDefault(purchase.Id) ?? new List<PurchaseItemDTO>();

    protected async Task OnRowExpand(PurchaseSummaryDTO purchase)
    {
        if (PurchaseItemsCache.ContainsKey(purchase.Id)) return;

        try
        {
            var items = await ServiceUnitOfWork.PurchaseService.GetPurchaseItems(purchase.Id);
            PurchaseItemsCache[purchase.Id] = items?.ToList() ?? new List<PurchaseItemDTO>();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            NotifyError($"Failed to load purchase items: {ex.Message}");
            PurchaseItemsCache[purchase.Id] = new List<PurchaseItemDTO>();
        }
    }

    // ── Print / PDF ───────────────────────────────────────────────────────────

    protected async Task PrintReport()
    {
        if (!FilteredPurchases.Any())
        {
            NotifyError("No records to print. Please adjust the date filter.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            // Load items for every purchase in the filtered set that isn't cached yet
            foreach (var purchase in FilteredPurchases)
            {
                if (!PurchaseItemsCache.ContainsKey(purchase.Id))
                {
                    var items = await ServiceUnitOfWork.PurchaseService.GetPurchaseItems(purchase.Id);
                    PurchaseItemsCache[purchase.Id] = items?.ToList() ?? new List<PurchaseItemDTO>();
                }
            }

            // Use the active filter dates for the report header;
            // fall back to the min/max dates in the current result set
            var reportFrom = FilterDateFrom
                ?? FilteredPurchases.Min(p => p.PurchaseDate);
            var reportTo = FilterDateTo
                ?? FilteredPurchases.Max(p => p.PurchaseDate);

            // Build a cache slice containing only the filtered purchases
            var filteredCache = FilteredPurchases
                .ToDictionary(
                    p => p.Id,
                    p => PurchaseItemsCache.GetValueOrDefault(p.Id) ?? new List<PurchaseItemDTO>());

            var pdfBytes = PurchaseReportService.GeneratePurchaseDetailReport(
                FilteredPurchases,
                filteredCache,
                dateFrom: reportFrom,
                dateTo: reportTo);

            var fileName = $"PurchaseReport_{reportFrom:yyyyMMdd}_to_{reportTo:yyyyMMdd}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            NotifySuccess($"Report generated — {FilteredPurchases.Count()} purchase(s).");
        }
        catch (Exception ex)
        {
            NotifyError($"Failed to generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    protected async Task OpenImagePreview(string imageBase64, string itemName)
    {
        await dialogService.OpenAsync<ImagePreviewDialog>(
            $"Item Image — {itemName}",
            new Dictionary<string, object>
            {
                { "ImageBase64", imageBase64 },
                { "ItemName",    itemName }
            },
            new DialogOptions { Width = "600px", CloseDialogOnOverlayClick = true });
    }

    protected void AddPurchase()
        => NavigationManager.NavigateTo("/PurchaseEntry");

    protected void EditPurchase(PurchaseSummaryDTO purchase)
        => NavigationManager.NavigateTo($"/PurchaseEntry/Edit/{purchase.Id}");

    protected async Task DeletePurchase(PurchaseSummaryDTO purchase)
    {
        var confirm = await DialogService.Confirm(
            $"Are you sure you want to delete purchase '{purchase.BillInvoiceNumber}'?",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirm == true)
        {
            try
            {
                await ServiceUnitOfWork.PurchaseService.DeletePurchase(purchase.Id);
                PurchaseItemsCache.Remove(purchase.Id);
                await LoadPurchases();
                NotifySuccess("Purchase deleted successfully.");
            }
            catch (Exception ex)
            {
                NotifyError($"Failed to delete purchase: {ex.Message}");
            }
        }
    }

    // ── Notifications ─────────────────────────────────────────────────────────

    private void NotifyError(string message) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000
        });

    private void NotifySuccess(string message) =>
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 3000
        });

    protected void NavigateToBarcodePrint(int purchaseId)
    {
        NavigationManager.NavigateTo($"/BarcodePrint/{purchaseId}");
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    public void Dispose()
    {
        PurchasesGrid?.Dispose();
        PurchaseItemsCache.Clear();
    }
}
