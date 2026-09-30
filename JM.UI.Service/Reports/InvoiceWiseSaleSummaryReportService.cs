using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class InvoiceWiseSaleSummaryReportService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 9f;
    private const float BodyFont = 9f;
    private const float TotalFont = 9.5f;

    public byte[] GenerateInvoiceWiseSaleSummaryReport(
        IEnumerable<InvoiceWiseSaleSummaryDTO> rows,
        string storeName = "ASIA FASHION TAILOR & FABRICS",
        string storeAddress = "Hossain Plaza (1st Floor) bandartila, Chattogram.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<InvoiceWiseSaleSummaryDTO> rows,
        string storeName = "ASIA FASHION TAILOR & FABRICS",
        string storeAddress = "Hossain Plaza (1st Floor) bandartila, Chattogram.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<InvoiceWiseSaleSummaryDTO>();
        var groups = BuildGroups(data);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(header =>
                    ComposeHeader(header, storeName, storeAddress, dateFrom, dateTo));

                page.Content().Element(content =>
                    ComposeContent(content, groups, data));

                page.Footer().Element(ComposeFooter);
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
            col.Item().AlignCenter().Text("INVOICE WISE SALE SUMMARY REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:MMM d, yyyy} To: {dateTo:MMM d, yyyy}"
                : "ALL TIME";

            col.Item().AlignCenter().Text(range).FontSize(9);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    // ── FOOTER ────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(0.5f).BorderColor(GreyMedium).PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(7).FontColor(GreyDarken1));
                text.Span("Print Date: ").Bold();
                text.Span(DateTime.Now.ToString("MM/dd/yyyy"));
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

    // ── GROUPING ──────────────────────────────────────────────────────────

    private sealed class Group
    {
        public string UserName { get; set; } = string.Empty;
        public List<InvoiceWiseSaleSummaryDTO> Rows { get; set; } = new();
    }

    private static List<Group> BuildGroups(List<InvoiceWiseSaleSummaryDTO> data)
    {
        var groups = new List<Group>();

        foreach (var row in data)
        {
            var key = row.UserName ?? string.Empty;

            if (groups.Count == 0 || groups[^1].UserName != key)
                groups.Add(new Group { UserName = key });

            groups[^1].Rows.Add(row);
        }

        return groups;
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(
        IContainer container,
        List<Group> groups,
        List<InvoiceWiseSaleSummaryDTO> data)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(AddHeaderTable);

            if (groups.Count == 0)
            {
                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    table.Cell().ColumnSpan(9).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No sales records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                });
            }

            foreach (var group in groups)
                col.Item().Table(table => AddGroupTable(table, group));

            if (data.Any())
                col.Item().Table(table => AddGrandTotalTable(table, data));
        });
    }

    // ── HEADER TABLE ──────────────────────────────────────────────────────

    private static void AddHeaderTable(TableDescriptor table)
    {
        DefineColumns(table);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Date").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Invoice").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Gross Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Disc. Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Vat Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Net Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Cash Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("CARD Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("DUE Amount").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    // ── ONE USER GROUP ────────────────────────────────────────────────────

    private static void AddGroupTable(TableDescriptor table, Group group)
    {
        DefineColumns(table);

        // User name banner, spanning the full width.
        table.Cell().ColumnSpan(9).Background(GreyLighten2)
             .Border(0.5f).BorderColor(GreyMedium).Padding(4)
             .Text(Dash(group.UserName)).Bold().FontSize(BodyFont);

        foreach (var row in group.Rows)
        {
            table.Cell().Element(GridCell).Text(row.SaleDate.ToString("dd-MMM-yy")).FontSize(BodyFont);
            table.Cell().Element(GridCell).Text(Dash(row.InvoiceNo)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.GrossAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.DiscAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.VatAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.NetAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.CashAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.CardAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.DueAmount)).Bold().FontSize(BodyFont);
        }

        // Sub Total row, spanning the Date + Invoice columns. Sums the invoice
        // rows in this group so it can never disagree with them.
        table.Cell().ColumnSpan(2).Background(GreyLighten2).Element(TotalCell)
             .Text("Sub Total").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.DiscAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.VatAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.NetAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.CashAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.CardAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.DueAmount))).Bold().FontSize(TotalFont);
    }

    // ── GRAND TOTAL TABLE ─────────────────────────────────────────────────

    private static void AddGrandTotalTable(TableDescriptor table, List<InvoiceWiseSaleSummaryDTO> data)
    {
        DefineColumns(table);

        table.Cell().ColumnSpan(2).Background(GreyLighten3).Element(TotalCell)
             .Text("Grand Total").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.DiscAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.VatAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.NetAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.CashAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.CardAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.DueAmount))).Bold().FontSize(TotalFont);
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
        // A4 landscape is 841.89pt wide; with 20pt margins the usable width is
        // 801.89pt. These 9 columns total 740pt, leaving headroom so QuestPDF
        // does not raise a conflicting-size-constraints layout error.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(68);   // Date
            c.ConstantColumn(112);  // Invoice
            c.ConstantColumn(82);   // Gross Amount
            c.ConstantColumn(82);   // Disc. Amount
            c.ConstantColumn(64);   // Vat Amount
            c.ConstantColumn(84);   // Net Amount
            c.ConstantColumn(84);   // Cash Amount
            c.ConstantColumn(84);   // CARD Amount
            c.ConstantColumn(80);   // DUE Amount
        });
    }
}
