using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.PurchaseItems;

namespace JM.UI.Service.Reports;

public class PurchaseReportService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";   // light header / subtotal bg
    private const string GreyLighten2 = "#EEEEEE";   // challan-total bg
    private const string GreyMedium = "#9E9E9E";   // border
    private const string GreyDarken1 = "#616161";   // small note text
    private const string GreyLighten1 = "#E0E0E0";   // separator line

    public byte[] GeneratePurchaseDetailReport(
        IEnumerable<PurchaseSummaryDTO> purchases,
        Dictionary<int, List<PurchaseItemDTO>> purchaseItemsCache,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM",
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(header =>
                    ComposeHeader(header, storeName, storeAddress, dateFrom, dateTo));

                page.Content().Element(content =>
                    ComposeContent(content, purchases, purchaseItemsCache));

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(7);
                    text.CurrentPageNumber().FontSize(7);
                    text.Span(" of ").FontSize(7);
                    text.TotalPages().FontSize(7);
                });
            });
        });

        return document.GeneratePdf();
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
                ? $"DATE RANGE- {dateFrom:dd-MMM-yyyy} TO {dateTo:dd-MMM-yyyy}"
                : $"DATE RANGE- {DateTime.Today:dd-MMM-yyyy} TO {DateTime.Today:dd-MMM-yyyy}";

            col.Item().AlignCenter().Text(range).FontSize(9);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(
        IContainer container,
        IEnumerable<PurchaseSummaryDTO> purchases,
        Dictionary<int, List<PurchaseItemDTO>> cache)
    {
        container.PaddingTop(4).Column(col =>
        {
            foreach (var purchase in purchases)
            {
                var items = cache.GetValueOrDefault(purchase.Id) ?? new List<PurchaseItemDTO>();

                // ── Challan header row ────────────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(80);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                    });

                    table.Cell().Background(GreyLighten3).Padding(2)
                        .Text("P.Challan No:").Bold().FontSize(8);
                    table.Cell().Background(GreyLighten3).Padding(2)
                        .Text(purchase.BillInvoiceNumber ?? "").Bold().FontSize(8);
                    table.Cell().Background(GreyLighten3).Padding(2)
                        .Text("");
                });

                // ── Supplier / Bill info row ──────────────────────────────
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(100);
                        c.RelativeColumn(3);
                        c.ConstantColumn(50);
                        c.RelativeColumn(2);
                        c.ConstantColumn(55);
                        c.RelativeColumn(2);
                    });

                    table.Cell().Padding(2).Text("Supplier name:").Bold().FontSize(8);
                    table.Cell().Padding(2).Text(purchase.SupplierName ?? "").FontSize(8);
                    table.Cell().Padding(2).Text("Bill:").Bold().FontSize(8);
                    table.Cell().Padding(2).Text(purchase.BillInvoiceNumber ?? "").FontSize(8);
                    table.Cell().Padding(2).Text("BillDt:").Bold().FontSize(8);
                    table.Cell().Padding(2)
                        .Text((purchase.BillDate ?? purchase.PurchaseDate).ToString("dd-MMM-yy")).FontSize(8);
                });

                // ── Groups ────────────────────────────────────────────────
                var groups = items.GroupBy(i => i.GroupName ?? "Uncategorized");

                foreach (var group in groups)
                {
                    // Group label
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => c.RelativeColumn());
                        table.Cell().Padding(2)
                            .Text($"Group: {group.Key}").Bold().FontSize(8);
                    });

                    // Items table
                    col.Item().Table(table =>
                    {
                        DefineItemTableColumns(table);
                        AddTableHeader(table);

                        decimal subQty = 0;
                        decimal subPurTotal = 0;
                        decimal subSaleTotal = 0;

                        foreach (var item in group)
                        {
                            var purTotal = item.Quantity * item.PurchasePrice;
                            var saleTotal = item.Quantity * (item.SalePrice ?? 0);

                            subQty += item.Quantity;
                            subPurTotal += purTotal;
                            subSaleTotal += saleTotal;

                            // Main data row
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .Text(item.Barcode ?? "").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .Text(item.ItemName ?? "").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignRight().Text($"{item.PurchasePrice:N2}").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignRight().Text($"{item.Quantity:N2}").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignCenter().Text(item.MesurementUnitName ?? "").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignRight().Text($"{purTotal:N2}").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignRight().Text($"{item.SalePrice:N2}").FontSize(7.5f);
                            table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                .AlignRight().Text($"{saleTotal:N2}").FontSize(7.5f);

                            // Price breakdown sub-row  (Pp-xxx+CC-xxx+Vat-0)
                            var breakdown = BuildPriceBreakdown(item);
                            if (!string.IsNullOrWhiteSpace(breakdown))
                            {
                                table.Cell().ColumnSpan(8)
                                    .PaddingLeft(8).PaddingBottom(1)
                                    .Text(breakdown)
                                    .FontSize(6.5f).FontColor(GreyDarken1);
                            }
                        }

                        AddSubTotalRow(table, subQty, subPurTotal, subSaleTotal);
                    });
                }

                // ── Challan Total ─────────────────────────────────────────
                var challanPurTotal = items.Sum(i => i.Quantity * i.PurchasePrice);
                var challanSaleTotal = items.Sum(i => i.Quantity * (i.SalePrice ?? 0));
                var challanQty = items.Sum(i => i.Quantity);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.ConstantColumn(55);
                        c.ConstantColumn(60);
                        c.ConstantColumn(60);
                        c.ConstantColumn(60);
                        c.ConstantColumn(65);
                    });

                    table.Cell().ColumnSpan(2).Background(GreyLighten2).Padding(2)
                        .AlignRight().Text("Challan Total").Bold().FontSize(8);
                    table.Cell().Background(GreyLighten2).Padding(2)
                        .AlignRight().Text($"{challanQty:N2}").Bold().FontSize(8);
                    table.Cell().Background(GreyLighten2).Padding(2)
                        .AlignRight().Text($"{challanPurTotal:N2}").Bold().FontSize(8);
                    table.Cell().Background(GreyLighten2).Padding(2).Text("");
                    table.Cell().Background(GreyLighten2).Padding(2)
                        .AlignRight().Text($"{challanSaleTotal:N2}").Bold().FontSize(8);
                });

                col.Item().PaddingVertical(4)
                    .LineHorizontal(0.5f).LineColor(GreyLighten1);
            }
        });
    }

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static void DefineItemTableColumns(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(65);   // Code / Barcode
            c.RelativeColumn(3);    // ItemName
            c.ConstantColumn(60);   // Cost Price
            c.ConstantColumn(45);   // Quantity
            c.ConstantColumn(35);   // UoM
            c.ConstantColumn(65);   // Pur.Total
            c.ConstantColumn(60);   // Sale Price
            c.ConstantColumn(65);   // Sale Total
        });
    }

    private void AddTableHeader(TableDescriptor table)
    {
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .Text("Code").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .Text("ItemName").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignRight().Text("Cost Price").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignRight().Text("Quantity").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignCenter().Text("UoM").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignRight().Text("Pur.Total").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignRight().Text("Sale Price").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3)
            .AlignRight().Text("Sale Total").Bold().FontSize(8);
    }

    private void AddSubTotalRow(
        TableDescriptor table,
        decimal qty,
        decimal purTotal,
        decimal saleTotal)
    {
        table.Cell().ColumnSpan(3).Background(GreyLighten3).Padding(2)
            .AlignRight().Text("SubTotal").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Padding(2)
            .AlignRight().Text($"{qty:N2}").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Padding(2).Text("");
        table.Cell().Background(GreyLighten3).Padding(2)
            .AlignRight().Text($"{purTotal:N2}").Bold().FontSize(8);
        table.Cell().Background(GreyLighten3).Padding(2).Text("");
        table.Cell().Background(GreyLighten3).Padding(2)
            .AlignRight().Text($"{saleTotal:N2}").Bold().FontSize(8);
    }

    // ── PRICE BREAKDOWN ───────────────────────────────────────────────────

    /// <summary>
    /// Builds e.g. "Pp-850+CC-100+TR-20+OC-200+Vat-0"
    /// </summary>
    private static string BuildPriceBreakdown(PurchaseItemDTO item)
    {
        var parts = new List<string>
        {
            $"Pp-{item.PurchasePrice:0}"
        };

        if (item.CarryingCost is { } cc && cc != 0)
            parts.Add($"CC-{cc:0}");

        if (item.TransportCost is { } tr && tr != 0)
            parts.Add($"TR-{tr:0}");

        if (item.OtherCost is { } oc && oc != 0)
            parts.Add($"OC-{oc:0}");

        if (item.OperationalCost is { } op && op != 0)
            parts.Add($"OP-{op:0}");

        parts.Add($"Vat-{item.VatAmount ?? 0:0}");

        return string.Join("+", parts);
    }
}
