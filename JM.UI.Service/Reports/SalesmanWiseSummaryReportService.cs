using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class SalesmanWiseSummaryReportService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 8.5f;
    private const float BodyFont = 8.5f;
    private const float TotalFont = 9f;

    public byte[] GenerateSalesmanWiseSummaryReport(
        IEnumerable<SalesmanWiseSummaryDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<SalesmanWiseSummaryDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<SalesmanWiseSummaryDTO>();
        var salesmen = BuildSalesmen(data);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial"));

                page.Header().Element(header =>
                    ComposeHeader(header, storeName, storeAddress, dateFrom, dateTo));

                page.Content().Element(content =>
                    ComposeContent(content, salesmen));

                page.Footer().Element(footer => ComposeFooter(footer, printedBy));
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
            col.Item().AlignCenter().Text("SALESMAN-WISE SUMMARY REPORT").Bold().FontSize(10);

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

    // ── GROUPING ──────────────────────────────────────────────────────────

    private sealed class Row
    {
        public string ProductSold { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal NetPaid { get; set; }
    }

    private sealed class DateGroup
    {
        public DateTime Date { get; set; }
        public bool IsNoSale { get; set; }
        public List<Row> Rows { get; set; } = new();
    }

    private sealed class Salesman
    {
        public string Name { get; set; } = string.Empty;
        public List<DateGroup> Dates { get; set; } = new();
    }

    private static List<Salesman> BuildSalesmen(List<SalesmanWiseSummaryDTO> data)
    {
        var distinctDates = data.Select(d => d.SaleDate.Date).Distinct().OrderBy(d => d).ToList();
        var names = data.Select(d => d.SalesmanName ?? string.Empty).Distinct().OrderBy(n => n).ToList();

        var salesmen = new List<Salesman>();

        foreach (var name in names)
        {
            var salesman = new Salesman { Name = name };

            foreach (var date in distinctDates)
            {
                var rows = data
                    .Where(d => (d.SalesmanName ?? string.Empty) == name && d.SaleDate.Date == date)
                    .Select(d => new Row
                    {
                        ProductSold = d.ProductSold,
                        Quantity = d.Quantity,
                        GrossAmount = d.GrossAmount,
                        DiscAmt = d.DiscAmt,
                        ExchangeAmt = d.ExchangeAmt,
                        TotalAmt = d.TotalAmt,
                        NetPaid = d.NetPaid
                    })
                    .ToList();

                salesman.Dates.Add(new DateGroup { Date = date, IsNoSale = rows.Count == 0, Rows = rows });
            }

            salesmen.Add(salesman);
        }

        return salesmen;
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, List<Salesman> salesmen)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(AddHeaderTable);

            if (salesmen.Count == 0)
            {
                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    table.Cell().ColumnSpan(10).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No sales records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                });
            }

            foreach (var salesman in salesmen)
                col.Item().Table(table => AddSalesmanTable(table, salesman));

            if (salesmen.Count > 0)
                col.Item().Table(table => AddGrandTotalTable(table, salesmen));
        });
    }

    private static void AddSalesmanTable(TableDescriptor table, Salesman salesman)
    {
        DefineColumns(table);

        bool firstSalesmanRow = true;

        foreach (var date in salesman.Dates)
        {
            if (date.IsNoSale)
            {
                // Salesman name cell (spanning), date, and "NO SALE" across the rest.
                SalesmanNameCell(table, salesman.Name, firstSalesmanRow);
                firstSalesmanRow = false;

                table.Cell().Element(GridCell).Text(date.Date.ToString("dd-MMM-yy")).FontSize(BodyFont);
                table.Cell().ColumnSpan(8).Element(GridCell).Text("NO SALE").FontColor(GreyDarken1).FontSize(BodyFont);
                continue;
            }

            for (var i = 0; i < date.Rows.Count; i++)
            {
                var row = date.Rows[i];

                SalesmanNameCell(table, salesman.Name, firstSalesmanRow);
                firstSalesmanRow = false;

                // Date shown once per day (spans the day's product rows).
                if (i == 0)
                    table.Cell().Element(GridCell).Text(date.Date.ToString("dd-MMM-yy")).FontSize(BodyFont);
                else
                    table.Cell().Element(ContinueCell);

                table.Cell().Element(GridCell).Text(Dash(row.ProductSold)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(FmtQty(row.Quantity)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.GrossAmount)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Pct(row.DiscAmt, row.GrossAmount)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.DiscAmt)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.TotalAmt)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.ExchangeAmt)).FontSize(BodyFont);
                table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.NetPaid)).FontSize(BodyFont);
            }

            // DaySum row.
            SalesmanNameCell(table, salesman.Name, false);
            table.Cell().Element(ContinueCell);
            table.Cell().Background(GreyLighten2).Element(TotalCell).Text("DaySum").Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(FmtQty(date.Rows.Sum(r => r.Quantity))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(date.Rows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Pct(date.Rows.Sum(r => r.DiscAmt), date.Rows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(date.Rows.Sum(r => r.DiscAmt))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(date.Rows.Sum(r => r.TotalAmt))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(date.Rows.Sum(r => r.ExchangeAmt))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(date.Rows.Sum(r => r.NetPaid))).Bold().FontSize(TotalFont);
        }

        // SubTotal row (sum of all dates' rows).
        var allRows = salesman.Dates.Where(d => !d.IsNoSale).SelectMany(d => d.Rows).ToList();
        SalesmanNameCell(table, salesman.Name, false);
        table.Cell().ColumnSpan(2).Background(GreyLighten2).Element(TotalCell).Text("SubTotal").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(FmtQty(allRows.Sum(r => r.Quantity))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Pct(allRows.Sum(r => r.DiscAmt), allRows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.DiscAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.TotalAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.ExchangeAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.NetPaid))).Bold().FontSize(TotalFont);
    }

    private static void AddGrandTotalTable(TableDescriptor table, List<Salesman> salesmen)
    {
        DefineColumns(table);

        var allRows = salesmen.SelectMany(s => s.Dates).Where(d => !d.IsNoSale).SelectMany(d => d.Rows).ToList();

        table.Cell().ColumnSpan(3).Background(GreyLighten3).Element(TotalCell).Text("GRAND TOTAL").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(FmtQty(allRows.Sum(r => r.Quantity))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Pct(allRows.Sum(r => r.DiscAmt), allRows.Sum(r => r.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.DiscAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.TotalAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.ExchangeAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(allRows.Sum(r => r.NetPaid))).Bold().FontSize(TotalFont);
    }

    // ── HEADER TABLE ──────────────────────────────────────────────────────

    private static void AddHeaderTable(TableDescriptor table)
    {
        DefineColumns(table);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Salesman Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Date").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Product Sold").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Qty").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Gross Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Disc(%)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("DiscAmt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("TotalAmt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Exchange Amt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Net Paid").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    // ── HELPERS ───────────────────────────────────────────────────────────

    private static IContainer GridCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    private static IContainer ContinueCell(IContainer c)
        => c.BorderLeft(0.5f).BorderRight(0.5f).BorderColor(GreyMedium).Padding(3);

    private static IContainer TotalCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    private static void SalesmanNameCell(TableDescriptor table, string name, bool first)
    {
        if (first)
            table.Cell().BorderTop(0.5f).BorderLeft(0.5f).BorderRight(0.5f).BorderColor(GreyMedium).Padding(3)
                 .Text(Dash(name)).Bold().FontSize(BodyFont);
        else
            table.Cell().BorderLeft(0.5f).BorderRight(0.5f).BorderColor(GreyMedium).Padding(3);
    }

    private static string Fmt(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FmtQty(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string Pct(decimal discAmt, decimal gross)
        => (discAmt == 0 || gross == 0) ? "-" : (discAmt / gross * 100m).ToString("N2", CultureInfo.InvariantCulture);

    private static string Dash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 landscape is 841.89pt wide; with 20pt margins the usable width is
        // 801.89pt. These 10 columns total 694pt, leaving headroom.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(80);   // Salesman Name
            c.ConstantColumn(60);   // Date
            c.ConstantColumn(100);  // Product Sold
            c.ConstantColumn(52);   // Qty
            c.ConstantColumn(74);   // Gross Amount
            c.ConstantColumn(50);   // Disc(%)
            c.ConstantColumn(68);   // DiscAmt
            c.ConstantColumn(74);   // TotalAmt
            c.ConstantColumn(68);   // Exchange Amt
            c.ConstantColumn(68);   // Net Paid
        });
    }
}
