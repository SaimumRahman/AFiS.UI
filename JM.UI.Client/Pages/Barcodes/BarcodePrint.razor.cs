using JM.UI.Entities.Model.Barcodes;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace JM.UI.Client.Pages.Barcode;

public partial class BarcodePrintComponent : PosComponentBase
{
    // ── Scale / Pixel helpers ────────────────────────────────────
    private const double ScaleFactor = 2.2;
    protected int LabelWidthPx => (int)(PrintConfig.LabelWidthMm * (decimal)ScaleFactor);
    protected int LabelHeightPx => (int)(PrintConfig.LabelHeightMm * (decimal)ScaleFactor);
    protected int QrSizePx => (int)(LabelHeightPx * 0.52);          // QR box ~52 % of label height
    protected int NameFontSizePx => Math.Max(7, (int)(LabelHeightPx * 0.14));
    protected int BarcodeFontSizePx => Math.Max(5, (int)(LabelHeightPx * 0.09));
    protected int SmallFontPx => Math.Max(5, (int)(LabelHeightPx * 0.09));
    protected bool ShowPreview { get; set; } = false;
    protected bool ShowPrinterConfig { get; set; } = false;
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public BarcodePrintPdfService _barcodePrintPdfService { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    // ── Template State ───────────────────────────────────────────
    protected List<BarcodeTemplateDTO> BarcodeTemplates { get; set; } = new();
    protected int? SelectedTemplateId { get; set; }
    protected BarcodeTemplateDTO? SelectedTemplate { get; set; }

    /// <summary>Parsed fields from SelectedTemplate.Descriptions, e.g. ["ProductName","Brand","UOM","Price"]</summary>
    protected List<string> TemplateFields { get; set; } = new();

    protected bool IsLoadingTemplates { get; set; } = false;

    // ── Filter State ─────────────────────────────────────────────
    protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
    protected DateTime ToDate { get; set; } = DateTime.Today;

    // ── Purchase Dropdown ────────────────────────────────────────
    protected List<PurchaseInvoiceDTO> PurchaseList { get; set; } = new();
    protected int? SelectedPurchaseId { get; set; }
    protected PurchaseInvoiceDTO? SelectedPurchase { get; set; }

    // ── Single-Barcode Dropdown ──────────────────────────────────
    protected List<BarcodeItemDTO> AllBarcodes { get; set; } = new();
    protected List<BarcodeSelectItem> BarcodeSelectItems { get; set; } = new();
    protected int? SelectedBarcodeId { get; set; }
    protected decimal SinglePrintQty { get; set; } = 1;

    public class BarcodeSelectItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = "";
        public string BarcodeValue { get; set; } = "";
        public string ReturnRefNo { get; set; } = "";
        /// <summary>Combined search text so the built-in filter searches all fields.</summary>
        public string SearchText => $"{ProductName} {BarcodeValue} {ReturnRefNo}";
    }

    // ── Print Preview List ───────────────────────────────────────
    protected List<BarcodePrintItemDTO> PrintItems { get; set; } = new();

    // ── Configuration (from DB) ──────────────────────────────────
    protected BarcodePrintConfigDTO PrintConfig { get; set; } = new();

    // ── UI State ────────────────────────────────────────────────
    protected bool IsLoadingPurchases { get; set; } = false;
    protected bool IsLoadingBarcodes { get; set; } = false;
    protected bool IsPrinting { get; set; } = false;
    protected bool IsDownloadingPdf { get; set; } = false;
    protected bool IsLoadingConfig { get; set; } = false;

    [Parameter] public int? PurchaseId { get; set; }

    // ────────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        await Task.WhenAll(
            LoadPrintConfiguration(),
            LoadBarcodeTemplates(),
            LoadPurchaseList(),
            LoadAllBarcodes()
        );

        if (PurchaseId.HasValue)
        {
            SelectedPurchaseId = PurchaseId;
            await OnPurchaseSelected((object)PurchaseId.Value);
        }
    }

    // ── Configuration ────────────────────────────────────────────
    private async Task LoadPrintConfiguration()
    {
        try
        {
            IsLoadingConfig = true;
            PrintConfig = new BarcodePrintConfigDTO
            {
                LabelWidthMm = 50,
                LabelHeightMm = 33,
                FabricRepeatCount = 2
            };
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Config Error",
                $"Failed to load print configuration: {ex.Message}");
        }
        finally { IsLoadingConfig = false; }
    }

    // ── Barcode Templates ────────────────────────────────────────
    private async Task LoadBarcodeTemplates()
    {
        try
        {
            IsLoadingTemplates = true;
            // TODO: wire to real service
            // BarcodeTemplates = (await _serviceUnitOfWork.BarcodeTemplateService.GetAllBarcodeTemplates()).ToList();

            // Stub — mirrors the DB row in the screenshot
            BarcodeTemplates = new List<BarcodeTemplateDTO>
            {
                new() { Id = 1, TemplateName = "33*55", Descriptions = "ProductName, Brand, UOM, Price" }
            };

            if (BarcodeTemplates.Any() && SelectedTemplateId == null)
                OnTemplateSelected(BarcodeTemplates.First().Id);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load barcode templates: {ex.Message}");
        }
        finally { IsLoadingTemplates = false; }
    }

    protected void OnTemplateSelected(object value)
    {
        if (value is int templateId && templateId > 0)
        {
            SelectedTemplateId = templateId;
            SelectedTemplate = BarcodeTemplates.FirstOrDefault(t => t.Id == templateId);

            // Parse label size from TemplateName e.g. "33*55" → height=33, width=55
            if (SelectedTemplate != null)
            {
                var parts = SelectedTemplate.TemplateName
                    .Split(new[] { '*', 'x', 'X', '×' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2
                    && decimal.TryParse(parts[0].Trim(), out var h)
                    && decimal.TryParse(parts[1].Trim(), out var w))
                {
                    PrintConfig.LabelHeightMm = h;
                    PrintConfig.LabelWidthMm = w;
                }

                // Parse visible fields from Descriptions CSV
                TemplateFields = (SelectedTemplate.Descriptions ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim())
                    .ToList();
            }

            // Clear queue so labels re-render with new template
            if (PrintItems.Any())
            {
                PrintItems = new List<BarcodePrintItemDTO>(); 
                notificationService.Notify(NotificationSeverity.Info, "Template Changed",
                    "Print queue cleared. Please re-add items for the new template.");
            }

            StateHasChanged();
        }
    }

    // ── Purchase List ────────────────────────────────────────────
    private async Task LoadPurchaseList()
    {
        try
        {
            IsLoadingPurchases = true;
            // TODO: wire to real service
             PurchaseList = (await _serviceUnitOfWork.PurchaseService.GetPurchasesByDateRange(FromDate, ToDate)).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load purchase list: {ex.Message}");
        }
        finally { IsLoadingPurchases = false; }
    }

    protected async Task OnDateFilterChanged()
    {
        if (FromDate > ToDate)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Date Range",
                "From date cannot be greater than To date.");
            return;
        }
        SelectedPurchaseId = null;
        SelectedPurchase = null;
        await LoadPurchaseList();
    }

    // ── Barcode stripe helper (for any fallback linear barcode display) ──
    protected static IEnumerable<int> GetBarcodeStripeWidths(string barcodeValue)
    {
        int seed = barcodeValue?.Aggregate(0, (h, c) => h * 31 + c) ?? 0;
        var rng = new Random(seed);
        for (int i = 0; i < 28; i++)
            yield return rng.Next(0, 3) == 0 ? 2 : 1;
    }

    // ── QR cell helper — stable pseudo-QR grid from barcode value ──
    protected static IEnumerable<(int X, int Y)> GetQrCells(string barcodeValue)
    {
        int seed = barcodeValue?.Aggregate(0, (h, c) => h * 31 + c) ?? 0;
        var rng = new Random(seed);
        var cells = new List<(int, int)>();

        // Finder patterns (top-left, top-right, bottom-left) — fixed
        int[][] finders = { new[] { 0, 0 }, new[] { 14, 0 }, new[] { 0, 14 } };
        foreach (var f in finders)
        {
            for (int dy = 0; dy < 7; dy++)
                for (int dx = 0; dx < 7; dx++)
                {
                    bool border = dx == 0 || dx == 6 || dy == 0 || dy == 6;
                    bool inner = dx >= 2 && dx <= 4 && dy >= 2 && dy <= 4;
                    if (border || inner) cells.Add((f[0] + dx, f[1] + dy));
                }
        }

        // Data modules
        for (int y = 0; y < 21; y++)
            for (int x = 0; x < 21; x++)
            {
                bool inFinder = (x < 8 && y < 8) || (x > 12 && y < 8) || (x < 8 && y > 12);
                if (!inFinder && rng.Next(2) == 1) cells.Add((x, y));
            }

        return cells;
    }

    // ── All Barcodes (single-add) ────────────────────────────────
    private async Task LoadAllBarcodes()
    {
        try
        {
            IsLoadingBarcodes = true;
            AllBarcodes = (await _serviceUnitOfWork.BarcodePrintConfigService.GetAllItemsForBarcodePrint())?.ToList() ?? new();
            BarcodeSelectItems = AllBarcodes.Select(b => new BarcodeSelectItem
            {
                Id = b.Id,
                ProductName = b.ProductName,
                BarcodeValue = b.BarcodeValue,
                ReturnRefNo = b.ReturnRefNo
            }).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load barcodes: {ex.Message}");
        }
        finally { IsLoadingBarcodes = false; }
    }

    // ── Purchase Selection ───────────────────────────────────────
    protected async Task OnPurchaseSelected(object value)
    {
        if (value is int purchaseId && purchaseId > 0)
        {
            if (SelectedTemplateId == null)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Template Required",
                    "Please select a barcode template first.");
                return;
            }
            SelectedPurchaseId = purchaseId;
            await LoadBarcodesForPurchase(purchaseId);
        }
    }

    private async Task LoadBarcodesForPurchase(int purchaseId)
    {
        try
        {
            var barcodes = await _serviceUnitOfWork.BarcodePrintConfigService
                               .GetBarcodeItemsByPurchaseId(purchaseId);

            foreach (var barcode in barcodes)
            {
                decimal qty = barcode.GroupId?.Equals("FABRIC", StringComparison.OrdinalIgnoreCase) == true
                    ? PrintConfig.FabricRepeatCount
                    : 1;
                AddOrUpdatePrintItem(barcode, qty);
            }

            notificationService.Notify(NotificationSeverity.Info, "Barcodes Loaded",
                $"{barcodes.Count()} barcode(s) added from purchase.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load barcodes: {ex.Message}");
        }
    }

    // ── Single Barcode Add ───────────────────────────────────────
    protected async Task AddSingleBarcode()
    {
        if (SelectedTemplateId == null)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Template Required",
                "Please select a barcode template first.");
            return;
        }
        if (SelectedBarcodeId == null)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                "Please select a barcode first.");
            return;
        }
        if (SinglePrintQty < 1)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation",
                "Print quantity must be at least 1.");
            return;
        }

        var barcode = AllBarcodes.FirstOrDefault(b => b.Id == SelectedBarcodeId);
        if (barcode == null) return;

        AddOrUpdatePrintItemFromItem(barcode, SinglePrintQty);
        notificationService.Notify(NotificationSeverity.Success, "Added",
            $"Barcode {barcode.BarcodeValue} added ({SinglePrintQty}×).");
    }

    // ── Print Item Helpers ───────────────────────────────────────
    private void AddOrUpdatePrintItem(BarcodeItemDTO barcode, decimal qty)
    {
        var existing = PrintItems.FirstOrDefault(p => p.BarcodeId == barcode.Id);
        if (existing != null)
        {
            existing.PrintQty += qty;
            PrintItems = new List<BarcodePrintItemDTO>(PrintItems); // new reference
        }
        else
        {
            PrintItems = new List<BarcodePrintItemDTO>(PrintItems)
        {
            new BarcodePrintItemDTO
            {
                BarcodeId = barcode.Id,
                BarcodeValue = barcode.BarcodeValue,
                ProductName = barcode.ProductName,
                Brand = barcode.BrandName,
                Price = barcode.SalesPrice,
                UoM = barcode.UnitName,
                GroupId = barcode.GroupId,
                GroupName = barcode.GroupName,
                PrintQty = qty,
                LabelWidthMm = PrintConfig.LabelWidthMm,
                LabelHeightMm = PrintConfig.LabelHeightMm,
                TemplateId = SelectedTemplateId ?? 0,
                ReturnRefNo = barcode.ReturnRefNo,
            }
        };
        }
        StateHasChanged();
    }
    private void AddOrUpdatePrintItemFromItem(BarcodeItemDTO barcode, decimal qty)
    {
        var existing = PrintItems.FirstOrDefault(p => p.BarcodeId == barcode.Id);
        if (existing != null)
        {
            existing.PrintQty += qty;
            PrintItems = new List<BarcodePrintItemDTO>(PrintItems); // new reference
        }
        else
        {
            PrintItems = new List<BarcodePrintItemDTO>(PrintItems)
        {
            new BarcodePrintItemDTO
            {
                BarcodeId = barcode.Id,
                BarcodeValue = barcode.BarcodeValue,
                ProductName = barcode.ProductName,
                Brand = barcode.BrandName,
                Price = barcode.SalesPrice,
                UoM = barcode.UnitName,
                GroupId = barcode.GroupId,
                GroupName = barcode.GroupName,
                PrintQty = qty,
                LabelWidthMm = PrintConfig.LabelWidthMm,
                LabelHeightMm = PrintConfig.LabelHeightMm,
                TemplateId = SelectedTemplateId ?? 0,
                ReturnRefNo = barcode.ReturnRefNo,
            }
        };
        }
        StateHasChanged();
    }
    protected void RemovePrintItem(BarcodePrintItemDTO item)
    {
        PrintItems = PrintItems.Where(p => p != item).ToList();
        StateHasChanged();
    }


    protected void UpdateQty(BarcodePrintItemDTO item, decimal qty)
    {
        if (qty < 1) qty = 1;
        item.PrintQty = qty;
        PrintItems = new List<BarcodePrintItemDTO>(PrintItems); // trigger re-render
        StateHasChanged();
    }
    protected void ClearAll()
    {
        PrintItems = new List<BarcodePrintItemDTO>();
        SelectedPurchaseId = null;
        StateHasChanged();
    }

    // ── Print ────────────────────────────────────────────────────
    protected async Task PrintBarcodes()
    {
        if (!PrintItems.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Nothing to Print",
                "Please add barcodes to the print queue first.");
            return;
        }

        try
        {
            IsPrinting = true;

            var printRequest = new BarcodePrintRequestDTO
            {
                Items = PrintItems,
                TemplateId = SelectedTemplateId ?? 0,
                PrintedBy = 1,   // TODO: replace with actual session userId
                PrintedAt = DateTime.Now
            };

            // TODO: Call service → POST /api/barcode/print
            // var result = await _serviceUnitOfWork.BarcodeService.PrintBarcodes(printRequest);

            // TODO: Invoke JS interop to trigger browser/ZPL print
            // await JSRuntime.InvokeVoidAsync("barcodePrint.send", printRequest);

            await Task.Delay(800); // remove once real service is wired

            notificationService.Notify(NotificationSeverity.Success, "Print Sent",
                $"{PrintItems.Sum(x => x.PrintQty)} label(s) sent to printer.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Print Error",
                $"Failed to print: {ex.Message}");
        }
        finally { IsPrinting = false; }
    }
    // ── Download PDF ─────────────────────────────────────────────────────────
    protected async Task DownloadPdfAsync()
    {
        if (!PrintItems.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Nothing to Download",
                "Please add barcodes to the print queue first.");
            return;
        }

        try
        {
            IsDownloadingPdf = true;

            var pdfBytes = _barcodePrintPdfService.GeneratePdf(
                PrintItems,
                TemplateFields,
                35,
                55);

            if (pdfBytes.Length == 0)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Empty PDF",
                    "Generated PDF is empty.");
                return;
            }

            var fileName = $"BarcodeLabels_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JsRuntime.InvokeVoidAsync("downloadFileFromBytes",
                fileName,
                "application/pdf",
                pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "PDF Downloaded",
                $"{PrintItems.Sum(x => x.PrintQty)} label(s) saved as {fileName}");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "PDF Error",
                $"Failed to generate PDF: {ex.Message}");
        }
        finally { IsDownloadingPdf = false; }
    }

    // ── Preview ──────────────────────────────────────────────────────────────
    protected void OpenPreview()
    {
        if (!PrintItems.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Empty Queue",
                "Add items to the queue before opening the preview.");
            return;
        }
        ShowPreview = true;
        StateHasChanged();
    }

    // ── Printer Config Dialog ────────────────────────────────────────────────
    protected void OpenPrinterConfig()
    {
        if (!PrintItems.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "Nothing to Print",
                "Please add barcodes to the print queue first.");
            return;
        }
        ShowPrinterConfig = true;
        StateHasChanged();
    }

    protected async Task OnPrinterConfigClose(bool didPrint)
    {
        ShowPrinterConfig = false;
        if (didPrint)
        {
            // Optionally clear queue after successful print
            // PrintItems.Clear();
        }
        StateHasChanged();
    }

    protected void OnPrinterNotify(string message)
    {
        bool isError = message.StartsWith("❌");
        notificationService.Notify(
            isError ? NotificationSeverity.Error : NotificationSeverity.Success,
            isError ? "Print Error" : "Print",
            message);
    }
}
