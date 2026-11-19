using JM.UI.Entities.Model.SalesPOS;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JM.UI.Service.Reports;

/// <summary>
/// Generates a POS (80mm thermal paper) size sales invoice PDF using QuestPDF.
/// </summary>
public class PosInvoicePdfService
{
    private const string Black = "#000000";
    private const string GreyDark = "#424242";
    private const string GreyMedium = "#9E9E9E";

    public byte[] GeneratePosInvoice(
        SaleMasterDTO sale,
        string companyName = "ASIA FASHION",
        string companyAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        string companyContact = "",
        string companyVat = "",
        string servedBy = "",
        string terms = "",
        IReadOnlyDictionary<int, string>? paymentTypeNames = null,
        string branchName = "")
        => BuildDocument(sale, companyName, companyAddress, companyContact, companyVat,
                          servedBy, terms, paymentTypeNames, branchName).GeneratePdf();

    /// <summary>
    /// Builds the invoice document (useful for previews/tests as well as PDF output).
    /// </summary>
    public Document BuildDocument(
        SaleMasterDTO sale,
        string companyName = "ASIA FASHION",
        string companyAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        string companyContact = "",
        string companyVat = "",
        string servedBy = "",
        string terms = "",
        IReadOnlyDictionary<int, string>? paymentTypeNames = null,
        string branchName = "")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        int itemCount = sale.SaleDetails?.Count ?? 0;
        float widthMm = 80f;

        // Estimate the vertical space needed by the (optional) terms block so
        // the fixed-height thermal page is not clipped.
        float termsHeightMm = 0;
        if (!string.IsNullOrWhiteSpace(terms))
        {
            var normalized = terms.Replace("\r\n", "\n").Replace('\r', '\n');
            var lineCount = 0;
            foreach (var rawLine in normalized.Split('\n'))
                lineCount += Math.Max(1, (int)Math.Ceiling(rawLine.Length / 42.0));
            termsHeightMm = 15f + (lineCount * 3.9f);
        }

        // Height sized to the measured content plus a small safety margin so the
        // larger text is never clipped on the fixed-size thermal page.
        float heightMm = 122f + (itemCount * 8.5f) + termsHeightMm;
        var paymentCount = sale.PaymentTransactions?.Count ?? 0;
        heightMm += paymentCount * 5.2f;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(widthMm, heightMm, Unit.Millimetre);
                page.MarginLeft(3, Unit.Millimetre);
                page.MarginRight(3, Unit.Millimetre);
                page.MarginTop(4, Unit.Millimetre);
                page.MarginBottom(4, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily("Arial").LineHeight(1.25f));

                page.Content().Column(col =>
                {
                    bool isBookingDoc = sale.IsBooking == true
    || (!string.IsNullOrWhiteSpace(sale.InvoiceNo)
        && sale.InvoiceNo.StartsWith("BK-", StringComparison.OrdinalIgnoreCase));
ComposeHeader(col, companyName, branchName, companyAddress, companyContact, companyVat, isBookingDoc);
                    ComposeInfo(col, sale, servedBy);
                    ComposeItems(col, sale);
                    ComposeTotals(col, sale, paymentTypeNames);
                    // Payment breakdown sits after the Net Payable block and before the terms.
                    ComposePayments(col, sale, paymentTypeNames);
                    ComposeTerms(col, terms);
                    ComposeFooter(col);
                });
            });
        });

        return document;
    }

    // ── 1. Company details ────────────────────────────────────────────────

    private static void ComposeHeader(
        ColumnDescriptor col,
        string companyName,
        string branchName,
        string companyAddress,
        string companyContact,
        string companyVat,
        bool isBooking = false)
    {
        col.Item().AlignCenter().Text(companyName).Bold().FontSize(15);
        // Branch name sits directly under the company name in normal (non-bold) weight.
        if (!string.IsNullOrWhiteSpace(branchName))
            col.Item().AlignCenter().Text(branchName).FontSize(10);
        if (!string.IsNullOrWhiteSpace(companyAddress))
            col.Item().AlignCenter().Text(companyAddress).FontSize(9);
        if (!string.IsNullOrWhiteSpace(companyContact))
            col.Item().AlignCenter().Text(companyContact).FontSize(9);
        if (!string.IsNullOrWhiteSpace(companyVat))
            col.Item().AlignCenter().Text($"VAT Reg No: {companyVat}").FontSize(9);

        var invoiceHeading = isBooking ? "BOOKING INVOICE" : "SALES INVOICE";
        col.Item().PaddingTop(3).AlignCenter().Text(invoiceHeading).Bold().FontSize(12);
        col.Item().PaddingVertical(2).LineHorizontal(0.6f).LineColor(Black);
    }

    // ── 2. Invoice No / Served by / Date / Customer ───────────────────────

    private static void ComposeInfo(ColumnDescriptor col, SaleMasterDTO sale, string servedBy)
    {
        void InfoRow(string label, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(66).Text(label).Bold().FontSize(9).FontColor(GreyDark);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(9.5f);
            });
        }

        InfoRow("Invoice No:", sale.InvoiceNo ?? "");
        InfoRow("Served by:", servedBy);
        InfoRow("Date:", sale.SalesDate != default ? sale.SalesDate.ToString("dd/MM/yyyy hh:mm tt") : DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
        InfoRow("Customer:", sale.CustomerName ?? "");
        InfoRow("Mobile:", sale.CustomerPhone ?? "");

        col.Item().PaddingVertical(2).LineHorizontal(0.6f).LineColor(Black);
    }

    // ── 3. Items table (Barcode / Qty / Rate / Amount) ────────────────────

    private static void ComposeItems(ColumnDescriptor col, SaleMasterDTO sale)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.ConstantColumn(22);
                c.ConstantColumn(50);
                c.ConstantColumn(52);
            });

            // Header
            table.Cell().BorderBottom(0.6f).BorderColor(Black).Padding(1).Text("Item").Bold().FontSize(9);
            table.Cell().BorderBottom(0.6f).BorderColor(Black).Padding(1).AlignRight().Text("Qty").Bold().FontSize(9);
            table.Cell().BorderBottom(0.6f).BorderColor(Black).Padding(1).AlignRight().Text("Rate").Bold().FontSize(9);
            table.Cell().BorderBottom(0.6f).BorderColor(Black).Padding(1).AlignRight().Text("Amount").Bold().FontSize(9);

            foreach (var item in sale.SaleDetails ?? new List<SaleDetailDTO>())
            {
                var barcode = item.Barcode ?? "";
                var name = item.ProductName ?? "";

                table.Cell().Padding(1).Column(c =>
                {
                    c.Item().Text(barcode).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, barcode, StringComparison.OrdinalIgnoreCase))
                        c.Item().Text(name.Length > 20 ? name[..20] + ".." : name).FontSize(8).FontColor(GreyDark);
                });

                table.Cell().Padding(1).AlignRight().Text($"{item.Qty:0.##}").FontSize(9);
                table.Cell().Padding(1).AlignRight().Text($"{item.UnitPrice:N2}").FontSize(9);
                table.Cell().Padding(1).AlignRight().Text($"{item.TotalAmount:N2}").FontSize(9);
            }
        });

        col.Item().PaddingVertical(2).LineHorizontal(0.6f).LineColor(Black);
    }

    // ── 4 & 5. Discount / VAT / Net Amount / Net Payable / Paid / Due ─────

    private static void ComposeTotals(ColumnDescriptor col, SaleMasterDTO sale, IReadOnlyDictionary<int, string>? paymentTypeNames = null)
    {
        var discount = (sale.CampaignDiscount ?? 0) + (sale.MembershipDiscount ?? 0)
                     + (sale.InvoiceDiscount ?? 0);
        var exchange = sale.ExchangeAmount ?? 0;
        var vat = sale.VatAmount ?? 0;
        var netAmount = sale.NetAmount;
        var paid = sale.PaidAmount ?? sale.PaymentTransactions?.Sum(p => p.PaidAmount ?? 0) ?? 0;
        var due = sale.DueAmount ?? Math.Max(0, netAmount - paid);

        void TotalRow(string label, string value, bool bold = false)
        {
            col.Item().Row(row =>
            {
                var labelText = row.RelativeItem().Text(label).FontSize(9.5f).FontColor(GreyDark);
                var valueText = row.ConstantItem(66).AlignRight().Text(value).FontSize(9.5f);
                if (bold)
                {
                    labelText.Bold().FontColor(Black);
                    valueText.Bold();
                }
            });
        }

        TotalRow("Discount", $"{discount:N2}");
        // The exchange credit is a returned-goods allowance, not a price
        // discount, so it is reported on its own line whenever one applies.
        if (exchange > 0)
        {
            TotalRow("Exchange Credit", $"{exchange:N2}");
            if (!string.IsNullOrWhiteSpace(sale.ReturnInvoiceNo))
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("  Exchanged Against").FontSize(8.5f).FontColor(GreyMedium);
                    row.ConstantItem(66).AlignRight().Text(sale.ReturnInvoiceNo).FontSize(8.5f).FontColor(GreyMedium);
                });
        }
        TotalRow("VAT", $"{vat:N2}");
        TotalRow("Net Amount", $"{netAmount:N2}");

        col.Item().PaddingVertical(1.5f).LineHorizontal(0.5f).LineColor(Black);

        TotalRow("Net Payable", $"{netAmount:N2}", bold: true);
        TotalRow("Paid Amount", $"{paid:N2}");
        TotalRow("Due Amount", $"{due:N2}", bold: due > 0);
    }

    // ── 5.5. Payment breakdown (per payment method, like the payment dialog) ──
    private static void ComposePayments(ColumnDescriptor col, SaleMasterDTO sale, IReadOnlyDictionary<int, string>? paymentTypeNames)
    {
        var payments = sale.PaymentTransactions;
        if (payments == null || payments.Count == 0) return;

        col.Item().PaddingTop(2).LineHorizontal(0.6f).LineColor(Black);
        col.Item().PaddingTop(2).Text("Payments").Bold().FontSize(9.5f).FontColor(GreyDark);

        foreach (var p in payments)
        {
            string name;
            if (paymentTypeNames != null && paymentTypeNames.TryGetValue(p.TransactionTypeId, out var n) && !string.IsNullOrWhiteSpace(n))
                name = n;
            else if (p.TransactionTypeId == 0)
                name = "Cash";
            else
                name = $"Account #{p.TransactionTypeId}";

            var refNo = string.IsNullOrWhiteSpace(p.TransactionNo) ? "" : $" ({p.TransactionNo})";
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"{name}{refNo}").FontSize(9);
                row.ConstantItem(66).AlignRight().Text($"{(p.PaidAmount ?? 0):N2}").FontSize(9);
            });
        }

        // Cash tendered / change returned (only when recorded).
        var tendered = sale.CashTendered ?? 0;
        var balance = sale.BalanceReturned ?? 0;
        if (tendered > 0)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Cash Tendered").FontSize(9);
                row.ConstantItem(66).AlignRight().Text($"{tendered:N2}").FontSize(9);
            });
        }
        if (balance > 0)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Balance (returned)").FontSize(9);
                row.ConstantItem(66).AlignRight().Text($"{balance:N2}").FontSize(9);
            });
        }

        // Total paid line at the end of the payment block.
        var totalPaid = sale.TotalPaid > 0 ? sale.TotalPaid : payments.Sum(p => p.PaidAmount ?? 0);
        col.Item().PaddingTop(1).Row(row =>
        {
            row.RelativeItem().Text("Total Paid").FontSize(9.5f).Bold();
            row.ConstantItem(66).AlignRight().Text($"{totalPaid:N2}").FontSize(9.5f).Bold();
        });

        col.Item().PaddingTop(2).LineHorizontal(0.6f).LineColor(Black);
    }

    private static void ComposeTerms(ColumnDescriptor col, string terms)
    {
        if (string.IsNullOrWhiteSpace(terms))
            return;

        col.Item().PaddingTop(3).LineHorizontal(0.6f).LineColor(Black);
        col.Item().PaddingTop(2).Text("Terms & Conditions").Bold().FontSize(9);
        col.Item().Text(terms).FontSize(8.5f).FontColor(GreyDark);
    }

    private static void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().PaddingTop(6).AlignCenter().Text("*** Thank You ***").FontSize(9.5f);
        col.Item().AlignCenter().Text("Powered by AFiS ERP").FontSize(8).FontColor(GreyMedium);
    }
}
