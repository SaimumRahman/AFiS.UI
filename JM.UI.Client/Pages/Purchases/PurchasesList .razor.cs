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
    protected IEnumerable<PurchaseSummaryDTO> Purchases = new List<PurchaseSummaryDTO>();
    protected Dictionary<int, List<PurchaseItemDTO>> PurchaseItemsCache = new();
    protected bool IsLoading;
    protected bool IsPrinting;

    protected override async Task OnInitializedAsync()
    {
        await LoadPurchases();
    }

    protected async Task OpenImagePreview(string imageBase64, string itemName)
    {
        await dialogService.OpenAsync<ImagePreviewDialog>(
            $"Item Image — {itemName}",
            new Dictionary<string, object>
            {
                { "ImageBase64", imageBase64 },
                { "ItemName", itemName }
            },
            new DialogOptions { Width = "600px", CloseDialogOnOverlayClick = true }
        );
    }

    protected async Task LoadPurchases()
    {
        try
        {
            IsLoading = true;
            Purchases = await ServiceUnitOfWork.PurchaseService.GetAllPurchases();
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

    protected List<PurchaseItemDTO> GetPurchaseItems(PurchaseSummaryDTO purchase)
    {
        return PurchaseItemsCache.GetValueOrDefault(purchase.Id) ?? new List<PurchaseItemDTO>();
    }

    protected async Task OnRowExpand(PurchaseSummaryDTO purchase)
    {
        if (PurchaseItemsCache.ContainsKey(purchase.Id))
            return;

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

    /// <summary>
    /// Loads ALL purchase items for every purchase (needed for full report),
    /// then generates and downloads the PDF.
    /// </summary>
    protected async Task PrintReport()
    {
        try
        {
            IsPrinting = true;
            StateHasChanged();

            // Load items for any purchases not yet in cache
            foreach (var purchase in Purchases)
            {
                if (!PurchaseItemsCache.ContainsKey(purchase.Id))
                {
                    var items = await ServiceUnitOfWork.PurchaseService.GetPurchaseItems(purchase.Id);
                    PurchaseItemsCache[purchase.Id] = items?.ToList() ?? new List<PurchaseItemDTO>();
                }
            }

            // Determine date range from loaded purchases
            DateTime? dateFrom = Purchases.Any() ? Purchases.Min(p => p.PurchaseDate) : null;
            DateTime? dateTo = Purchases.Any() ? Purchases.Max(p => p.PurchaseDate) : null;

            // Generate PDF bytes
            var pdfBytes = PurchaseReportService.GeneratePurchaseDetailReport(
                Purchases,
                PurchaseItemsCache,
                dateFrom: dateFrom,
                dateTo: dateTo
            );

            // Trigger browser download via JS interop
            var fileName = $"PurchaseDetailReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            NotifySuccess("Report generated successfully.");
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

    protected void AddPurchase()
    {
        NavigationManager.NavigateTo("/PurchaseEntry");
    }

    protected void EditPurchase(PurchaseSummaryDTO purchase)
    {
        NavigationManager.NavigateTo($"/PurchaseEntry/Edit/{purchase.Id}");
    }

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
                NotifySuccess("Purchase deleted successfully");
            }
            catch (Exception ex)
            {
                NotifyError($"Failed to delete purchase: {ex.Message}");
            }
        }
    }

    private void NotifyError(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000
        });
    }

    private void NotifySuccess(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 3000
        });
    }

    public void Dispose()
    {
        PurchasesGrid?.Dispose();
        PurchaseItemsCache.Clear();
    }
}