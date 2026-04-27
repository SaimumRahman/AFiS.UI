using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Barcode;

public partial class BarcodePrintComponent : PosComponentBase
{
    private const double ScaleFactor = 2.2;
    protected int LabelWidthPx => (int)(PrintConfig.LabelWidthMm * Convert.ToDecimal(ScaleFactor));
    protected int LabelHeightPx => (int)(PrintConfig.LabelHeightMm * Convert.ToDecimal(ScaleFactor));
    protected int BarHeightPx => (int)(LabelHeightPx * 0.30);
    protected int NameFontSizePx => Math.Max(7, (int)(LabelHeightPx * 0.14));
    protected int BarcodeFontSizePx => Math.Max(5, (int)(LabelHeightPx * 0.09));
    protected int SmallFontPx => Math.Max(5, (int)(LabelHeightPx * 0.09));
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    // ── Filter State ────────────────────────────────────────────
    protected DateTime FromDate { get; set; } = DateTime.Today;
    protected DateTime ToDate { get; set; } = DateTime.Today;

    // ── Purchase Dropdown ────────────────────────────────────────
    protected List<PurchaseNoDTO> PurchaseList { get; set; } = new();
    protected int? SelectedPurchaseId { get; set; }
    protected PurchaseNoDTO? SelectedPurchase { get; set; }

    // ── Single-Barcode Dropdown ──────────────────────────────────
    protected List<BarcodeItemDTO> AllBarcodes { get; set; } = new();
    protected int? SelectedBarcodeId { get; set; }
    protected int SinglePrintQty { get; set; } = 1;

    // ── Print Preview List ───────────────────────────────────────
    protected List<BarcodePrintItemDTO> PrintItems { get; set; } = new();

    // ── Configuration (from DB) ──────────────────────────────────
    protected BarcodePrintConfigDTO PrintConfig { get; set; } = new();

    // ── UI State ─────────────────────────────────────────────────
    protected bool IsLoadingPurchases { get; set; } = false;
    protected bool IsLoadingBarcodes { get; set; } = false;
    protected bool IsPrinting { get; set; } = false;
    protected bool IsLoadingConfig { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        // Load config and initial data in parallel
        await Task.WhenAll(
            LoadPrintConfiguration(),
            LoadPurchaseList(),
            LoadAllBarcodes()
        );
    }

    // ── Configuration ────────────────────────────────────────────

    private async Task LoadPrintConfiguration()
    {
        try
        {
            IsLoadingConfig = true;
            // TODO: Call service → GET /api/barcode/print-config
            // PrintConfig = await _serviceUnitOfWork.BarcodeService.GetPrintConfiguration();

            // Stub until wired:
            PrintConfig = new BarcodePrintConfigDTO
            {
                LabelWidthMm = 50,
                LabelHeightMm = 30,
                FabricRepeatCount = 2
            };
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Config Error",
                $"Failed to load print configuration: {ex.Message}");
        }
        finally
        {
            IsLoadingConfig = false;
        }
    }

    // ── Purchase List ────────────────────────────────────────────

    private async Task LoadPurchaseList()
    {
        try
        {
            IsLoadingPurchases = true;
            // TODO: Call service → GET /api/purchase/list?from={FromDate}&to={ToDate}
            // PurchaseList = await _serviceUnitOfWork.PurchaseService.GetPurchaseNosByDateRange(FromDate, ToDate);

            // Stub:
            PurchaseList = new List<PurchaseNoDTO>
            {
                new() { Id = 1, PurchaseNo = "PO-2025-001", PurchaseDate = DateTime.Today, ItemCount = 5  },
                new() { Id = 2, PurchaseNo = "PO-2025-002", PurchaseDate = DateTime.Today, ItemCount = 12 },
            };
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load purchase list: {ex.Message}");
        }
        finally
        {
            IsLoadingPurchases = false;
        }
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
    protected static IEnumerable<int> GetBarcodeStripeWidths(string barcodeValue)
    {
        // seed a stable hash so the same barcode always renders identically
        int seed = barcodeValue?.Aggregate(0, (h, c) => h * 31 + c) ?? 0;
        var rng = new Random(seed);
        int totalBars = 28; // number of bars
        for (int i = 0; i < totalBars; i++)
            yield return rng.Next(0, 3) == 0 ? 2 : 1; // mostly thin bars, occasional wide
    }
    // ── Barcode Dropdown (all barcodes for single-add) ───────────

    private async Task LoadAllBarcodes()
    {
        try
        {
            IsLoadingBarcodes = true;
            // TODO: Call service → GET /api/barcode/all
            // AllBarcodes = await _serviceUnitOfWork.BarcodeService.GetAllBarcodes();

            AllBarcodes = new List<BarcodeItemDTO>
            {
                new() { Id = 1, BarcodeValue = "BC-10001", ProductName = "Cotton Fabric Roll",  GroupId = "FABRIC", GroupName = "Fabric" },
                new() { Id = 2, BarcodeValue = "BC-10002", ProductName = "Blue Denim Jeans",    GroupId = "GARMENT",GroupName = "Garment"},
                new() { Id = 3, BarcodeValue = "BC-10003", ProductName = "Polyester Thread",    GroupId = "FABRIC", GroupName = "Fabric" },
            };
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error",
                $"Failed to load barcodes: {ex.Message}");
        }
        finally
        {
            IsLoadingBarcodes = false;
        }
    }

    // ── Purchase Selection ───────────────────────────────────────

    protected async Task OnPurchaseSelected(object value)
    {
        if (value is int purchaseId && purchaseId > 0)
        {
            SelectedPurchaseId = purchaseId;
            await LoadBarcodesForPurchase(purchaseId);
        }
    }

    private async Task LoadBarcodesForPurchase(int purchaseId)
    {
        try
        {
            // TODO: Call service → GET /api/purchase/{purchaseId}/barcodes
            // var barcodes = await _serviceUnitOfWork.BarcodeService.GetBarcodesByPurchaseId(purchaseId);

            // Stub response:
            var barcodes = new List<BarcodeItemDTO>
            {
                new() { Id = 1, BarcodeValue = "BC-10001", ProductName = "Cotton Fabric Roll", GroupId = "FABRIC",  GroupName = "Fabric"  },
                new() { Id = 2, BarcodeValue = "BC-10002", ProductName = "Blue Denim Jeans",   GroupId = "GARMENT", GroupName = "Garment" },
            };

            foreach (var barcode in barcodes)
            {
                // Fabric group → repeat count from config; others → 1
                int qty = barcode.GroupId?.Equals("FABRIC", StringComparison.OrdinalIgnoreCase) == true
                    ? PrintConfig.FabricRepeatCount
                    : 1;

                AddOrUpdatePrintItem(barcode, qty);
            }

            notificationService.Notify(NotificationSeverity.Info, "Barcodes Loaded",
                $"{barcodes.Count} barcode(s) added from purchase.");
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

        // TODO: Optionally fetch barcode details if not already in AllBarcodes
        var barcode = AllBarcodes.FirstOrDefault(b => b.Id == SelectedBarcodeId);
        if (barcode == null) return;

        AddOrUpdatePrintItem(barcode, SinglePrintQty);
        notificationService.Notify(NotificationSeverity.Success, "Added",
            $"Barcode {barcode.BarcodeValue} added ({SinglePrintQty}×).");
    }

    // ── Print Item Helpers ───────────────────────────────────────

    private void AddOrUpdatePrintItem(BarcodeItemDTO barcode, int qty)
    {
        var existing = PrintItems.FirstOrDefault(p => p.BarcodeId == barcode.Id);
        if (existing != null)
        {
            existing.PrintQty += qty;
        }
        else
        {
            PrintItems.Add(new BarcodePrintItemDTO
            {
                BarcodeId = barcode.Id,
                BarcodeValue = barcode.BarcodeValue,
                ProductName = barcode.ProductName,
                GroupId = barcode.GroupId,
                GroupName = barcode.GroupName,
                PrintQty = qty,
                LabelWidthMm = PrintConfig.LabelWidthMm,
                LabelHeightMm = PrintConfig.LabelHeightMm
            });
        }
        StateHasChanged();
    }

    protected void RemovePrintItem(BarcodePrintItemDTO item)
    {
        PrintItems.Remove(item);
        StateHasChanged();
    }

    protected void UpdateQty(BarcodePrintItemDTO item, int qty)
    {
        if (qty < 1) qty = 1;
        item.PrintQty = qty;
        StateHasChanged();
    }

    protected void ClearAll()
    {
        PrintItems.Clear();
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
                PrintedBy = 1, // TODO: replace with actual session userId
                PrintedAt = DateTime.Now
            };

            // TODO: Call service → POST /api/barcode/print
            // var result = await _serviceUnitOfWork.BarcodeService.PrintBarcodes(printRequest);
            // if (!result.IsSuccessStatus) { ... error ... return; }

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
        finally
        {
            IsPrinting = false;
        }
    }
}

public class PurchaseNoDTO
{
    public int Id { get; set; }
    public string PurchaseNo { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public int ItemCount { get; set; }
}
public class BarcodeItemDTO
{
    public int Id { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string DisplayLabel => $"{BarcodeValue}  —  {ProductName} ({GroupName})";
}
public class BarcodePrintConfigDTO
{
    public decimal LabelWidthMm { get; set; } = 50;
    public decimal LabelHeightMm { get; set; } = 30;
    public int FabricRepeatCount { get; set; } = 2;
}
public class BarcodePrintItemDTO
{
    public int BarcodeId { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int PrintQty { get; set; } = 1;
    public decimal LabelWidthMm { get; set; }
    public decimal LabelHeightMm { get; set; }
}

public class BarcodePrintRequestDTO
{
    public List<BarcodePrintItemDTO> Items { get; set; } = new();
    public int? PrintedBy { get; set; }
    public DateTime PrintedAt { get; set; } = DateTime.Now;
}