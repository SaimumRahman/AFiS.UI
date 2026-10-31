using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class PayTypeWiseSalesDetailsReportService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 9f;
    private const float BodyFont = 9f;
    private const float TotalFont = 9.5f;

    public byte[] GeneratePayTypeWiseSalesDetailsReport(
        IEnumerable<PayTypeWiseSalesDetailsDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<PayTypeWiseSalesDetailsDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<PayTypeWiseSalesDetailsDTO>();
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
            col.Item().AlignCenter().Text("PAY TYPE-WISE SALES DETAILS REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy} TO {dateTo:dd-MMM-yyyy}".ToUpperInvariant()
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
                text.Span("Print Date & Time: ").Bold();
                text.Span($"{DateTime.Now:dd-MMM-yyyy hh.mmtt}");
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

    // ── GROUPING (by date) ────────────────────────────────────────────────

    private sealed class Group
    {
        public DateTime Date { get; set; }
        public List<PayTypeWiseSalesDetailsDTO> Rows { get; set; } = new();
    }

    private static List<Group> BuildGroups(List<PayTypeWiseSalesDetailsDTO> data)
    {
        var groups = new List<Group>();

        foreach (var row in data)
        {
            if (groups.Count == 0 || groups[^1].Date.Date != row.SaleDate.Date)
                groups.Add(new Group { Date = row.SaleDate.Date });

            groups[^1].Rows.Add(row);
        }

        return groups;
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(
        IContainer container,
        List<Group> groups,
        List<PayTypeWiseSalesDetailsDTO> data)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(AddHeaderTable);

            if (groups.Count == 0)
            {
                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    table.Cell().ColumnSpan(12).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
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
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Invoice No").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Date").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Customer Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("NET Amt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Exchange Amt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Cash Paid").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("CityPOS").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("DBBLPOS").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Bkash").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Nagad").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Dues").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Rcvd By").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    // ── ONE DATE GROUP ────────────────────────────────────────────────────

    private static void AddGroupTable(TableDescriptor table, Group group)
    {
        DefineColumns(table);

        foreach (var row in group.Rows)
        {
            table.Cell().Element(GridCell).Text(Dash(row.InvoiceNo)).FontSize(BodyFont);
            table.Cell().Element(GridCell).Text(row.SaleDate.ToString("dd-MMM-yy")).FontSize(BodyFont);
            table.Cell().Element(GridCell).Text(Dash(row.CustomerName)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.NetAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.ExchangeAmount)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.CashPaid)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.CityPos)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.DbbLPos)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.Bkash)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.Nagad)).FontSize(BodyFont);
            table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.Dues)).FontSize(BodyFont);
            table.Cell().Element(GridCell).Text(Dash(row.RcvdBy)).FontSize(BodyFont);
        }

        // DaySum row, spanning Invoice No + Date + Customer Name. The Rcvd By
        // cell is intentionally blank. Sums the rows in this date so it cannot
        // disagree with them.
        table.Cell().ColumnSpan(3).Background(GreyLighten2).Element(TotalCell)
             .Text("DaySum").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.NetAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.ExchangeAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.CashPaid))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.CityPos))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.DbbLPos))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.Bkash))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.Nagad))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight().Text(Fmt(group.Rows.Sum(r => r.Dues))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten2).Element(TotalCell);
    }

    // ── GRAND TOTAL TABLE ─────────────────────────────────────────────────

    private static void AddGrandTotalTable(TableDescriptor table, List<PayTypeWiseSalesDetailsDTO> data)
    {
        DefineColumns(table);

        table.Cell().ColumnSpan(3).Background(GreyLighten3).Element(TotalCell)
             .Text("GRAND TOTAL").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.NetAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.ExchangeAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.CashPaid))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.CityPos))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.DbbLPos))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.Bkash))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.Nagad))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight().Text(Fmt(data.Sum(r => r.Dues))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell);
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
        // 801.89pt. These 12 columns total 771pt, leaving headroom so QuestPDF
        // does not raise a conflicting-size-constraints layout error.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(82);   // Invoice No
            c.ConstantColumn(58);   // Date
            c.ConstantColumn(88);   // Customer Name
            c.ConstantColumn(66);   // NET Amt
            c.ConstantColumn(66);   // Exchange Amt
            c.ConstantColumn(66);   // Cash Paid
            c.ConstantColumn(58);   // CityPOS
            c.ConstantColumn(58);   // DBBLPOS
            c.ConstantColumn(58);   // Bkash
            c.ConstantColumn(58);   // Nagad
            c.ConstantColumn(54);   // Dues
            c.ConstantColumn(58);   // Rcvd By
        });
    }
}
