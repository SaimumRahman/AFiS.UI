using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Service.Reports;

public class CollectionSummaryReportPdfService
{
    private const float BodyFont = 7.5f;

    public byte[] Generate(CollectionSummaryReportDTO report, string storeName = "-", DateTime? dateFrom = null, DateTime? dateTo = null, string printedBy = "-")
        => BuildDocument(report, storeName, dateFrom, dateTo, printedBy).GeneratePdf();

    public Document BuildDocument(CollectionSummaryReportDTO report, string storeName = "-", DateTime? dateFrom = null, DateTime? dateTo = null, string printedBy = "-")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var rows = report?.Rows ?? new List<CollectionRowDTO>();
        var channels = report?.Channels ?? new List<string>();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(16);
                page.DefaultTextStyle(x => x.FontSize(BodyFont).FontFamily("Arial"));

                page.Header().Element(h => ComposeHeader(h, storeName, dateFrom, dateTo, printedBy, rows.Count));
                page.Content().Element(c => ComposeContent(c, rows, channels));
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    private void ComposeHeader(IContainer container, string storeName, DateTime? dateFrom, DateTime? dateTo, string printedBy, int count)
    {
        var range = (dateFrom.HasValue && dateTo.HasValue) ? $"{dateFrom:dd-MMM-yyyy}  TO  {dateTo:dd-MMM-yyyy}" : "ALL TIME";
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("ASIA FASHION").Bold().FontSize(12);
            col.Item().AlignCenter().Text("Collection Summary Report").Bold().FontSize(10);
            col.Item().PaddingTop(4).Row(r =>
            {
                r.RelativeItem().Text($"Branch: {storeName}");
                r.RelativeItem().AlignRight().Text($"Period: {range}");
            });
            col.Item().Row(r =>
            {
                r.RelativeItem().Text($"Bills: {count}");
                r.RelativeItem().AlignRight().Text($"Printed: {DateTime.Now:dd-MMM-yyyy HH:mm}   By: {printedBy}");
            });
            col.Item().PaddingTop(4).LineHorizontal(0.5f);
        });
    }

    private void ComposeContent(IContainer container, List<CollectionRowDTO> rows, List<string> channels)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(24);
                    cd.ConstantColumn(70);
                    cd.ConstantColumn(40);
                    cd.ConstantColumn(70);
                    cd.RelativeColumn(1.2f);
                    cd.ConstantColumn(60);
                    cd.ConstantColumn(60);
                    foreach (var _ in channels) cd.ConstantColumn(55);
                });

                table.Header(h =>
                {
                    h.Cell().Text("#").Bold();
                    h.Cell().Text("Date & Time").Bold();
                    h.Cell().Text("Branch").Bold();
                    h.Cell().Text("Type").Bold();
                    h.Cell().Text("Invoice / Ref").Bold();
                    h.Cell().Text("Cashier").Bold();
                    h.Cell().Text("NET Paid").Bold().AlignRight();
                    foreach (var ch in channels) h.Cell().Text(ch).Bold().AlignRight();
                });

                int idx = 0;
                foreach (var row in rows)
                {
                    idx++;
                    table.Cell().Text(idx.ToString());
                    table.Cell().Text(row.CreatedDate.ToString("dd MMM yy HH:mm"));
                    table.Cell().Text(row.StoreCode);
                    table.Cell().Text(row.SourceType);
                    table.Cell().Text(row.RefNo);
                    table.Cell().Text(row.Cashier);
                    table.Cell().Text(row.NetPaid.ToString("N0")).AlignRight();
                    foreach (var ch in channels)
                    {
                        var sum = row.Payments.Where(p => p.Channel == ch).Sum(p => p.Amount);
                        table.Cell().Text(sum > 0 ? sum.ToString("N0") : "-").AlignRight();
                    }
                }

                table.Footer(f =>
                {
                    f.Cell().ColumnSpan(6).Text("Total Collection").Bold().AlignRight();
                    f.Cell().Text(rows.Sum(r => r.NetPaid).ToString("N0")).Bold().AlignRight();
                    foreach (var ch in channels)
                    {
                        var sum = rows.SelectMany(r => r.Payments).Where(p => p.Channel == ch).Sum(p => p.Amount);
                        f.Cell().Text(sum.ToString("N0")).Bold().AlignRight();
                    }
                });
            });

            col.Item().PaddingTop(8).Text("Account-Wise Collection Totals").Bold().FontSize(8.5f);
            col.Item().Row(r =>
            {
                foreach (var acc in rows.SelectMany(x => x.Payments).Select(p => p.AccountName).Distinct().OrderBy(a => a))
                {
                    var amt = rows.SelectMany(x => x.Payments).Where(p => p.AccountName == acc).Sum(p => p.Amount);
                    r.AutoItem().Padding(2).Border(0.5f).BorderColor("#e5e7eb").Padding(3).Text($"{acc}: {amt:N0}").FontSize(7.5f);
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.Span("Page ").FontSize(8);
            t.CurrentPageNumber().FontSize(8);
            t.Span(" of ").FontSize(8);
            t.TotalPages().FontSize(8);
        });
    }
}