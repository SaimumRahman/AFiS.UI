using JM.UI.Entities.Model.Stock;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Service.Reports;

public class StockAgingReportPdfService
{
    private const float BodyFont = 7f;

    /// <summary>
    /// Quantity text for a row: two decimals for units that allow fractions
    /// (MTR / YDS …), a whole number otherwise — matching what is stored.
    /// </summary>
    private static string QtyText(decimal qty, string? uomName)
        => qty.ToString(
            JM.UI.Entities.Model.MesurementUnits.UomRules.AllowsDecimalQuantity(uomName) ? "N2" : "N0");

    private static string UomLabel(string? uomName)
        => string.IsNullOrWhiteSpace(uomName) ? "pcs" : uomName.Trim();

    /// <summary>Totals mix measurement units, so trim trailing zeros instead.</summary>
    private static string SumText(decimal qty) => qty.ToString("#,##0.###");

    public byte[] Generate(StockAgingReportDTO report, string storeName = "-", string printedBy = "-", int? onlyStoreId = null)
        => BuildDocument(report, storeName, printedBy, onlyStoreId).GeneratePdf();

    public Document BuildDocument(StockAgingReportDTO report, string storeName = "-", string printedBy = "-", int? onlyStoreId = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var rows = report?.Rows ?? new List<StockAgingRowDTO>();
        var stores = report?.Stores ?? new List<StockAgingStoreDTO>();

        // When the page is scoped to a source branch, print only that branch's
        // column and use it for the row totals, exactly like the on-screen grid.
        var scopedStoreId = onlyStoreId.HasValue && onlyStoreId.Value > 0 ? onlyStoreId : null;
        if (scopedStoreId.HasValue)
            stores = stores.Where(s => s.StoreId == scopedStoreId.Value).ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(16);
                page.DefaultTextStyle(x => x.FontSize(BodyFont).FontFamily("Arial"));

                page.Header().Element(h => ComposeHeader(h, storeName, printedBy, rows.Count, report?.Kpi));
                page.Content().Element(c => ComposeContent(c, rows, stores, scopedStoreId));
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    private void ComposeHeader(IContainer container, string storeName, string printedBy, int count, StockAgingKpiDTO? kpi)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("ASIA FASHION").Bold().FontSize(12);
            col.Item().AlignCenter().Text("Stock Aging Report").Bold().FontSize(10);
            col.Item().PaddingTop(4).Row(r =>
            {
                r.RelativeItem().Text($"Branch: {storeName}");
                r.RelativeItem().AlignRight().Text($"Printed: {DateTime.Now:dd-MMM-yyyy HH:mm}   By: {printedBy}");
            });
            if (kpi != null)
            {
                col.Item().PaddingTop(2).Text($"Transfer SKUs: {kpi.TransferSkuCount} • Stockouts: {kpi.StockoutCount} • Dead Cost: {kpi.DeadCost:N0} • Orphan Designs: {kpi.OrphanDesignCount}").FontSize(7.5f);
            }
            col.Item().PaddingTop(4).LineHorizontal(0.5f);
        });
    }

    private void ComposeContent(IContainer container, List<StockAgingRowDTO> rows, List<StockAgingStoreDTO> stores, int? scopedStoreId = null)
    {
        decimal TotalQtyFor(StockAgingRowDTO row) =>
            scopedStoreId.HasValue
                ? row.Cells.FirstOrDefault(c => c.StoreId == scopedStoreId.Value)?.Qty ?? 0
                : row.TotalQty;

        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(18);
                    cd.RelativeColumn(1.6f);
                    cd.ConstantColumn(55);
                    foreach (var _ in stores) cd.ConstantColumn(38);
                    cd.ConstantColumn(60);
                    cd.ConstantColumn(40);
                    cd.RelativeColumn(1.4f);
                });

                table.Header(h =>
                {
                    h.Cell().Text("#").Bold();
                    h.Cell().Text("Barcode & Product").Bold();
                    h.Cell().Text("Cost/MRP").Bold().AlignRight();
                    foreach (var st in stores) h.Cell().Text(st.StoreCode).Bold().AlignCenter();
                    h.Cell().Text("Total/Cost").Bold().AlignRight();
                    h.Cell().Text("Age").Bold().AlignCenter();
                    h.Cell().Text("Recommended Action").Bold();
                });

                int idx = 0;
                foreach (var row in rows)
                {
                    idx++;
                    table.Cell().Text(idx.ToString());
                    table.Cell().Text($"{row.Barcode}\n{row.ProductName}");
                    table.Cell().Text($"{row.Cost:N0}\n{row.Mrp:N0}").AlignRight();
                    foreach (var st in stores)
                    {
                        var cell = row.Cells.FirstOrDefault(c => c.StoreId == st.StoreId);
                        var qty = cell?.Qty ?? 0;
                        var sold = cell?.Sold30 ?? 0;
                        table.Cell().Text($"{QtyText(qty, row.UomName)}\n{sold:N0} sold").AlignCenter();
                    }
                    table.Cell().Text($"{QtyText(TotalQtyFor(row), row.UomName)} {UomLabel(row.UomName)}\n{TotalQtyFor(row) * row.Cost:N0}").AlignRight();
                    table.Cell().Text($"{row.AgeDays}d").AlignCenter();
                    table.Cell().Text(row.ActionText);
                }

                table.Footer(f =>
                {
                    f.Cell().ColumnSpan(3).Text("Totals").Bold().AlignRight();
                    foreach (var st in stores)
                    {
                        var q = rows.Sum(r => r.Cells.FirstOrDefault(c => c.StoreId == st.StoreId)?.Qty ?? 0);
                        f.Cell().Text(SumText(q)).Bold().AlignCenter();
                    }
                    f.Cell().Text($"{SumText(rows.Sum(TotalQtyFor))} units\n{rows.Sum(r => TotalQtyFor(r) * r.Cost):N0}").Bold().AlignRight();
                    f.Cell().Text("").Bold();
                    f.Cell().Text("").Bold();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.Span("Page ").FontSize(8);
            t.CurrentPageNumber().FontSize(8);
            t.Span(" of ").FontSize(8);
            t.TotalPages().FontSize(8);
        });
    }
}