using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class ExchangeReportService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 7.5f;
    private const float BodyFont = 7.5f;
    private const float SubFont = 6.5f;
    private const float TotalFont = 8f;

    public byte[] GenerateExchangeReport(
        IEnumerable<ExchangeReportDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<ExchangeReportDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<ExchangeReportDTO>();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(header =>
                    ComposeHeader(header, storeName, storeAddress, dateFrom, dateTo));

                page.Content().Element(content =>
                    ComposeContent(content, data));

                page.Footer().Element(footer =>
                    ComposeFooter(footer, printedBy));
            });
        });
    }

    // ── HEADER ────────────────────────────────────────────────────────────

    private void ComposeHeader(
        IContainer container,
        string storeName,
        string storeAddress,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text(storeName).Bold().FontSize(13);
            col.Item().AlignCenter().Text(storeAddress).FontSize(9);
            col.Item().AlignCenter().Text("EXCHANGE REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy} TO {dateTo:dd-MMM-yyyy}".ToUpperInvariant()
                : "ALL TIME";

            col.Item().AlignCenter().Text(range).FontSize(9);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    // ── FOOTER ────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer container, string printedBy)
    {
        container.BorderTop(0.5f).BorderColor(GreyMedium).PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7).FontColor(GreyDarken1));
                text.Span("Print Date & Time: ").Bold();
                text.Span($"{DateTime.Now:dd-MMM-yyyy hh.mmtt}");
            });

            row.RelativeItem().AlignCenter().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7).FontColor(GreyDarken1));
                text.Span("Printed by: ").Bold();
                text.Span(string.IsNullOrWhiteSpace(printedBy) ? "-" : printedBy);
            });

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7).FontColor(GreyDarken1));
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    // ── CONTENT: one table row per exchange, items stacked inside ─────────

    private void ComposeContent(IContainer container, List<ExchangeReportDTO> data)
    {
        container.PaddingTop(4).Column(col =>
        {
            decimal grandOld = 0, grandNew = 0, grandAdj = 0, grandPaid = 0;

            // The API returns one row per stacking position; collapse those back
            // into a single printed row per exchange.
            var exchanges = data
                .GroupBy(r => r.SaleMasterId)
                .OrderByDescending(g => g.First().ExchangeDate)
                .ThenByDescending(g => g.Key)
                .ToList();

            if (!exchanges.Any())
            {
                col.Item().AlignCenter().PaddingTop(20)
                   .Text("No exchange records found for the selected filters.")
                   .FontColor(GreyDarken1);
                return;
            }

            col.Item().Table(table =>
            {
                DefineColumns(table);
                AddTableHeader(table);

                foreach (var exchangeGroup in exchanges)
                {
                    var head = exchangeGroup.First();
                    var slots = exchangeGroup.OrderBy(r => r.Position).ToList();

                    grandOld += head.OldTotalValue;
                    grandNew += head.NewTotalValue;
                    grandAdj += head.ExchangeAdjustment;
                    grandPaid += head.PaidAmount;

                    IContainer CellStyle(IContainer c)
                        => c.BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2).AlignTop();

                    // ── 1. New Invoice No ──
                    table.Cell().Element(c => CellStyle(c)
                        .Text(Dash(head.NewInvoiceNo)));

                    // ── 2. Exchange Date (05-Jun-25) ──
                    table.Cell().Element(c => CellStyle(c)
                        .Text(FormatExchangeDate(head.ExchangeDate)));

                    // ── 3. Old Invoice No ──
                    table.Cell().Element(c => CellStyle(c)
                        .Text(Dash(head.OldInvoiceNo)));

                    // ── 4. Customer name on one line, phone below ──
                    table.Cell().Element(c => CellStyle(c).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(BodyFont));
                        text.Span(Dash(head.CustomerName)).Bold();
                        text.Line(Dash(head.CustomerPhone)).FontColor(GreyDarken1);
                    }));

                    // ── 5/6. Old items and their prices, stacked line for line ──
                    table.Cell().Element(c => CellStyle(c).Element(inner =>
                        StackedItemCell(inner, slots, oldSide: true)));

                    table.Cell().Element(c => CellStyle(c).Element(inner =>
                        StackedPriceCell(inner, slots, oldSide: true)));

                    // ── 7/8. New items and their prices, stacked line for line ──
                    table.Cell().Element(c => CellStyle(c).Element(inner =>
                        StackedItemCell(inner, slots, oldSide: false)));

                    table.Cell().Element(c => CellStyle(c).Element(inner =>
                        StackedPriceCell(inner, slots, oldSide: false)));

                    // ── 9. Exchange Adjustment ──
                    table.Cell().Element(c => CellStyle(c)
                        .AlignRight().Text(FormatAmount(head.ExchangeAdjustment)));

                    // ── 10. Paid Amount ──
                    table.Cell().Element(c => CellStyle(c)
                        .AlignRight().Text(FormatAmount(head.PaidAmount)));

                    // ── 11. Handled By ──
                    table.Cell().Element(c => CellStyle(c)
                        .Text(Dash(head.HandledBy)));
                }
            });

            // ── GRAND TOTAL ──
            col.Item().PaddingTop(4).Table(table =>
            {
                DefineColumns(table);
                AddTotalRow(table, "GRAND TOTAL", grandOld, grandNew, grandAdj, grandPaid,
                            GreyLighten2, TotalFont + 0.5f);
            });
        });
    }

    // ── STACKED CELLS ─────────────────────────────────────────────────────
    //
    // The item and price cells must stay aligned line-for-line, so each visual
    // line is emitted as its own single-line clamped text element. Without the
    // clamp a long item name wraps onto a second visual line and pushes every
    // following price up against the wrong item.
    //
    // Line 1 uses BodyFont and line 2 uses SubFont on BOTH sides, so the row
    // heights of the item cell and the price cell match exactly and cannot
    // drift apart over a tall multi-item exchange.

    private static void StackedItemCell(
        IContainer container,
        List<ExchangeReportDTO> slots,
        bool oldSide)
    {
        container.Column(col =>
        {
            foreach (var slot in slots)
            {
                string code = oldSide ? Dash(slot.OldItemCode) : Dash(slot.NewItemCode);
                string name = oldSide ? Dash(slot.OldItemName) : Dash(slot.NewItemName);

                col.Item().Text(text =>
                {
                    text.ClampLines(1, "...");
                    text.Span(code).FontSize(BodyFont);
                });

                col.Item().Text(text =>
                {
                    text.ClampLines(1, "...");
                    text.Span(name).FontSize(SubFont).FontColor(GreyDarken1);
                });
            }
        });
    }

    private static void StackedPriceCell(
        IContainer container,
        List<ExchangeReportDTO> slots,
        bool oldSide)
    {
        container.Column(col =>
        {
            foreach (var slot in slots)
            {
                decimal value = oldSide ? slot.OldValue : slot.NewValue;
                string detail = oldSide
                    ? FormatQtyRate(slot.OldQty, slot.OldRate, slot.OldUnit)
                    : FormatQtyRate(slot.NewQty, slot.NewRate, slot.NewUnit);

                // A blank side has no qty x rate text. Emit a non-breaking space
                // instead so the line still occupies its height and the matching
                // item name beside it stays on the same baseline.
                if (string.IsNullOrEmpty(detail))
                    detail = "\u00A0";

                col.Item().AlignRight().Text(text =>
                {
                    text.ClampLines(1, "...");
                    text.Span(FormatAmount(value)).FontSize(BodyFont);
                });

                col.Item().AlignRight().Text(text =>
                {
                    text.ClampLines(1, "...");
                    text.Span(detail).FontSize(SubFont).FontColor(GreyDarken1);
                });
            }
        });
    }

    // ── FORMATTING ────────────────────────────────────────────────────────

    private static string Dash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static string FormatAmount(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    // e.g. "3 x 2,520.00 Yards" — the small second line under each price.
    private static string FormatQtyRate(decimal qty, decimal rate, string? unit)
    {
        if (qty == 0 && rate == 0)
            return "";

        string qtyText = qty.ToString("0.##", CultureInfo.InvariantCulture);
        string rateText = rate.ToString("N2", CultureInfo.InvariantCulture);
        string text = $"{qtyText} x {rateText}";

        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    // e.g. 05-Jun-25
    private static string FormatExchangeDate(DateTime value)
        => value.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 landscape is 841.89pt wide; with 20pt margins on each side the usable
        // width is 801.89pt. These 11 columns total 762pt, leaving headroom so
        // QuestPDF does not raise a conflicting-size-constraints layout error.
        // The two item columns are the widest because the stacked item cells hold
        // "code" plus "item name"; long names are clamped to a single line with an
        // ellipsis so they cannot break the alignment with the price column.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(70);   // New Invoice No
            c.ConstantColumn(54);   // Exchange Date
            c.ConstantColumn(70);   // Old Invoice No
            c.ConstantColumn(88);   // Customer Name
            c.ConstantColumn(100);  // Old Item Code
            c.ConstantColumn(60);   // Old Item Price
            c.ConstantColumn(100);  // New Item Code
            c.ConstantColumn(60);   // New Price
            c.ConstantColumn(58);   // Exchange Adjustment
            c.ConstantColumn(54);   // Paid Amount
            c.ConstantColumn(48);   // Handled By
        });
    }

    private static void AddTableHeader(TableDescriptor table)
    {
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("New Invoice No").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Exchange Date").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Old Invoice No").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Customer Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Old Item Code").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Old Item Price").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("New Item Code").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("New Price").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Exchange Adjustment").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Paid Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Handled By").Bold().FontSize(HeaderFont);
    }

    private static void AddTotalRow(
        TableDescriptor table,
        string label,
        decimal oldTotal,
        decimal newTotal,
        decimal adjustment,
        decimal paid,
        string background,
        float fontSize)
    {
        IContainer TotalCell(IContainer c)
            => c.Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2);

        // Label spans New Invoice No → Old Item Code (5 columns).
        table.Cell().ColumnSpan(5).Element(c => TotalCell(c)
            .AlignRight().Text(label).Bold().FontSize(fontSize));

        // Old Item Price total
        table.Cell().Element(c => TotalCell(c)
            .AlignRight().Text(FormatAmount(oldTotal)).Bold().FontSize(fontSize));

        // New Item Code — intentionally blank
        table.Cell().Element(c => TotalCell(c).Text(""));

        // New Price total
        table.Cell().Element(c => TotalCell(c)
            .AlignRight().Text(FormatAmount(newTotal)).Bold().FontSize(fontSize));

        // Exchange Adjustment total
        table.Cell().Element(c => TotalCell(c)
            .AlignRight().Text(FormatAmount(adjustment)).Bold().FontSize(fontSize));

        // Paid Amount total
        table.Cell().Element(c => TotalCell(c)
            .AlignRight().Text(FormatAmount(paid)).Bold().FontSize(fontSize));

        // Handled By — intentionally blank
        table.Cell().Element(c => TotalCell(c).Text(""));
    }
}
