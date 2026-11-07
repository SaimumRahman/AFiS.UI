using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

/// <summary>
/// QuestPDF report generator for the Customer-wise Sales Report page.
/// Produces three documents using the same visual conventions as
/// <see cref="CustomerWiseSalesDetailReportService"/>:
///   • customer summary list (A4 landscape),
///   • single-customer statement with itemized lines (A4 landscape),
///   • 80mm invoice receipt slip.
/// </summary>
public class CustomerSalesReportPdfService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";

    private const float HeaderFont = 7.5f;
    private const float BodyFont = 7.5f;
    private const float TotalFont = 8f;

    private const string DefaultStoreAddress =
        "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.";

    // ═══════════════════════════════════════════════════════════════════════
    //  1. CUSTOMER SUMMARY (customer list)
    // ═══════════════════════════════════════════════════════════════════════

    public byte[] GenerateSummaryReport(
        IEnumerable<CustomerSalesSummaryRow> customers,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-",
        string filterText = "")
        => BuildSummaryDocument(customers, storeName, storeAddress, dateFrom, dateTo, printedBy, filterText)
            .GeneratePdf();

    public Document BuildSummaryDocument(
        IEnumerable<CustomerSalesSummaryRow> customers,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-",
        string filterText = "")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = customers?.ToList() ?? new List<CustomerSalesSummaryRow>();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(header => ComposeHeader(
                    header, storeName, storeAddress,
                    "CUSTOMER-WISE SALES - SUMMARY REPORT",
                    dateFrom, dateTo, filterText));

                page.Content().Element(content => ComposeSummaryContent(content, data));

                page.Footer().Element(footer => ComposeFooter(footer, printedBy));
            });
        });
    }

    private void ComposeSummaryContent(IContainer container, List<CustomerSalesSummaryRow> data)
    {
        container.PaddingTop(4).Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(52);    // ID
                    c.RelativeColumn(3);     // Customer Name
                    c.ConstantColumn(95);    // Mobile
                    c.ConstantColumn(70);    // Tier
                    c.RelativeColumn(2);     // Branch
                    c.ConstantColumn(50);    // Invoices
                    c.ConstantColumn(100);   // Total Purchase
                    c.ConstantColumn(78);    // Last Purchase
                });

                HeaderCell(table, "Cus ID");
                HeaderCell(table, "Customer Name");
                HeaderCell(table, "Mobile No");
                HeaderCell(table, "Status / Tier");
                HeaderCell(table, "Primary Branch");
                HeaderCell(table, "Invoices", right: true);
                HeaderCell(table, "Total Purchase (Tk)", right: true);
                HeaderCell(table, "Last Purchase", right: true);

                foreach (var row in data)
                {
                    BodyCell(table, string.IsNullOrWhiteSpace(row.DisplayCode) ? "-" : row.DisplayCode);
                    BodyCell(table, string.IsNullOrWhiteSpace(row.DisplayName) ? "-" : row.DisplayName);
                    BodyCell(table, string.IsNullOrWhiteSpace(row.DisplayPhone) ? "-" : row.DisplayPhone);
                    BodyCell(table, row.DisplayTier);
                    BodyCell(table, string.IsNullOrWhiteSpace(row.PrimaryBranch) ? "-" : row.PrimaryBranch);
                    BodyCell(table, row.InvoiceCount.ToString(CultureInfo.InvariantCulture), right: true);
                    BodyCell(table, FormatAmount(row.TotalPurchase), right: true);
                    BodyCell(table, row.LastPurchaseDate.HasValue
                        ? row.LastPurchaseDate.Value.ToString("dd-MMM-yyyy")
                        : "-", right: true);
                }

                // ── Grand total ──
                table.Cell().ColumnSpan(5).Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium)
                     .Padding(2).AlignRight()
                     .Text($"TOTAL — {data.Count} customer(s)")
                     .Bold().FontSize(TotalFont);

                table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                     .AlignRight().Text(data.Sum(x => x.InvoiceCount).ToString(CultureInfo.InvariantCulture))
                     .Bold().FontSize(TotalFont);

                table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                     .AlignRight().Text(FormatAmount(data.Sum(x => x.TotalPurchase)))
                     .Bold().FontSize(TotalFont);

                table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2).Text("");
            });

            if (data.Count == 0)
            {
                col.Item().PaddingTop(10).AlignCenter()
                   .Text("No customer sales found for the selected filters.").FontSize(9);
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  2. CUSTOMER STATEMENT (single customer, itemized)
    // ═══════════════════════════════════════════════════════════════════════

    public byte[] GenerateCustomerStatement(
        CustomerSalesSummaryRow customer,
        IEnumerable<CustomerSalesInvoiceGroup> invoices,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-",
        string filterText = "")
        => BuildStatementDocument(customer, invoices, storeName, storeAddress, dateFrom, dateTo, printedBy, filterText)
            .GeneratePdf();

    public Document BuildStatementDocument(
        CustomerSalesSummaryRow customer,
        IEnumerable<CustomerSalesInvoiceGroup> invoices,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string printedBy = "-",
        string filterText = "")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = invoices?.OrderByDescending(i => i.SaleDate).ToList()
                   ?? new List<CustomerSalesInvoiceGroup>();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(header => ComposeHeader(
                    header, storeName, storeAddress,
                    "CUSTOMER-WISE SALES - CUSTOMER STATEMENT",
                    dateFrom, dateTo, filterText));

                page.Content().Element(content => ComposeStatementContent(content, customer, data));

                page.Footer().Element(footer => ComposeFooter(footer, printedBy));
            });
        });
    }

    private void ComposeStatementContent(
        IContainer container,
        CustomerSalesSummaryRow customer,
        List<CustomerSalesInvoiceGroup> invoices)
    {
        container.PaddingTop(4).Column(col =>
        {
            ComposeCustomerBlock(col, customer, invoices);
            ComposeStatementTable(col, invoices);
        });
    }

    private void ComposeCustomerBlock(
        ColumnDescriptor col,
        CustomerSalesSummaryRow customer,
        List<CustomerSalesInvoiceGroup> invoices)
    {
        decimal total = invoices.Sum(i => i.InvoiceNet);
        int invoiceCount = invoices.Count;

        decimal avg = invoiceCount > 0 ? total / invoiceCount : 0;
        DateTime? firstDate = invoiceCount > 0 ? invoices.Min(i => i.SaleDate) : (DateTime?)null;
        DateTime? lastDate = invoiceCount > 0 ? invoices.Max(i => i.SaleDate) : (DateTime?)null;

        decimal months = 1;
        if (firstDate.HasValue && lastDate.HasValue)
        {
            months = Math.Max(1, ((lastDate.Value.Year - firstDate.Value.Year) * 12)
                                    + lastDate.Value.Month - firstDate.Value.Month + 1);
        }

        decimal frequency = months > 0 ? invoiceCount / months : 0;
        int loyalty = (int)Math.Floor(Math.Max(0, total) * 0.03m);

        col.Item().Border(0.5f).BorderColor(GreyMedium).Padding(4).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(200);  // Customer identity
                c.ConstantColumn(120);  // Total spent
                c.ConstantColumn(140);  // Purchase metrics
                c.ConstantColumn(150);  // Recency / loyalty
                c.RelativeColumn();     // filler
            });

            table.Cell().Padding(2).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(BodyFont));
                text.Span(string.IsNullOrWhiteSpace(customer.DisplayName) ? "-" : customer.DisplayName)
                    .Bold().FontSize(10);
                text.Span("\n");
                text.Span($"ID: {customer.DisplayCode}   Tier: {customer.DisplayTier}")
                    .FontSize(BodyFont).FontColor(GreyDarken1);
                text.Span("\n");
                text.Span($"Mobile: {customer.DisplayPhone}").FontSize(BodyFont).FontColor(GreyDarken1);
                text.Span("\n");
                text.Span($"Primary Branch: {(string.IsNullOrWhiteSpace(customer.PrimaryBranch) ? "-" : customer.PrimaryBranch)}")
                    .FontSize(BodyFont).FontColor(GreyDarken1);
            });

            table.Cell().Padding(2).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(BodyFont));
                text.Span("TOTAL LIFETIME SPENT").Bold().FontSize(7).FontColor(GreyDarken1);
                text.Span("\n");
                text.Span(FormatMoney(total)).Bold().FontSize(12);
                text.Span("\n");
                text.Span("Net after discounts & campaigns").FontSize(6.5f).FontColor(GreyDarken1);
            });

            table.Cell().Padding(2).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(BodyFont));
                text.Span("PURCHASE METRICS").Bold().FontSize(7).FontColor(GreyDarken1);
                text.Span("\n");
                text.Span($"{invoiceCount} invoices").Bold();
                text.Span("\n");
                text.Span($"Average invoice: {FormatMoney(avg)}");
                text.Span("\n");
                text.Span($"Frequency: {frequency.ToString("N1", CultureInfo.InvariantCulture)} orders / month");
            });

            table.Cell().Padding(2).Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(BodyFont));
                text.Span("RECENCY & LOYALTY").Bold().FontSize(7).FontColor(GreyDarken1);
                text.Span("\n");
                text.Span($"First purchase: {(firstDate.HasValue ? firstDate.Value.ToString("dd-MMM-yyyy") : "N/A")}");
                text.Span("\n");
                text.Span($"Last purchase: {(lastDate.HasValue ? lastDate.Value.ToString("dd-MMM-yyyy") : "N/A")}");
                text.Span("\n");
                text.Span($"Loyalty points: {loyalty.ToString("N0", CultureInfo.InvariantCulture)} pts").Bold();
            });

            table.Cell().Padding(2).Text("");
        });

        col.Item().PaddingTop(4);
    }

    private void ComposeStatementTable(ColumnDescriptor col, List<CustomerSalesInvoiceGroup> invoices)
    {
        decimal grandQty = 0, grandGross = 0, grandDisc = 0, grandVat = 0, grandNet = 0;

        col.Item().Table(table =>
        {
            DefineStatementColumns(table);

            HeaderCell(table, "Sale DT");
            HeaderCell(table, "Invoice No");
            HeaderCell(table, "Barcode & Item");
            HeaderCell(table, "Qty", right: true);
            HeaderCell(table, "Rate", right: true);
            HeaderCell(table, "Gross", right: true);
            HeaderCell(table, "Discount", right: true);
            HeaderCell(table, "VAT", right: true);
            HeaderCell(table, "Net Amount", right: true);
            HeaderCell(table, "Status");

            foreach (var invoice in invoices)
            {
                var lines = invoice.Lines.OrderBy(l => l.SalesDetailsId).ToList();

                bool isFirstLineOfInvoice = true;
                foreach (var line in lines)
                {
                    grandQty += line.Quantity;
                    grandGross += line.GrossAmount;
                    grandDisc += line.LineDiscount;
                    grandVat += line.Vat;
                    grandNet += line.LineNet;

                    BodyCell(table, FormatSaleDate(line.SaleDate));
                    BodyCell(table, string.IsNullOrWhiteSpace(line.InvoiceNo) ? "-" : line.InvoiceNo);

                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                         .Text(text =>
                         {
                             text.DefaultTextStyle(x => x.FontSize(BodyFont));
                             text.Span(string.IsNullOrWhiteSpace(line.Barcode) ? "-" : line.Barcode);
                             text.Span("\n");
                             text.Span(string.IsNullOrWhiteSpace(line.ItemName) ? "-" : line.ItemName);
                         });

                    BodyCell(table, FormatQuantity(line.Quantity, line.UnitName), right: true);
                    BodyCell(table, FormatAmount(line.Rate), right: true);
                    BodyCell(table, FormatAmount(line.GrossAmount), right: true);
                    BodyCell(table, FormatAmount(line.LineDiscount), right: true);
                    BodyCell(table, FormatAmount(line.Vat), right: true);
                    BodyCell(table, FormatAmount(line.LineNet), right: true);

                    if (isFirstLineOfInvoice)
                    {
                        table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                             .Text(invoice.StatusText).FontSize(BodyFont);
                        isFirstLineOfInvoice = false;
                    }
                    else
                    {
                        table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                             .Text("").FontSize(BodyFont);
                    }
                }

                // ── Invoice level amounts (invoice discount / paid / due) ──
                table.Cell().ColumnSpan(10).BorderBottom(0.3f).BorderColor(GreyMedium)
                     .Background(GreyLighten3).Padding(2)
                     .Text(text =>
                     {
                         text.DefaultTextStyle(x => x.FontSize(6.8f).FontColor(GreyDarken1));
                         text.Span($"Invoice {invoice.InvoiceNo} — ").Bold();
                         text.Span($"Invoice Discount: {FormatAmount(invoice.TotalDiscount)}   ");
                         text.Span($"Net Payable: {FormatAmount(invoice.InvoiceNet)}   ");
                         text.Span($"Paid: {FormatAmount(invoice.NetPaid)}   ");
                         text.Span($"Due: {FormatAmount(invoice.Due)}   ");
                         text.Span($"Branch: {(string.IsNullOrWhiteSpace(invoice.StoreName) ? "-" : invoice.StoreName)}");
                     });
            }

            // ── Customer total ──
            table.Cell().ColumnSpan(3).Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium)
                 .Padding(2).AlignRight().Text("LINE TOTALS").Bold().FontSize(TotalFont);

            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                 .AlignRight().Text(FormatAmount(grandQty)).Bold().FontSize(TotalFont);

            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2).Text("");

            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                 .AlignRight().Text(FormatAmount(grandGross)).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                 .AlignRight().Text(FormatAmount(grandDisc)).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                 .AlignRight().Text(FormatAmount(grandVat)).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                 .AlignRight().Text(FormatAmount(grandNet)).Bold().FontSize(TotalFont);
            table.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2).Text("");
        });

        col.Item().PaddingTop(4).AlignRight().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(9));
            text.Span("Invoice Net Total (invoice discounts applied): ").Bold();
            text.Span(FormatMoney(invoices.Sum(i => i.InvoiceNet))).Bold();
        });
    }

    private static void DefineStatementColumns(TableDescriptor table)
    {
        // A4 landscape usable width ≈ 801pt.
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(84);    // Sale DT
            c.ConstantColumn(72);    // Invoice No
            c.RelativeColumn(2);     // Barcode & Item
            c.ConstantColumn(56);    // Qty
            c.ConstantColumn(52);    // Rate
            c.ConstantColumn(60);    // Gross
            c.ConstantColumn(60);    // Discount
            c.ConstantColumn(48);    // VAT
            c.ConstantColumn(66);    // Net
            c.ConstantColumn(100);   // Status
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  3. INVOICE RECEIPT (80mm slip)
    // ═══════════════════════════════════════════════════════════════════════

    public byte[] GenerateInvoiceReceipt(
        CustomerSalesInvoiceGroup invoice,
        string customerName,
        string customerPhone,
        string companyName = "ASIA FASHION",
        string printedBy = "-")
        => BuildInvoiceReceiptDocument(invoice, customerName, customerPhone, companyName, printedBy)
            .GeneratePdf();

    public Document BuildInvoiceReceiptDocument(
        CustomerSalesInvoiceGroup invoice,
        string customerName,
        string customerPhone,
        string companyName = "ASIA FASHION",
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var lines = invoice?.Lines?.OrderBy(l => l.SalesDetailsId).ToList()
                    ?? new List<CustomerWiseSalesDetailDTO>();

        const float widthMm = 80f;
        float heightMm = 84f + (lines.Count * 7f);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(widthMm, heightMm, Unit.Millimetre);
                page.MarginLeft(3, Unit.Millimetre);
                page.MarginRight(3, Unit.Millimetre);
                page.MarginTop(4, Unit.Millimetre);
                page.MarginBottom(4, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text(companyName).Bold().FontSize(12);
                    col.Item().AlignCenter()
                       .Text(string.IsNullOrWhiteSpace(invoice?.StoreName) ? "Head Office" : invoice.StoreName)
                       .FontSize(7.5f).FontColor(GreyDarken1);
                    col.Item().PaddingTop(2).AlignCenter()
                       .Text(invoice?.StatusText == "Regular Purchase" ? "SALES RECEIPT" : "SALES / EXCHANGE RECEIPT")
                       .Bold().FontSize(9);
                    col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Black);

                    ReceiptInfoRow(col, "Invoice:", invoice?.InvoiceNo ?? "-");
                    ReceiptInfoRow(col, "Date:", invoice != null ? invoice.SaleDate.ToString("dd-MMM-yyyy hh:mm tt") : "-");
                    ReceiptInfoRow(col, "Customer:", string.IsNullOrWhiteSpace(customerName) ? "-" : customerName);
                    ReceiptInfoRow(col, "Mobile:", string.IsNullOrWhiteSpace(customerPhone) ? "-" : customerPhone);
                    ReceiptInfoRow(col, "Status:", invoice?.StatusText ?? "-");

                    col.Item().PaddingVertical(2).LineHorizontal(0.4f).LineColor(GreyMedium);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();                        // Item
                            c.ConstantColumn(10, Unit.Millimetre);     // Qty
                            c.ConstantColumn(15, Unit.Millimetre);     // Rate
                            c.ConstantColumn(19, Unit.Millimetre);     // Amount
                        });

                        table.Cell().Text("Item").Bold().FontSize(6.5f).FontColor(GreyDarken1);
                        table.Cell().AlignRight().Text("Qty").Bold().FontSize(6.5f).FontColor(GreyDarken1);
                        table.Cell().AlignRight().Text("Rate").Bold().FontSize(6.5f).FontColor(GreyDarken1);
                        table.Cell().AlignRight().Text("Amount").Bold().FontSize(6.5f).FontColor(GreyDarken1);

                        foreach (var line in lines)
                        {
                            table.Cell().PaddingTop(2).Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontSize(7));
                                text.Span(string.IsNullOrWhiteSpace(line.ItemName) ? "-" : line.ItemName);
                                if (!string.IsNullOrWhiteSpace(line.Barcode))
                                {
                                    text.Span("\n");
                                    text.Span(line.Barcode).FontSize(6).FontColor(GreyDarken1);
                                }
                            });

                            table.Cell().PaddingTop(2).AlignRight()
                                 .Text(line.Quantity.ToString("N2", CultureInfo.InvariantCulture)).FontSize(7);
                            table.Cell().PaddingTop(2).AlignRight()
                                 .Text(FormatAmount(line.Rate)).FontSize(7);
                            table.Cell().PaddingTop(2).AlignRight()
                                 .Text(FormatAmount(line.LineNet)).FontSize(7);
                        }
                    });

                    col.Item().PaddingTop(3).LineHorizontal(0.4f).LineColor(GreyMedium);

                    ReceiptTotalRow(col, "Gross:", lines.Sum(l => l.GrossAmount));
                    ReceiptTotalRow(col, "Discount:", invoice?.TotalDiscount ?? 0);
                    ReceiptTotalRow(col, "VAT:", lines.Sum(l => l.Vat));

                    col.Item().PaddingTop(1).LineHorizontal(0.6f).LineColor(Black);

                    col.Item().PaddingTop(1).Row(row =>
                    {
                        row.RelativeItem().Text("NET PAYABLE").Bold().FontSize(9);
                        row.ConstantItem(26, Unit.Millimetre).AlignRight()
                           .Text(FormatMoney(invoice?.InvoiceNet ?? 0)).Bold().FontSize(9);
                    });

                    ReceiptTotalRow(col, "Paid:", invoice?.NetPaid ?? 0);
                    ReceiptTotalRow(col, "Due:", invoice?.Due ?? 0);

                    col.Item().PaddingVertical(2).LineHorizontal(0.4f).LineColor(GreyMedium);

                    col.Item().AlignCenter()
                       .Text("Thank you for shopping with Asia Fashion")
                       .FontSize(7).FontColor(GreyDarken1);

                    col.Item().PaddingTop(2).AlignCenter()
                       .Text($"Printed by: {(string.IsNullOrWhiteSpace(printedBy) ? "-" : printedBy)}")
                       .FontSize(6.5f).FontColor(GreyDarken1);

                    col.Item().AlignCenter()
                       .Text($"Print Date: {DateTime.Now:dd-MMM-yyyy hh.mmtt}")
                       .FontSize(6.5f).FontColor(GreyDarken1);
                });
            });
        });
    }

    private static void ReceiptInfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(18, Unit.Millimetre).Text(label).FontSize(7).FontColor(GreyDarken1).Bold();
            row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(7);
        });
    }

    private static void ReceiptTotalRow(ColumnDescriptor col, string label, decimal value)
    {
        col.Item().PaddingTop(1).Row(row =>
        {
            row.RelativeItem().AlignRight().Text(label).FontSize(7).FontColor(GreyDarken1);
            row.ConstantItem(26, Unit.Millimetre).AlignRight().Text(FormatAmount(value)).FontSize(7);
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HEADER / FOOTER
    // ═══════════════════════════════════════════════════════════════════════

    private void ComposeHeader(
        IContainer container,
        string storeName,
        string storeAddress,
        string title,
        DateTime? dateFrom,
        DateTime? dateTo,
        string filterText)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text(storeName).Bold().FontSize(13);
            col.Item().AlignCenter().Text(storeAddress).FontSize(9);
            col.Item().AlignCenter().Text(title).Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy} To {dateTo:dd-MMM-yyyy}"
                : "ALL TIME";

            col.Item().AlignCenter().Text(range).FontSize(9);

            if (!string.IsNullOrWhiteSpace(filterText))
                col.Item().AlignCenter().Text(filterText).FontSize(7.5f).FontColor(GreyDarken1);

            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

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

    // ═══════════════════════════════════════════════════════════════════════
    //  TABLE CELL HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static void HeaderCell(TableDescriptor table, string label, bool right = false)
    {
        var cell = table.Cell()
            .Background(GreyLighten3)
            .Border(0.5f).BorderColor(GreyMedium)
            .Padding(2);

        if (right)
            cell.AlignRight().Text(label).Bold().FontSize(HeaderFont);
        else
            cell.Text(label).Bold().FontSize(HeaderFont);
    }

    private static void BodyCell(TableDescriptor table, string value, bool right = false)
    {
        var cell = table.Cell()
            .BorderBottom(0.3f).BorderColor(GreyMedium)
            .Padding(2);

        if (right)
            cell.AlignRight().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(BodyFont);
        else
            cell.Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(BodyFont);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  FORMATTING
    // ═══════════════════════════════════════════════════════════════════════

    private static string FormatAmount(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal value)
        => "Tk " + value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatQuantity(decimal qty, string? unit)
    {
        if (qty == 0)
            return "-";

        string text = qty.ToString("N2", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    private static string FormatSaleDate(DateTime value)
        => $"{value:dd-MMM-yyyy} {value:hh\\.mm}{value:tt}";
}
