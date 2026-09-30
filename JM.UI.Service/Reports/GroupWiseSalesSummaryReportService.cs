using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class GroupWiseSalesSummaryReportService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 9f;
    private const float BodyFont = 9f;
    private const float TotalFont = 9.5f;

    public byte[] GenerateGroupWiseSalesSummaryReport(
        IEnumerable<GroupWiseSalesSummaryDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<GroupWiseSalesSummaryDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<GroupWiseSalesSummaryDTO>();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

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
            // "SUMMERY" is kept exactly as it appears in the source report.
            col.Item().AlignCenter().Text("GROUP-WISE SALES SUMMERY REPORT").Bold().FontSize(10);

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

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, List<GroupWiseSalesSummaryDTO> data)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(table =>
            {
                DefineColumns(table);
                AddTableHeader(table);

                if (!data.Any())
                {
                    // Keep the 5 column grid intact on an empty report.
                    table.Cell().ColumnSpan(5).Element(c => GridCell(c)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No sales records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                }

                foreach (var row in data)
                {
                    table.Cell().Element(c => GridCell(c)
                        .Text(string.IsNullOrWhiteSpace(row.GroupName) ? "-" : row.GroupName)
                        .FontSize(BodyFont));

                    table.Cell().Element(c => GridCell(c)
                        .AlignRight().Text(FormatAmount(row.HlsAmount)).FontSize(BodyFont));
                    table.Cell().Element(c => GridCell(c)
                        .AlignRight().Text(FormatAmount(row.GecAmount)).FontSize(BodyFont));
                    table.Cell().Element(c => GridCell(c)
                        .AlignRight().Text(FormatAmount(row.BtbAmount)).FontSize(BodyFont));
                    table.Cell().Element(c => GridCell(c)
                        .AlignRight().Text(FormatAmount(row.TotalAmount)).Bold().FontSize(BodyFont));
                }

                // Grand Total is summed from the group rows above it. The original
                // report double counted the first two groups in every column; this
                // recomputes from the rows so the total always matches them.
                if (data.Any())
                {
                    table.Cell().Element(c => TotalCell(c)
                        .Text("Grand Total").Bold().FontSize(TotalFont));
                    table.Cell().Element(c => TotalCell(c)
                        .AlignRight().Text(FormatAmount(data.Sum(r => r.HlsAmount))).Bold().FontSize(TotalFont));
                    table.Cell().Element(c => TotalCell(c)
                        .AlignRight().Text(FormatAmount(data.Sum(r => r.GecAmount))).Bold().FontSize(TotalFont));
                    table.Cell().Element(c => TotalCell(c)
                        .AlignRight().Text(FormatAmount(data.Sum(r => r.BtbAmount))).Bold().FontSize(TotalFont));
                    table.Cell().Element(c => TotalCell(c)
                        .AlignRight().Text(FormatAmount(data.Sum(r => r.TotalAmount))).Bold().FontSize(TotalFont));
                }
            });

            if (data.Any())
            {
                col.Item().PaddingTop(4).Text("*All Sales are NET (After Discount)")
                   .FontSize(8).Italic();
            }
        });
    }

    // ── FORMATTING ────────────────────────────────────────────────────────

    private static string FormatAmount(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static IContainer GridCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static IContainer TotalCell(IContainer c)
        => c.Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 portrait is 595.28pt wide; with 20pt margins on each side the usable
        // width is 555.28pt. These 5 columns total 535pt, leaving headroom so
        // QuestPDF does not raise a conflicting-size-constraints layout error.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(165);  // Group Name
            c.ConstantColumn(92);   // HLS (Tk)
            c.ConstantColumn(92);   // GEC (Tk)
            c.ConstantColumn(92);   // BTB (Tk)
            c.ConstantColumn(94);   // Total (Tk)
        });
    }

    private static void AddTableHeader(TableDescriptor table)
    {
        table.Cell().Background(GreyLighten3).Element(c => c.Border(0.5f).BorderColor(GreyMedium).Padding(4))
             .Text("Group Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(c => c.Border(0.5f).BorderColor(GreyMedium).Padding(4))
             .AlignRight().Text("HLS (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(c => c.Border(0.5f).BorderColor(GreyMedium).Padding(4))
             .AlignRight().Text("GEC (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(c => c.Border(0.5f).BorderColor(GreyMedium).Padding(4))
             .AlignRight().Text("BTB (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(c => c.Border(0.5f).BorderColor(GreyMedium).Padding(4))
             .AlignRight().Text("Total (Tk)").Bold().FontSize(HeaderFont);
    }
}
