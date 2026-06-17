using System;
using System.Collections.Generic;
using System.Text;
using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.Transfer;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JM.UI.Service.Reports
{
    /// <summary>
    /// Generates a Stock Transfer Challan PDF report using QuestPDF.
    /// Install via NuGet: QuestPDF
    /// License: QuestPDF Community License (free for small businesses / open source)
    /// </summary>
    public class StockTransferChallanService
    {
        public byte[] GenerateReport(CompanyDTO company, IEnumerable<TransferSummaryDTO> transferItems)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var items = transferItems.ToList();
                if (items.Count == 0)
                    return Array.Empty<byte>();

                var first = items.First();

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30, Unit.Point);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        page.Content().Column(col =>
                        {
                            // ── COMPANY HEADER ──────────────────────────────────────
                            col.Item().AlignCenter().Column(header =>
                            {
                                header.Item().Text(company.Name)
                                    .Bold().FontSize(16).AlignCenter();

                                header.Item().Text(company.Address)
                                    .FontSize(8).AlignCenter();

                                header.Item().Text($"Call- {company.Contact}")
                                    .FontSize(8).AlignCenter();

                                if (!string.IsNullOrWhiteSpace(company.VAT))
                                    header.Item().Text($"BIN No- {company.VAT}")
                                        .FontSize(8).AlignCenter();
                            });

                            col.Item().PaddingVertical(6).LineHorizontal(1);

                            // ── REPORT TITLE ─────────────────────────────────────────
                            col.Item().Text("STOCK TRANSFER CHALLAN")
                                .Bold().FontSize(12).AlignCenter();

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                // LEFT: From / To
                                row.RelativeItem().Column(left =>
                                {
                                    left.Item().Row(r =>
                                    {
                                        r.ConstantItem(40).Text("From :").Bold();
                                        r.RelativeItem().Column(fromCol =>
                                        {
                                            fromCol.Item().Text($"{first.FromStoreCode}  {first.FromStoreName}").Bold();
                                            if (!string.IsNullOrWhiteSpace(first.FromStoreAddress))
                                                fromCol.Item().Text(first.FromStoreAddress).FontSize(8);
                                            if (!string.IsNullOrWhiteSpace(first.FromStoreContact))
                                                fromCol.Item().Text($"Call-{first.FromStoreContact}").FontSize(8);
                                        });
                                    });

                                    left.Item().PaddingTop(4).Row(r =>
                                    {
                                        r.ConstantItem(40).Text("To :").Bold();
                                        r.RelativeItem().Column(toCol =>
                                        {
                                            toCol.Item().Text($"{first.ToStoreCode} : {first.ToStoreName}").Bold();
                                            if (!string.IsNullOrWhiteSpace(first.ToStoreAddress))
                                                toCol.Item().Text(first.ToStoreAddress).FontSize(8);
                                            if (!string.IsNullOrWhiteSpace(first.ToStoreContact))
                                                toCol.Item().Text($"Call: {first.ToStoreContact}").FontSize(8);
                                        });
                                    });

                                    if (!string.IsNullOrWhiteSpace(first.Comments))
                                    {
                                        left.Item().PaddingTop(4).Row(r =>
                                        {
                                            r.ConstantItem(60).Text("Remarks:").Bold();
                                            r.RelativeItem().Text(first.Comments);
                                        });
                                    }
                                });

                                // RIGHT: Challan meta
                                row.ConstantItem(200).Column(right =>
                                {
                                    right.Item().Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Challan #").AlignRight();
                                        r.RelativeItem().PaddingLeft(6)
                                            .Text(first.TransferNo).Bold().FontSize(11);
                                    });

                                    right.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Challan Date:").AlignRight();
                                        r.RelativeItem().PaddingLeft(6)
                                            .Text(first.TransferDate.ToString("dd-MMM-yyyy")).Bold();
                                    });

                                    right.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Print Date:").AlignRight();
                                        r.RelativeItem().PaddingLeft(6)
                                            .Text(DateTime.Now.ToString("dd-MMM-yyyy"));
                                    });

                                    right.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Print Time:").AlignRight();
                                        r.RelativeItem().PaddingLeft(6)
                                            .Text(DateTime.Now.ToString("HH:mm:ss"));
                                    });
                                });
                            });

                            col.Item().PaddingTop(10);

                            // ── ITEM TABLE ───────────────────────────────────────────
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(100); // Barcode
                                    cols.RelativeColumn(3);   // Item Name
                                    cols.RelativeColumn(1);   // Sale Price
                                    cols.ConstantColumn(60);  // QTY
                                    cols.ConstantColumn(70);  // Amount
                                });

                                // Header row
                                static IContainer HeaderCell(IContainer c) =>
                                    c.BorderBottom(1).BorderColor(Color.FromRGB(0, 0, 0))
                                     .BorderTop(1).BorderColor(Color.FromRGB(0, 0, 0))
                                     .PaddingVertical(4).PaddingHorizontal(3);

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Barcode").Bold();
                                    header.Cell().Element(HeaderCell).Text("Item Name").Bold();
                                    header.Cell().Element(HeaderCell).AlignRight().Text("Sale Price").Bold();
                                    header.Cell().Element(HeaderCell).AlignCenter().Text("QTY").Bold();
                                    header.Cell().Element(HeaderCell).AlignRight().Text("Amount").Bold();
                                });

                                // Category header: READYMADE (group label — static or dynamic)
                                table.Cell().ColumnSpan(5)
                                    .PaddingVertical(3).PaddingHorizontal(3)
                                    .Text("READYMADE").Bold().Underline();

                                // Data rows
                                foreach (var item in items)
                                {
                                    static IContainer DataCell(IContainer c) =>
                                        c.BorderBottom(0.5f).BorderColor(Color.FromRGB(128, 128, 128))
                                         .PaddingVertical(3).PaddingHorizontal(3);

                                    table.Cell().Element(DataCell).Text(item.Barcode ?? string.Empty).FontSize(8);
                                    table.Cell().Element(DataCell).Text(item.ItemName ?? string.Empty).FontSize(8);
                                    table.Cell().Element(DataCell).AlignRight()
                                        .Text((item.SalePrice ?? 0).ToString("N2")).FontSize(8);
                                    table.Cell().Element(DataCell).AlignCenter()
                                        .Text($"{item.IssueQty:N2} Pcs").FontSize(8);
                                    table.Cell().Element(DataCell).AlignRight()
                                        .Text(item.Amount.ToString("N2")).FontSize(8);
                                }

                                // Sub Total row
                                var subTotalQty = items.Sum(x => x.IssueQty);
                                var grandTotal = items.Sum(x => x.Amount);

                                static IContainer TotalCell(IContainer c) =>
                                    c.BorderTop(1).BorderColor(Color.FromRGB(0, 0, 0))
                                     .PaddingVertical(4).PaddingHorizontal(3);

                                table.Cell().ColumnSpan(3).Element(TotalCell)
                                    .AlignRight().Text("Sub Total:").Bold();
                                table.Cell().Element(TotalCell).AlignCenter()
                                    .Text(subTotalQty.ToString("N2")).Bold();
                                table.Cell().Element(TotalCell).AlignRight()
                                    .Text(grandTotal.ToString("N2")).Bold();

                                // Grand Total row
                                static IContainer GrandCell(IContainer c) =>
                                    c.BorderTop(0.5f).BorderColor(Color.FromRGB(128, 128, 128))
                                     .PaddingVertical(4).PaddingHorizontal(3);

                                table.Cell().ColumnSpan(3).Element(GrandCell)
                                    .AlignRight().Text("Grand Total:").Bold();
                                table.Cell().Element(GrandCell).AlignCenter()
                                    .Text(subTotalQty.ToString("N2")).Bold();
                                table.Cell().Element(GrandCell).AlignRight()
                                    .Text(grandTotal.ToString("N2")).Bold();
                            });

                            // ── SIGNATURE SECTION ────────────────────────────────────
                            col.Item().PaddingTop(40).Column(sig =>
                            {
                                // Posted By name centered above the line
                                if (!string.IsNullOrWhiteSpace(first.UserName))
                                {
                                    sig.Item().Row(nameRow =>
                                    {
                                        nameRow.RelativeItem();
                                        nameRow.RelativeItem().AlignCenter().Text(first.UserName);
                                        nameRow.RelativeItem();
                                        nameRow.RelativeItem();
                                    });
                                }

                                sig.Item().PaddingTop(4).Row(lineRow =>
                                {
                                    lineRow.RelativeItem().LineHorizontal(1);
                                    lineRow.ConstantItem(10);
                                    lineRow.RelativeItem().LineHorizontal(1);
                                    lineRow.ConstantItem(10);
                                    lineRow.RelativeItem().LineHorizontal(1);
                                    lineRow.ConstantItem(10);
                                    lineRow.RelativeItem().LineHorizontal(1);
                                    lineRow.ConstantItem(10);
                                    lineRow.RelativeItem().LineHorizontal(1);
                                });

                                sig.Item().PaddingTop(4).Row(labelRow =>
                                {
                                    labelRow.RelativeItem().AlignCenter().Text("Warehouse -\nReceived By").FontSize(8);
                                    labelRow.ConstantItem(10);
                                    labelRow.RelativeItem().AlignCenter().Text("Warehouse Incharge").FontSize(8);
                                    labelRow.ConstantItem(10);
                                    labelRow.RelativeItem().AlignCenter().Text("Posted By").FontSize(8);
                                    labelRow.ConstantItem(10);
                                    labelRow.RelativeItem().AlignCenter().Text("Branch -\nCounted By").FontSize(8);
                                    labelRow.ConstantItem(10);
                                    labelRow.RelativeItem().AlignCenter().Text("Branch -\nOperation Manager").FontSize(8);
                                });
                            });
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
