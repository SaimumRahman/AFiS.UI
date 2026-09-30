using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class CustomerWiseSalesDetailReportService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten4 = "#FAFAFA";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 7.5f;
    private const float BodyFont = 7.5f;
    private const float TotalFont = 8f;

    public byte[] GenerateCustomerWiseSalesDetailReport(
        IEnumerable<CustomerWiseSalesDetailDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
        => BuildDocument(rows, storeName, storeAddress, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<CustomerWiseSalesDetailDTO> rows,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = rows?.ToList() ?? new List<CustomerWiseSalesDetailDTO>();

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
            col.Item().AlignCenter().Text("CUSTOMER-WISE SALES - DETAIL REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy} To {dateTo:dd-MMM-yyyy}"
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

    // ── CONTENT: customer blocks → invoice blocks → item lines ───────────

    private void ComposeContent(IContainer container, List<CustomerWiseSalesDetailDTO> data)
    {
        container.PaddingTop(4).Column(col =>
        {
            // Grand total accumulators live outside the loops.
            decimal grandQty = 0, grandGross = 0;
            decimal grandInvDisc = 0, grandCampDisc = 0, grandTotalDisc = 0;
            decimal grandNetPaid = 0, grandDue = 0;

            var customerGroups = data
                .GroupBy(r => new { r.CustomerId, r.CustomerName, r.CustomerPhone, r.CustomerStatus, r.CustomerCode })
                .ToList();

            foreach (var customerGroup in customerGroups)
            {
                var first = customerGroup.First();

                decimal cQty = 0, cGross = 0;
                decimal cInvDisc = 0, cCampDisc = 0, cTotalDisc = 0, cNetPaid = 0, cDue = 0;

                col.Item().Table(table =>
                {
                    DefineColumns(table);
                    AddTableHeader(table);

                    bool isFirstLineOfCustomer = true;

                    foreach (var invoiceGroup in customerGroup.GroupBy(r => r.SaleMasterId))
                    {
                        // Invoice level amounts are repeated on every line by the API,
                        // so take them once per invoice to keep the sums correct.
                        var invoiceRow = invoiceGroup.First();
                        var lines = invoiceGroup.OrderBy(r => r.SalesDetailsId).ToList();
                        bool isFirstLineOfInvoice = true;

                        foreach (var line in lines)
                        {
                            decimal lineGross = line.Rate * line.Quantity;

                            cQty += line.Quantity;
                            cGross += lineGross;

                            // ── 1. CusID ──
                            if (isFirstLineOfCustomer)
                            {
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .Element(cell => ComposeCustomerCell(cell, first));
                            }
                            else
                            {
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2).Text("");
                            }

                            // ── 2. SaleDT ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .Text(FormatSaleDate(line.SaleDate, line.IsBooking)).FontSize(BodyFont);

                            // ── 3. InvoiceNo ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .Element(cell => ComposeInvoiceCell(cell, line));

                            // ── 4. Barcode & Item ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .Text(text =>
                                 {
                                     text.DefaultTextStyle(x => x.FontSize(BodyFont));
                                     text.Span(string.IsNullOrWhiteSpace(line.Barcode) ? "-" : line.Barcode);
                                     text.Line(string.IsNullOrWhiteSpace(line.ItemName) ? "-" : line.ItemName);
                                 });

                            // ── 5. Quantity (with unit) ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .AlignRight().Text(FormatQuantity(line.Quantity, line.UnitName)).FontSize(BodyFont);

                            // ── 6. Rate ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .AlignRight().Text(FormatAmount(line.Rate)).FontSize(BodyFont);

                            // ── 7. Gross Amt ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .AlignRight().Text(FormatAmount(lineGross)).FontSize(BodyFont);

                            // ── 8/9/10/11/12. Invoice level amounts, once per invoice ──
                            if (isFirstLineOfInvoice)
                            {
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .AlignRight().Text(FormatAmount(line.InvDisc)).FontSize(BodyFont);
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .AlignRight().Text(FormatAmount(line.CampaignDisc)).FontSize(BodyFont);
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .AlignRight().Text(FormatAmount(line.TotalDiscount)).FontSize(BodyFont);
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .AlignRight().Text(FormatAmount(line.NetPaid)).FontSize(BodyFont);
                                table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                     .AlignRight().Text(FormatAmount(line.Due)).FontSize(BodyFont);

                                cInvDisc += line.InvDisc;
                                cCampDisc += line.CampaignDisc;
                                cTotalDisc += line.TotalDiscount;
                                cNetPaid += line.NetPaid;
                                cDue += line.Due;
                            }
                            else
                            {
                                for (int blank = 0; blank < 5; blank++)
                                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2).Text("");
                            }

                            // ── 13. UserID ──
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                 .Text(string.IsNullOrWhiteSpace(line.UserId) ? "-" : line.UserId).FontSize(BodyFont);

                            isFirstLineOfInvoice = false;
                            isFirstLineOfCustomer = false;
                        }
                    }

                    // ── Customer SubTotal ──
                    AddTotalRow(table, "SubTotal", cQty, cGross, cInvDisc, cCampDisc, cTotalDisc, cNetPaid, cDue,
                                GreyLighten3, TotalFont);
                });

                grandQty += cQty;
                grandGross += cGross;
                grandInvDisc += cInvDisc;
                grandCampDisc += cCampDisc;
                grandTotalDisc += cTotalDisc;
                grandNetPaid += cNetPaid;
                grandDue += cDue;

                col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(GreyLighten2);
            }

            // ── Grand Total ──
            col.Item().Table(table =>
            {
                DefineColumns(table);
                AddTotalRow(table, "GRAND TOTAL", grandQty, grandGross, grandInvDisc, grandCampDisc,
                            grandTotalDisc, grandNetPaid, grandDue, GreyLighten2, TotalFont + 0.5f);
            });
        });
    }

    // ── CELLS ─────────────────────────────────────────────────────────────

    private static void ComposeCustomerCell(IContainer container, CustomerWiseSalesDetailDTO row)
    {
        bool isWalkIn = row.CustomerId <= 0;
        string phone = isWalkIn ? "WALK-IN CUSTOMER" : row.CustomerPhone;
        string name = isWalkIn ? "-" : row.CustomerName;

        container.Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(BodyFont));
            text.Span(string.IsNullOrWhiteSpace(phone) ? "-" : phone).Bold();
            text.Line($"Status: {(string.IsNullOrWhiteSpace(row.CustomerStatus) ? "-" : row.CustomerStatus)}");
            text.Line(string.IsNullOrWhiteSpace(row.CustomerCode) ? "-" : row.CustomerCode);
            text.Line(string.IsNullOrWhiteSpace(name) ? "-" : name);
        });
    }

    private static void ComposeInvoiceCell(IContainer container, CustomerWiseSalesDetailDTO row)
    {
        container.Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(BodyFont));
            text.Span(string.IsNullOrWhiteSpace(row.InvoiceNo) ? "-" : row.InvoiceNo);
            if (row.IsBooking)
                text.Line("BOOKING").FontColor(GreyDarken1);
        });
    }

    // ── FORMATTING ────────────────────────────────────────────────────────

    private static string FormatAmount(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatQuantity(decimal qty, string? unit)
    {
        if (qty == 0)
            return "-";

        string text = qty.ToString("N2", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    private static string FormatSaleDate(DateTime value, bool isBooking)
    {
        // e.g. 28-Jun-2025 03.30PM
        string text = $"{value:dd-MMM-yyyy} {value:hh\\.mm}{value:tt}";
        return isBooking ? $"{text} (BOOKING)" : text;
    }

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static void DefineColumns(TableDescriptor table)
    {
        // A4 landscape is 841.89pt wide; with 20pt margins on each side the usable
        // width is 801.89pt. These 13 columns total 782pt, leaving headroom so
        // QuestPDF does not raise a conflicting-size-constraints layout error.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(88);   // CusID
            c.ConstantColumn(85);   // SaleDT
            c.ConstantColumn(64);   // InvoiceNo
            c.ConstantColumn(98);   // Barcode & Item
            c.ConstantColumn(56);   // Quantity
            c.ConstantColumn(50);   // Rate
            c.ConstantColumn(56);   // Gross Amt
            c.ConstantColumn(42);   // Inv. Disc.
            c.ConstantColumn(42);   // Camp. Disc.
            c.ConstantColumn(50);   // Total Disc.
            c.ConstantColumn(56);   // Net Paid (Tk)
            c.ConstantColumn(47);   // Due (Tk)
            c.ConstantColumn(48);   // UserID
        });
    }

    private static void AddTableHeader(TableDescriptor table)
    {
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("CusID").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("SaleDT").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("InvoiceNo").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("Barcode & Item").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Quantity").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Rate").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Gross Amt").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Inv. Disc.").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Camp. Disc.").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Total Disc.").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Net Paid (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text("Due (Tk)").Bold().FontSize(HeaderFont);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .Text("UserID").Bold().FontSize(HeaderFont);
    }

    private static void AddTotalRow(
        TableDescriptor table,
        string label,
        decimal qty,
        decimal gross,
        decimal invDisc,
        decimal campDisc,
        decimal totalDisc,
        decimal netPaid,
        decimal due,
        string background,
        float fontSize)
    {
        // Label spans CusID + SaleDT + InvoiceNo + Barcode & Item (4 columns),
        // leaving 9 cells to reach the full 13 column grid.
        table.Cell().ColumnSpan(4).Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(label).Bold().FontSize(fontSize);

        // Quantity, Rate (intentionally blank — summing a rate is meaningless)
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(qty)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2).Text("");

        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(gross)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(invDisc)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(campDisc)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(totalDisc)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(netPaid)).Bold().FontSize(fontSize);
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2)
             .AlignRight().Text(FormatAmount(due)).Bold().FontSize(fontSize);

        // UserID
        table.Cell().Background(background).Border(0.5f).BorderColor(GreyMedium).Padding(2).Text("");
    }
}
