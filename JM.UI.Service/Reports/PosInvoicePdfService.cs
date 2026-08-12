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
        string servedBy = "")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        int itemCount = sale.SaleDetails?.Count ?? 0;
        float widthMm = 80f;
        float heightMm = 120f + (itemCount * 6f);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(widthMm, heightMm, Unit.Millimetre);
                page.MarginLeft(3, Unit.Millimetre);
                page.MarginRight(3, Unit.Millimetre);
                page.MarginTop(4, Unit.Millimetre);
                page.MarginBottom(4, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    ComposeHeader(col, companyName, companyAddress, companyContact, companyVat);
                    ComposeInfo(col, sale, servedBy);
                    ComposeItems(col, sale);
                    ComposeTotals(col, sale);
                    ComposeFooter(col);
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── 1. Company details ────────────────────────────────────────────────

    private static void ComposeHeader(
        ColumnDescriptor col,
        string companyName,
        string companyAddress,
        string companyContact,
        string companyVat)
    {
        col.Item().AlignCenter().Text(companyName).Bold().FontSize(13);
        if (!string.IsNullOrWhiteSpace(companyAddress))
            col.Item().AlignCenter().Text(companyAddress).FontSize(7.5f);
        if (!string.IsNullOrWhiteSpace(companyContact))
            col.Item().AlignCenter().Text(companyContact).FontSize(7.5f);
        if (!string.IsNullOrWhiteSpace(companyVat))
            col.Item().AlignCenter().Text($"VAT Reg No: {companyVat}").FontSize(7.5f);

        col.Item().PaddingTop(2).AlignCenter().Text("SALES INVOICE").Bold().FontSize(10);
        col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Black);
    }

    // ── 2. Invoice No / Served by / Date / Customer ───────────────────────

    private static void ComposeInfo(ColumnDescriptor col, SaleMasterDTO sale, string servedBy)
    {
        void InfoRow(string label, string value)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(34).Text(label).Bold().FontSize(7.5f).FontColor(GreyDark);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(7.5f);
            });
        }

        InfoRow("Invoice No:", sale.InvoiceNo ?? "");
        InfoRow("Served by:", servedBy);
        InfoRow("Date:", sale.SalesDate != default ? sale.SalesDate.ToString("dd/MM/yyyy hh:mm tt") : DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
        InfoRow("Customer:", sale.CustomerName ?? "");
        InfoRow("Mobile:", sale.CustomerPhone ?? "");

        col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Black);
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
                c.ConstantColumn(48);
                c.ConstantColumn(48);
            });

            // Header
            table.Cell().BorderBottom(0.5f).BorderColor(Black).Padding(1).Text("Item").Bold().FontSize(7);
            table.Cell().BorderBottom(0.5f).BorderColor(Black).Padding(1).AlignRight().Text("Qty").Bold().FontSize(7);
            table.Cell().BorderBottom(0.5f).BorderColor(Black).Padding(1).AlignRight().Text("Rate").Bold().FontSize(7);
            table.Cell().BorderBottom(0.5f).BorderColor(Black).Padding(1).AlignRight().Text("Amount").Bold().FontSize(7);

            foreach (var item in sale.SaleDetails ?? new List<SaleDetailDTO>())
            {
                var barcode = item.Barcode ?? "";
                var name = item.ProductName ?? "";

                table.Cell().Padding(1).Column(c =>
                {
                    c.Item().Text(barcode).FontSize(7);
                    if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, barcode, StringComparison.OrdinalIgnoreCase))
                        c.Item().Text(name.Length > 28 ? name[..28] + ".." : name).FontSize(6.5f).FontColor(GreyMedium);
                });

                table.Cell().Padding(1).AlignRight().Text($"{item.Qty:0.##}").FontSize(7);
                table.Cell().Padding(1).AlignRight().Text($"{item.UnitPrice:N2}").FontSize(7);
                table.Cell().Padding(1).AlignRight().Text($"{item.TotalAmount:N2}").FontSize(7);
            }
        });

        col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Black);
    }

    // ── 4 & 5. Discount / VAT / Net Amount / Net Payable / Paid / Due ─────

    private static void ComposeTotals(ColumnDescriptor col, SaleMasterDTO sale)
    {
        var discount = (sale.CampaignDiscount ?? 0) + (sale.MembershipDiscount ?? 0)
                     + (sale.InvoiceDiscount ?? 0) + (sale.ExchangeAmount ?? 0);
        var vat = sale.VatAmount ?? 0;
        var netAmount = sale.NetAmount;
        var paid = sale.PaidAmount ?? sale.PaymentTransactions?.Sum(p => p.PaidAmount ?? 0) ?? 0;
        var due = sale.DueAmount ?? Math.Max(0, netAmount - paid);

        void TotalRow(string label, string value, bool bold = false)
        {
            col.Item().Row(row =>
            {
                var labelText = row.RelativeItem().Text(label).FontSize(8).FontColor(GreyDark);
                var valueText = row.ConstantItem(62).AlignRight().Text(value).FontSize(8);
                if (bold)
                {
                    labelText.Bold().FontColor(Black);
                    valueText.Bold();
                }
            });
        }

        TotalRow("Discount", $"{discount:N2}");
        TotalRow("VAT", $"{vat:N2}");
        TotalRow("Net Amount", $"{netAmount:N2}");

        col.Item().PaddingVertical(1.5f).LineHorizontal(0.5f).LineColor(Black);

        TotalRow("Net Payable", $"{netAmount:N2}", bold: true);
        TotalRow("Paid Amount", $"{paid:N2}");
        TotalRow("Due Amount", $"{due:N2}", bold: due > 0);
    }

    private static void ComposeFooter(ColumnDescriptor col)
    {
        col.Item().PaddingTop(6).AlignCenter().Text("*** Thank You ***").FontSize(8);
        col.Item().AlignCenter().Text("Powered by AFiS ERP").FontSize(6.5f).FontColor(GreyMedium);
    }
}
