using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class PurchaseDetailReportService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 9f;
    private const float BodyFont = 9f;
    private const float BreakdownFont = 7.5f;
    private const float TotalFont = 9.5f;

    public byte[] GeneratePurchaseDetailReport(
        IEnumerable<PurchaseDetailDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<PurchaseDetailDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<PurchaseDetailDTO>();
        var challans = BuildChallans(data);

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
                    ComposeContent(content, challans, data));

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
            col.Item().AlignCenter().Text("PURCHASE DETAIL REPORT").Bold().FontSize(10);

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
        container.BorderTop(0.5f).BorderColor(GreyMedium).PaddingTop(4).Column(col =>
        {
            col.Item().Row(row =>
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
        });
    }

    // ── GROUPING ──────────────────────────────────────────────────────────

    private sealed class Section
    {
        public string BillNo { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public List<PurchaseDetailDTO> Rows { get; set; } = new();
    }

    private sealed class Challan
    {
        public int PurchaseId { get; set; }
        public string ChallanNo { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public List<Section> Sections { get; set; } = new();
    }

    private static List<Challan> BuildChallans(List<PurchaseDetailDTO> data)
    {
        var challans = new List<Challan>();

        foreach (var row in data)
        {
            if (challans.Count == 0 || challans[^1].PurchaseId != row.PurchaseId)
            {
                challans.Add(new Challan
                {
                    PurchaseId = row.PurchaseId,
                    ChallanNo = row.ChallanNo,
                    SupplierName = row.SupplierName,
                    BillDate = row.BillDate
                });
            }

            var challan = challans[^1];
            var key = (Bill: row.BillNo ?? string.Empty, Group: row.GroupName ?? string.Empty);

            if (challan.Sections.Count == 0 ||
                challan.Sections[^1].BillNo != key.Bill ||
                challan.Sections[^1].GroupName != key.Group)
            {
                challan.Sections.Add(new Section { BillNo = key.Bill, GroupName = key.Group });
            }

            challan.Sections[^1].Rows.Add(row);
        }

        return challans;
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(
        IContainer container,
        List<Challan> challans,
        List<PurchaseDetailDTO> data)
    {
        container.PaddingTop(6).Column(col =>
        {
            col.Item().Table(AddHeaderTable);

            if (challans.Count == 0)
            {
                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    table.Cell().ColumnSpan(8).Element(c => c.Border(0.5f).BorderColor(GreyMedium)
                        .AlignCenter().PaddingVertical(8)
                        .Text("No purchase records found for the selected period.")
                        .FontColor(GreyDarken1).FontSize(BodyFont));
                });
            }

            foreach (var challan in challans)
                AddChallan(col, challan);

            col.Item().PaddingTop(4).Text(
                "Pp-Product Price, CC-Carrying Cost, TR-Transport Cost, OC-Operation Cost, VAT-Value Added Tax")
               .FontSize(7.5f).Italic().FontColor(GreyDarken1);
        });
    }

    private static void AddChallan(ColumnDescriptor col, Challan challan)
    {
        // Challan banner.
        col.Item().Table(table =>
        {
            DefineColumns(table);
            table.Cell().ColumnSpan(8).Background(GreyLighten2)
                 .Border(0.5f).BorderColor(GreyMedium).Padding(4).Text(text =>
                 {
                     text.DefaultTextStyle(x => x.FontSize(BodyFont));
                     text.Span("P.Challan No: ").Bold();
                     text.Span(Dash(challan.ChallanNo));
                     text.Span("    Supplier: ").Bold();
                     text.Span(Dash(challan.SupplierName));
                     text.Span("    BillDt: ").Bold();
                     text.Span(challan.BillDate.ToString("ddMMMyy"));
                 });
        });

        foreach (var section in challan.Sections)
        {
            // Section banner: "Bill: X" + "Group: Y".
            col.Item().Table(table =>
            {
                DefineColumns(table);
                table.Cell().ColumnSpan(8).Background(GreyLighten3)
                     .Border(0.5f).BorderColor(GreyMedium).Padding(4).Text(text =>
                     {
                         text.DefaultTextStyle(x => x.FontSize(BodyFont));
                         text.Span($"Bill: {Dash(section.BillNo)}");
                         text.Span("    Group: ").Bold();
                         text.Span(Dash(section.GroupName));
                     });
            });

            // Item rows + SubTotal.
            col.Item().Table(table =>
            {
                DefineColumns(table);

                foreach (var row in section.Rows)
                {
                    table.Cell().Element(GridCell).Text(Dash(row.Code)).FontSize(BodyFont);

                    table.Cell().Element(GridCell).Column(col2 =>
                    {
                        col2.Item().Text(Dash(row.ItemName)).FontSize(BodyFont);
                        col2.Item().PaddingTop(2)
                            .Text(BuildBreakdown(row))
                            .FontSize(BreakdownFont).FontColor(GreyDarken1);
                    });

                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.CostPrice)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(FmtQty(row.Quantity)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).Text(Dash(row.Uom)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.PurTotal)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.SalePrice)).FontSize(BodyFont);
                    table.Cell().Element(GridCell).AlignRight().Text(Fmt(row.SaleTotal)).FontSize(BodyFont);
                }

                // SubTotal (Qty, Pur.Total, Sale Total).
                table.Cell().ColumnSpan(2).Background(GreyLighten2).Element(TotalCell)
                     .Text("SubTotal").Bold().FontSize(TotalFont);
                table.Cell().Background(GreyLighten2).Element(TotalCell);                                   // Cost Price blank
                table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                     .Text(FmtQty(section.Rows.Sum(r => r.Quantity))).Bold().FontSize(TotalFont);
                table.Cell().Background(GreyLighten2).Element(TotalCell);                                   // UoM blank
                table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                     .Text(Fmt(section.Rows.Sum(r => r.PurTotal))).Bold().FontSize(TotalFont);
                table.Cell().Background(GreyLighten2).Element(TotalCell);                                   // Sale Price blank
                table.Cell().Background(GreyLighten2).Element(TotalCell).AlignRight()
                     .Text(Fmt(section.Rows.Sum(r => r.SaleTotal))).Bold().FontSize(TotalFont);
            });
        }

        // Challan Total (Pur.Total, Sale Total).
        col.Item().Table(table =>
        {
            DefineColumns(table);
            table.Cell().ColumnSpan(2).Background(GreyLighten3).Element(TotalCell)
                 .Text("Challan Total").Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten3).Element(TotalCell);
            table.Cell().Background(GreyLighten3).Element(TotalCell);
            table.Cell().Background(GreyLighten3).Element(TotalCell);
            table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
                 .Text(Fmt(challan.Sections.Sum(s => s.Rows.Sum(r => r.PurTotal)))).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten3).Element(TotalCell);
            table.Cell().Background(GreyLighten3).Element(TotalCell).AlignRight()
                 .Text(Fmt(challan.Sections.Sum(s => s.Rows.Sum(r => r.SaleTotal)))).Bold().FontSize(TotalFont);
        });
    }

    // ── HEADER TABLE ──────────────────────────────────────────────────────

    private static void AddHeaderTable(TableDescriptor table)
    {
        DefineColumns(table);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("Code").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("ItemName").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Cost Price").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Quantity").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).Text("UoM").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Pur.Total").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Sale Price").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Element(Hdr).AlignRight().Text("Sale Total").Bold().FontSize(HeaderFont);
    }

    private static IContainer Hdr(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    // ── HELPERS ───────────────────────────────────────────────────────────

    private static IContainer GridCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static IContainer TotalCell(IContainer c)
        => c.Border(0.5f).BorderColor(GreyMedium).Padding(4);

    private static string Fmt(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FmtQty(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string Dash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static string BuildBreakdown(PurchaseDetailDTO row)
        => $"Pp-{row.ProductPrice:N0}+CC-{row.CarryingCost:N0}+TR-{row.OtherCost:N0}+OC-{row.OperationalCost:N0}+VAT-{row.VatAmount:N0}";

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 landscape is 841.89pt wide; with 20pt margins the usable width is
        // 801.89pt. These 8 columns total 724pt, leaving headroom.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(90);   // Code
            c.ConstantColumn(220);  // ItemName (+ breakdown line)
            c.ConstantColumn(70);   // Cost Price
            c.ConstantColumn(55);   // Quantity
            c.ConstantColumn(55);   // UoM
            c.ConstantColumn(80);   // Pur.Total
            c.ConstantColumn(72);   // Sale Price
            c.ConstantColumn(82);   // Sale Total
        });
    }
}
