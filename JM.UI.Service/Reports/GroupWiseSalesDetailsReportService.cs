using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class GroupWiseSalesDetailsReportService
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

    public byte[] GenerateGroupWiseSalesDetailsReport(
        IEnumerable<GroupWiseSalesDetailsDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<GroupWiseSalesDetailsDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<GroupWiseSalesDetailsDTO>();
        var blocks = BuildBlocks(data);

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
                    ComposeContent(content, blocks, data));

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
            col.Item().AlignCenter().Text("GROUP-WISE SALES DETAILS REPORT").Bold().FontSize(10);

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

    // ── BLOCK GROUPING ────────────────────────────────────────────────────

    private sealed class Block
    {
        public string GroupName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public List<GroupWiseSalesDetailsDTO> Rows { get; set; } = new();
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

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(
        IContainer container,
        List<Block> blocks,
        List<GroupWiseSalesDetailsDTO> data)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(AddHeaderTable);

            if (blocks.Count == 0)
            {
                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    table.Cell().ColumnSpan(7).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No sales records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                });
            }

            foreach (var block in blocks)
                col.Item().Table(table => AddBlockTable(table, block));

            if (data.Any())
                col.Item().Table(table => AddGrandTotalTable(table, data));

            if (data.Any())
            {
                col.Item().PaddingTop(4).Text("*ALL SALES AFTER DISCOUNT, NET SALE TAKA")
                   .FontSize(8).Italic();
            }
        });
    }

    // ── TABLE: HEADER ─────────────────────────────────────────────────────

    private static void AddHeaderTable(TableDescriptor table)
    {
        DefineColumns(table);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Group").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Product Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("SubProduct Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("HLS (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("GEC (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("BTB (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Total (Tk)").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    // ── TABLE: ONE GROUP/PRODUCT BLOCK ────────────────────────────────────

    private static void AddBlockTable(TableDescriptor table, Block block)
    {
        DefineColumns(table);

        var rows = block.Rows;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (i == 0)
            {
                // First row carries the Group and Product labels in a nested
                // two-column table, merged across the block's rows. Relative
                // columns let the nested table flex to the cell's usable width
                // (the outer cell border reduces the space by ~1pt). The cell
                // has no bottom border so the merged area reads as one cell.
                table.Cell().ColumnSpan(2)
                     .BorderTop(0.5f).BorderLeft(0.5f).BorderRight(0.5f).BorderColor(GreyMedium)
                     .AlignMiddle().Table(inner =>
                     {
                         inner.ColumnsDefinition(x =>
                         {
                             x.RelativeColumn(72);
                             x.RelativeColumn(88);
                         });
                         inner.Cell().Padding(4).Text(Dash(block.GroupName)).FontSize(BodyFont);
                         inner.Cell().Padding(4).Text(Dash(block.ProductName)).FontSize(BodyFont);
                     });
            }
            else
            {
                // Continuation of the merged Group/Product cell: left and right
                // borders only, so it reads as a single spanned cell.
                table.Cell().ColumnSpan(2)
                     .BorderLeft(0.5f).BorderRight(0.5f).BorderColor(GreyMedium)
                     .Padding(4);
            }

            table.Cell().Element(GridCell).Text(Dash(row.SubProductName)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.HlsAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.GecAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.BtbAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.TotalAmount)).Bold().FontSize(BodyFont);
        }

        // SubTotal row, spanning the three name columns. Sums the subproduct
        // rows in this block so it can never disagree with them.
        table.Cell().ColumnSpan(3).Background(GreyLighten2).Element(TotalCell)
             .Text("SubTotal").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
             .Text(Fmt(rows.Sum(r => r.HlsAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
             .Text(Fmt(rows.Sum(r => r.GecAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
             .Text(Fmt(rows.Sum(r => r.BtbAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
             .Text(Fmt(rows.Sum(r => r.TotalAmount))).Bold().FontSize(TotalFont);
    }

    // ── TABLE: GRAND TOTAL ────────────────────────────────────────────────

    private static void AddGrandTotalTable(TableDescriptor table, List<GroupWiseSalesDetailsDTO> data)
    {
        DefineColumns(table);

        table.Cell().ColumnSpan(3).Background(GreyLighten3).Element(TotalCell)
             .Text("Grand Total").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(data.Sum(r => r.HlsAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(data.Sum(r => r.GecAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(data.Sum(r => r.BtbAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(data.Sum(r => r.TotalAmount))).Bold().FontSize(TotalFont);
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    private static IContainer GridCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static IContainer TotalCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static string Fmt(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string Dash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 portrait is 595.28pt wide; with 20pt margins the usable width is
        // 555.28pt. These 7 columns total 527pt, leaving headroom so QuestPDF
        // does not raise a conflicting-size-constraints layout error.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(72);   // Group
            c.ConstantColumn(88);   // Product Name
            c.ConstantColumn(104);  // SubProduct Name
            c.ConstantColumn(65);   // HLS (Tk)
            c.ConstantColumn(65);   // GEC (Tk)
            c.ConstantColumn(65);   // BTB (Tk)
            c.ConstantColumn(68);   // Total (Tk)
        });
    }
}
