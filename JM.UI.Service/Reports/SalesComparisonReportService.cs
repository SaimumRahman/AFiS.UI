using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Service.Reports;

public class SalesComparisonReportService
{
    public byte[] GenerateSalesComparisonReport(
        IEnumerable<SalesComparisonPivotRow> rows,
        IEnumerable<SalesComparisonStoreGroup> stores,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string reportType = "Date Range",
        string printedBy = "-")
        => BuildDocument(rows, stores, dateFrom, dateTo, reportType, printedBy).GeneratePdf();

    public Document BuildDocument(
        IEnumerable<SalesComparisonPivotRow> rows,
        IEnumerable<SalesComparisonStoreGroup> stores,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string reportType = "Date Range",
        string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var data = (rows ?? Enumerable.Empty<SalesComparisonPivotRow>()).ToList();
        var storeList = (stores ?? Enumerable.Empty<SalesComparisonStoreGroup>()).ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(18);
                page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));

                page.Header().Element(h => ComposeHeader(h, dateFrom, dateTo, reportType));
                page.Content().Element(c => ComposeContent(c, data, storeList));
                page.Footer().Element(f => ComposeFooter(f, printedBy));
            });
        });
    }

    private void ComposeHeader(IContainer container, DateTime? dateFrom, DateTime? dateTo, string reportType)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("ASIA FASHION").Bold().FontSize(12);
            col.Item().AlignCenter().Text("HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.").FontSize(8.5f);
            col.Item().AlignCenter().Text("BRANCH SALES COMPARISON REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"{dateFrom:dd-MMM-yyyy}  TO  {dateTo:dd-MMM-yyyy}"
                : "ALL TIME";
            col.Item().AlignCenter().Text($"Period: {range}   |   View: {reportType}").FontSize(8.5f);
        });
    }

    private void ComposeContent(IContainer container, List<SalesComparisonPivotRow> data, List<SalesComparisonStoreGroup> stores)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.2f); // Period
                foreach (var _ in stores)
                {
                    columns.RelativeColumn(1f); // Total
                    columns.RelativeColumn(1f); // Disc
                    columns.RelativeColumn(1f); // Net
                }
                columns.RelativeColumn(1f); // Combined Total
                columns.RelativeColumn(1f); // Combined Disc
                columns.RelativeColumn(1f); // Combined Net
            });

            table.Header(header =>
            {
                // Row 1: Period (rowspan 2) + branch name spanning 3 + Combined Total spanning 3
                header.Cell().RowSpan(2).Background("#e9ecef").AlignMiddle()
                    .Text("Period").Bold().FontSize(7.5f);
                foreach (var store in stores)
                {
                    header.Cell().ColumnSpan(3).Background("#e9ecef").AlignCenter()
                        .Text(store.StoreName).Bold().FontSize(7.5f);
                }
                header.Cell().ColumnSpan(3).Background("#fff3cd").AlignCenter()
                    .Text("Combined Total").Bold().FontSize(7.5f);

                // Row 2: Total / Disc / Net under each branch and under Combined Total
                foreach (var _ in stores.Concat(new[] { new SalesComparisonStoreGroup { StoreName = "Combined" } }))
                {
                    header.Cell().Background("#f8f9fa").AlignCenter().Text("Total").FontSize(7f);
                    header.Cell().Background("#f8f9fa").AlignCenter().Text("Disc.").FontSize(7f).FontColor("#dc3545");
                    header.Cell().Background("#f8f9fa").AlignCenter().Text("Net").FontSize(7f).Bold();
                }
            });

            // Body rows
            foreach (var row in data)
            {
                table.Cell().Text(row.PeriodLabel).FontSize(7.5f);
                foreach (var store in stores)
                {
                    var cell = row.GetCell(store.StoreName);
                    table.Cell().AlignRight().Text(cell.Total.ToString("N0")).FontSize(7.5f);
                    table.Cell().AlignRight().Text(cell.Discount.ToString("N0")).FontSize(7.5f).FontColor("#dc3545");
                    table.Cell().AlignRight().Text(cell.Net.ToString("N0")).FontSize(7.5f).Bold();
                }
                table.Cell().AlignRight().Background("#f0f7ff").Text(row.CombinedTotal.ToString("N0")).FontSize(7.5f);
                table.Cell().AlignRight().Background("#f0f7ff").Text(row.CombinedDiscount.ToString("N0")).FontSize(7.5f).FontColor("#dc3545");
                table.Cell().AlignRight().Background("#f0f7ff").Text(row.CombinedNet.ToString("N0")).FontSize(7.5f).Bold();
            }

            // YTD total row
            if (data.Count > 0)
            {
                var ytdTotal = data.Sum(r => r.CombinedTotal);
                var ytdDisc = data.Sum(r => r.CombinedDiscount);
                var ytdNet = data.Sum(r => r.CombinedNet);

                table.Cell().Background("#e9ecef").Text("YTD Total").Bold().FontSize(7.5f);
                foreach (var store in stores)
                {
                    var t = data.Sum(r => r.GetCell(store.StoreName).Total);
                    var d = data.Sum(r => r.GetCell(store.StoreName).Discount);
                    var n = data.Sum(r => r.GetCell(store.StoreName).Net);
                    table.Cell().Background("#e9ecef").AlignRight().Text(t.ToString("N0")).Bold().FontSize(7.5f);
                    table.Cell().Background("#e9ecef").AlignRight().Text(d.ToString("N0")).Bold().FontSize(7.5f).FontColor("#dc3545");
                    table.Cell().Background("#e9ecef").AlignRight().Text(n.ToString("N0")).Bold().FontSize(7.5f);
                }
                table.Cell().Background("#dbeafe").AlignRight().Text(ytdTotal.ToString("N0")).Bold().FontSize(7.5f);
                table.Cell().Background("#dbeafe").AlignRight().Text(ytdDisc.ToString("N0")).Bold().FontSize(7.5f).FontColor("#dc3545");
                table.Cell().Background("#dbeafe").AlignRight().Text(ytdNet.ToString("N0")).Bold().FontSize(8f);
            }
        });
    }

    private void ComposeFooter(IContainer container, string printedBy)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text($"Printed by: {printedBy}").FontSize(8f);
            row.RelativeItem().AlignRight().Text($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm}").FontSize(8f);
        });
    }
}
