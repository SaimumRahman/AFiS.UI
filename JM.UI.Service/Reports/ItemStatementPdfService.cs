using JM.UI.Entities.Model.Stock;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JM.UI.Service.Reports;

public class ItemStatementPdfService
{
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";
    private const string GreyLighten2 = "#EEEEEE";
    private const string GreyMedium = "#9E9E9E";
    private const string GreyDarken1 = "#616161";
    private const string Green = "#27ae60";
    private const string Red = "#c0392b";
    private const string Neutral = "#7f8c8d";

    private const float HeaderFont = 8f;
    private const float BodyFont = 8f;
    private const float TotalFont = 8.5f;

    private const string DefaultStoreAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.";

    public byte[] Generate(
        ItemStatementDTO statement,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        string printedBy = "-")
        => BuildDocument(statement, dateFrom, dateTo, storeName, storeAddress, printedBy).GeneratePdf();

    public Document BuildDocument(
        ItemStatementDTO statement,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string storeName = "ASIA FASHION",
        string storeAddress = DefaultStoreAddress,
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        statement ??= new ItemStatementDTO();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(h => ComposeHeader(h, storeName, storeAddress, dateFrom, dateTo));

                page.Content().Element(c => ComposeContent(c, statement, dateFrom, dateTo));

                page.Footer().Element(f => ComposeFooter(f, printedBy));
            });
        });
    }

    private void ComposeHeader(IContainer container, string storeName, string storeAddress,
        DateTime? dateFrom, DateTime? dateTo)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text(storeName).Bold().FontSize(13);
            col.Item().AlignCenter().Text(storeAddress).FontSize(9);
            col.Item().AlignCenter().Text("COMPREHENSIVE ITEM STATEMENT").Bold().FontSize(10);
            col.Item().AlignCenter().Text("Multi-Branch Matrix View").FontSize(9);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy} To {dateTo:dd-MMM-yyyy}"
                : "ALL TIME";
            col.Item().AlignCenter().Text(range).FontSize(9);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    private void ComposeContent(IContainer container, ItemStatementDTO statement,
        DateTime? dateFrom, DateTime? dateTo)
    {
        container.PaddingTop(6).Column(col =>
        {
            // Item details block
            col.Item().Border(0.5f).BorderColor(GreyMedium).Padding(6).Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                MetaCell(t, "Product Name:", statement.ProductName);
                MetaCell(t, "Base Barcode:", statement.BaseBarcode);
                MetaCell(t, "Purchase Date:", statement.PurchaseDate.HasValue ? statement.PurchaseDate.Value.ToString("dd-MMM-yyyy") : "-");
                MetaCell(t, "By (Entry):", statement.PurchasedBy);
                MetaCell(t, "Group:", statement.GroupName);
                MetaCell(t, "Brand:", statement.BrandName);
                MetaCell(t, "Supplier:", statement.SupplierName);
                MetaCell(t, "Unit:", statement.UnitName);
                MetaCell(t, "Purchase Price:", FormatAmount(statement.PurchasePrice));
                MetaCell(t, "Sale Price:", FormatAmount(statement.SalePrice));
                MetaCell(t, "Color:", statement.ColorName);
                MetaCell(t, "Country:", statement.OriginName);
                MetaCell(t, "Catalogue:", statement.CatalogueName);
                // pad to multiple of 4
                var filled = 13;
                var rem = (4 - (filled % 4)) % 4;
                for (int i = 0; i < rem; i++) t.Cell().Text("");
            });

            col.Item().PaddingTop(8);

            if (statement.Sizes == null || statement.Sizes.Count == 0)
            {
                col.Item().AlignCenter().Text("No stock movements found for the given barcode and date range.").FontSize(9);
                return;
            }

            foreach (var size in statement.Sizes)
            {
                col.Item().PaddingTop(4).Text($"Barcode: {size.Barcode} (Size: {size.SizeName})")
                    .Bold().FontSize(9.5f).FontColor(GreyDarken1);

                col.Item().PaddingTop(2).Table(t =>
                {
                    var branches = size.Branches ?? new List<string>();
                    // columns: Date, Description, branches..., Total
                    t.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(62);   // Date
                        c.RelativeColumn(2);    // Description
                        foreach (var _ in branches) c.ConstantColumn(46);
                        c.ConstantColumn(52);   // Total Stock
                    });

                    // header
                    HeaderCell(t, "Date");
                    HeaderCell(t, "Description / Reference");
                    foreach (var b in branches) HeaderCell(t, b);
                    HeaderCell(t, "Total Stock", right: true);

                    // opening balance row
                    t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                        .Text((size.Transactions != null && size.Transactions.Count > 0
                            ? size.Transactions[0].Date.ToString("dd-MMM-yy")
                            : (dateFrom.HasValue ? dateFrom.Value.ToString("dd-MMM-yy") : "-")))
                        .FontSize(BodyFont);
                    t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                        .Text("Opening Balance").Bold().FontSize(BodyFont);
                    foreach (var b in branches)
                    {
                        var ov = size.OpeningByBranch != null && size.OpeningByBranch.TryGetValue(b, out var v) ? v : 0m;
                        t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                            .AlignCenter().Text(FormatAmount(ov)).FontSize(BodyFont).FontColor(Neutral);
                    }
                    t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text(FormatAmount(size.TotalOpening)).Bold().FontSize(BodyFont);

                    // transactions (skip the synthetic opening row)
                    var txns = (size.Transactions ?? new List<ItemStatementTxnDTO>()).Skip(1).ToList();
                    foreach (var txn in txns)
                    {
                        t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                            .Text(txn.Date.ToString("dd-MMM-yy")).FontSize(BodyFont);
                        t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                            .Text(string.IsNullOrWhiteSpace(txn.Description) ? "-" : txn.Description).FontSize(BodyFont);

                        foreach (var b in branches)
                        {
                            var d = txn.BranchDelta != null && txn.BranchDelta.TryGetValue(b, out var dv) ? dv : 0m;
                            if (d > 0)
                                t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                    .AlignCenter().Text("+" + FormatNum(d)).FontSize(BodyFont).FontColor(Green).Bold();
                            else if (d < 0)
                                t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                    .AlignCenter().Text(FormatNum(d)).FontSize(BodyFont).FontColor(Red).Bold();
                            else
                                t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                                    .AlignCenter().Text("-").FontSize(BodyFont).FontColor(Neutral);
                        }

                        t.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                            .AlignRight().Text(FormatAmount(txn.RunningTotal)).FontSize(BodyFont);
                    }

                    // closing balance row
                    t.Cell().ColumnSpan(2).Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"Closing Balance (Size {size.SizeName}):").Bold().FontSize(TotalFont);
                    foreach (var b in branches)
                    {
                        var cv = size.ClosingByBranch != null && size.ClosingByBranch.TryGetValue(b, out var v) ? v : 0m;
                        t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                            .AlignCenter().Text(FormatAmount(cv)).Bold().FontSize(TotalFont);
                    }
                    t.Cell().Background(GreyLighten2).Border(0.5f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text(FormatAmount(size.TotalClosing)).Bold().FontSize(TotalFont);
                });

                col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(GreyLighten2);
            }
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

    private static void MetaCell(TableDescriptor table, string label, string value)
    {
        table.Cell().Padding(2).Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(7.5f));
            text.Span(label).Bold().FontColor(GreyDarken1);
            text.Span(" " + (string.IsNullOrWhiteSpace(value) ? "-" : value));
        });
    }

    private static void HeaderCell(TableDescriptor table, string label, bool right = false)
    {
        var cell = table.Cell()
            .Background(GreyLighten3)
            .Border(0.5f).BorderColor(GreyMedium)
            .Padding(2);
        if (right) cell.AlignRight().Text(label).Bold().FontSize(HeaderFont);
        else cell.Text(label).Bold().FontSize(HeaderFont);
    }

    private static string FormatAmount(decimal value)
        => value == 0 ? "-" : value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatNum(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);
}
