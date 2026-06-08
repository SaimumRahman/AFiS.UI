// BarcodePrintPreview.razor.cs — Code-behind for the print preview overlay
using JM.UI.Entities.Model.Barcodes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JM.UI.Client.Pages.Barcode;

public partial class BarcodePrintPreview : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ── Parameters ───────────────────────────────────────────────
    [Parameter] public List<BarcodePrintItemDTO> Items { get; set; } = new();
    [Parameter] public List<string> TemplateFields { get; set; } = new();
    [Parameter] public string? TemplateName { get; set; }
    [Parameter] public decimal LabelWidthMm { get; set; } = 55;
    [Parameter] public decimal LabelHeightMm { get; set; } = 33;
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    // ── Zoom ─────────────────────────────────────────────────────
    private double _zoom = 1.0;
    protected string ZoomScale => _zoom.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
    protected int ZoomPct => (int)(_zoom * 100);

    protected void ZoomIn() { _zoom = Math.Min(2.0, _zoom + 0.1); }
    protected void ZoomOut() { _zoom = Math.Max(0.4, _zoom - 0.1); }
    protected void ResetZoom() { _zoom = 1.0; }

    // ── Computed ─────────────────────────────────────────────────
    protected int TotalLabels => Items.Sum(x => x.PrintQty);

    // ── Field Visibility ─────────────────────────────────────────
    protected bool ShowField(string fieldName) =>
        TemplateFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);

    // ── Actions ──────────────────────────────────────────────────
    protected async Task TriggerBrowserPrint()
    {
        await JS.InvokeVoidAsync("window.print");
    }

    protected async Task Close()
    {
        _zoom = 1.0;
        await OnClose.InvokeAsync();
    }

    // ── QR cell helper (reused from parent) ──────────────────────
    protected static IEnumerable<(int X, int Y)> GetQrCells(string barcodeValue)
    {
        int seed = barcodeValue?.Aggregate(0, (h, c) => h * 31 + c) ?? 0;
        var rng = new Random(seed);
        var cells = new List<(int, int)>();

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

        for (int y = 0; y < 21; y++)
            for (int x = 0; x < 21; x++)
            {
                bool inFinder = (x < 8 && y < 8) || (x > 12 && y < 8) || (x < 8 && y > 12);
                if (!inFinder && rng.Next(2) == 1) cells.Add((x, y));
            }

        return cells;
    }
}