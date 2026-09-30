using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class SalesmanWiseDetailedReportService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 8.5f;
    private const float BodyFont = 8.5f;
    private const float TotalFont = 9f;

    public byte[] GenerateSalesmanWiseDetailedReport(
        IEnumerable<SalesmanWiseDetailedDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<SalesmanWiseDetailedDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<SalesmanWiseDetailedDTO>();
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
            col.Item().AlignCenter().Text("SALESMAN-WISE DETAILED REPORT").Bold().FontSize(10);

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

    private sealed class Line
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CampDisc { get; set; }
    }

    private sealed class Invoice
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public decimal InvDisc { get; set; }
        public decimal TotalAmt { get; set; }
        public decimal ExchangeAmt { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Dues { get; set; }
        public List<Line> Lines { get; set; } = new();
    }

    private sealed class Salesman
    {
        public string Name { get; set; } = string.Empty;
        public List<Invoice> Invoices { get; set; } = new();
    }

    private static List<Salesman> BuildSalesmen(List<SalesmanWiseDetailedDTO> data)
    {
        var salesmen = new List<Salesman>();

        foreach (var row in data)
        {
            var name = row.SalesmanName ?? string.Empty;
            if (salesmen.Count == 0 || salesmen[^1].Name != name)
                salesmen.Add(new Salesman { Name = name });

            var salesman = salesmen[^1];
            if (salesman.Invoices.Count == 0 || salesman.Invoices[^1].InvoiceNo != (row.InvoiceNo ?? string.Empty))
            {
                salesman.Invoices.Add(new Invoice
                {
                    InvoiceNo = row.InvoiceNo,
                    SaleDate = row.SaleDate,
                    Remarks = row.Remarks,
                    InvDisc = row.InvDisc,
                    TotalAmt = row.TotalAmt,
                    ExchangeAmt = row.ExchangeAmt,
                    NetPaid = row.NetPaid,
                    Dues = row.Dues
                });
            }

            salesman.Invoices[^1].Lines.Add(new Line
            {
                ItemCode = row.ItemCode,
                ItemName = row.ItemName,
                Quantity = row.Quantity,
                Uom = row.Uom,
                Rate = row.Rate,
                GrossAmount = row.GrossAmount,
                CampDisc = row.CampDisc
            });
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
                    table.Cell().ColumnSpan(14).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No sales records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                });
            }

            foreach (var salesman in salesmen)
                AddSalesman(col, salesman);

            if (salesmen.Count > 0)
                col.Item().Table(table => AddGrandTotalTable(table, salesmen));
        });
    }

    private static void AddSalesman(ColumnDescriptor col, Salesman salesman)
    {
        col.Item().Table(table =>
        {
            DefineColumns(table);
            table.Cell().ColumnSpan(14).Background(GreyLighten2)
                 .Border(0.5f).BorderColor(GreyMedium).Padding(4)
                 .Text(Dash(salesman.Name)).Bold().FontSize(BodyFont);
        });

        col.Item().Table(table =>
        {
            DefineColumns(table);

            foreach (var invoice in salesman.Invoices)
            {
                for (var i = 0; i < invoice.Lines.Count; i++)
                {
                    var line = invoice.Lines[i];
                    var first = i == 0;

                    if (first)
                    {
                        table.Cell().Element(GridCell).Text(Dash(invoice.InvoiceNo)).FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text(invoice.SaleDate.ToString("dd-MMM-yy")).FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text(Dash(invoice.Remarks)).FontSize(BodyFont);
                    }
                    else
                    {
                        table.Cell().Element(GridCell).Text("").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("").FontSize(BodyFont);
                    }

                    table.Cell().Element(GridCell).Text(Dash(line.ItemCode)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).Text(Dash(line.ItemName)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).Text($"{line.Quantity:N2} {Dash(line.Uom)}").FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(line.Rate)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(line.GrossAmount)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(line.CampDisc)).FontSize(BodyFont);

                    // Invoice-level amounts only on the first line.
                    if (first)
                    {
                        table.Cell().Element(GridCell).AlignRight().Text(Fmt(invoice.InvDisc)).FontSize(BodyFont);
                        table.Cell().Element(GridCell).AlignRight().Text(Fmt(invoice.TotalAmt)).FontSize(BodyFont);
                        table.Cell().Element(GridCell).AlignRight().Text(ExchangeFmt(invoice.ExchangeAmt)).FontSize(BodyFont);
                        table.Cell().Element(GridCell).AlignRight().Text(Fmt(invoice.NetPaid)).FontSize(BodyFont);
                        table.Cell().Element(GridCell).AlignRight().Text(Fmt(invoice.Dues)).FontSize(BodyFont);
                    }
                    else
                    {
                        table.Cell().Element(GridCell).Text("-").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("-").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("-").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("-").FontSize(BodyFont);
                        table.Cell().Element(GridCell).Text("-").FontSize(BodyFont);
                    }
                }
            }

            // SubTotal row.
            var lines = salesman.Invoices.SelectMany(i => i.Lines).ToList();
            table.Cell().ColumnSpan(6).Background(GreyLighten2).Element(TotalCell)
                 .Text("SubTotal").Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell);                                   // Rate blank
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(lines.Sum(l => l.GrossAmount))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(lines.Sum(l => l.CampDisc))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(salesman.Invoices.Sum(i => i.InvDisc))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(salesman.Invoices.Sum(i => i.TotalAmt))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(ExchangeFmt(salesman.Invoices.Sum(i => i.ExchangeAmt))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(salesman.Invoices.Sum(i => i.NetPaid))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                 .Text(Fmt(salesman.Invoices.Sum(i => i.Dues))).Bold().FontSize(TotalFont);
        });
    }

    private static void AddGrandTotalTable(TableDescriptor table, List<Salesman> salesmen)
    {
        DefineColumns(table);

        var lines = salesmen.SelectMany(s => s.Invoices).SelectMany(i => i.Lines).ToList();
        var invoices = salesmen.SelectMany(s => s.Invoices).ToList();

        table.Cell().ColumnSpan(6).Background(GreyLighten3).Element(TotalCell)
             .Text("GRAND TOTAL").Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(lines.Sum(l => l.GrossAmount))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(lines.Sum(l => l.CampDisc))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(invoices.Sum(i => i.InvDisc))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(invoices.Sum(i => i.TotalAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(ExchangeFmt(invoices.Sum(i => i.ExchangeAmt))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(invoices.Sum(i => i.NetPaid))).Bold().FontSize(TotalFont);
        table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
             .Text(Fmt(invoices.Sum(i => i.Dues))).Bold().FontSize(TotalFont);
    }

    // ── HEADER TABLE ──────────────────────────────────────────────────────

    private static void AddHeaderTable(TableDescriptor table)
    {
        DefineColumns(table);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Invoice No").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Date").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Remarks").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Item Code").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Item Name").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Qty").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Rate").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Gross Amount").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Camp.Disc").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Inv.Disc").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Total Amt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("ExchngeAmt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Net Paid").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("DUES").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    // ── HELPERS ───────────────────────────────────────────────────────────

    private static IContainer GridCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    private static IContainer TotalCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(3);

    private static string Fmt(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string ExchangeFmt(decimal value)
        => value == 0 ? "-"
           : value < 0 ? $"({Math.Abs(value):N2})"
           : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string Dash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 landscape is 841.89pt wide; with 20pt margins the usable width is
        // 801.89pt. These 14 columns total 773pt, leaving headroom.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(70);   // Invoice No
            c.ConstantColumn(50);   // Date
            c.ConstantColumn(50);   // Remarks
            c.ConstantColumn(60);   // Item Code
            c.ConstantColumn(95);   // Item Name
            c.ConstantColumn(46);   // Qty
            c.ConstantColumn(46);   // Rate
            c.ConstantColumn(54);   // Gross Amount
            c.ConstantColumn(48);   // Camp.Disc
            c.ConstantColumn(44);   // Inv.Disc
            c.ConstantColumn(54);   // Total Amt
            c.ConstantColumn(56);   // ExchngeAmt
            c.ConstantColumn(54);   // Net Paid
            c.ConstantColumn(46);   // DUES
        });
    }
}
