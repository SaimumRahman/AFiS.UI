using JM.UI.Entities.Model.Reporting_D;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JM.UI.Service.Reports
{
    public class SaleAnalysisReportPdfService
    {
        public byte[] Generate(SaleAnalysisReportDTO report, string printedBy = "-")
            => BuildDocument(report, printedBy).GeneratePdf();

        public Document BuildDocument(SaleAnalysisReportDTO report, string printedBy = "-")
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var rows = report?.Rows ?? new List<SaleAnalysisRowDTO>();
            var stores = report?.Stores ?? new List<StoreLiteDTO>();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(14);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ASIA FASHION").Bold().FontSize(12);
                        col.Item().AlignCenter().Text("Sale Analysis Report (Fast / Slow Selling)").Bold().FontSize(10);
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text($"Period: {report.FromDate:dd MMM yyyy} to {report.ToDate:dd MMM yyyy}");
                            r.RelativeItem().AlignRight().Text($"Printed: {DateTime.Now:dd MMM yyyy HH:mm}   By: {printedBy}");
                        });
                        col.Item().PaddingTop(2).Text(
                            $"Fast ≥ {report.FastThreshold} units   |   Slow ≤ {report.SlowThreshold} units   |   " +
                            $"Items: {report.Kpi.TotalItems}   Qty: {report.Kpi.TotalQty:N0}   Revenue: {report.Kpi.TotalRevenue:N2} BDT   " +
                            $"Fast: {report.Kpi.FastCount}   Slow: {report.Kpi.SlowCount}   Normal: {report.Kpi.NormalCount}   Zero-sales: {report.Kpi.ZeroSalesCount}")
                            .FontSize(7).Bold();
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(46);   // barcode
                            columns.RelativeColumn(2.0f); // item
                            columns.RelativeColumn(1.0f); // group
                            columns.RelativeColumn(1.0f); // origin
                            columns.RelativeColumn(1.0f); // catalogue
                            columns.ConstantColumn(52);   // sale price
                            columns.ConstantColumn(40);   // qty
                            columns.ConstantColumn(60);   // revenue
                            columns.ConstantColumn(40);   // stock
                            foreach (var st in stores) columns.ConstantColumn(34);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Text("Barcode").Bold();
                            h.Cell().Text("Item").Bold();
                            h.Cell().Text("Group").Bold();
                            h.Cell().Text("Origin").Bold();
                            h.Cell().Text("Catalogue").Bold();
                            h.Cell().Text("Sale Price").Bold().AlignRight();
                            h.Cell().Text("Qty").Bold().AlignRight();
                            h.Cell().Text("Revenue").Bold().AlignRight();
                            h.Cell().Text("Stock").Bold().AlignRight();
                            foreach (var st in stores) h.Cell().Text(st.Name).Bold().AlignCenter();
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Text(row.Barcode);
                            table.Cell().Text($"{row.ItemName}\n({row.Classification})");
                            table.Cell().Text(row.GroupName);
                            table.Cell().Text(row.Origin);
                            table.Cell().Text(row.Catalogue);
                            table.Cell().Text(row.SalePrice.ToString("N2")).AlignRight();
                            table.Cell().Text(row.QtySold.ToString("N0")).AlignRight();
                            table.Cell().Text(row.Revenue.ToString("N2")).AlignRight();
                            table.Cell().Text(row.CurrentStock.ToString("N0")).AlignRight();
                            foreach (var st in stores)
                            {
                                var q = row.BranchQtys.FirstOrDefault(b => b.StoreId == st.Id)?.Qty ?? 0;
                                table.Cell().Text(q.ToString("N0")).AlignRight();
                            }
                        }

                        // Footer totals
                        table.Footer(f =>
                        {
                            f.Cell().Text("TOTAL").Bold();
                            f.Cell().Text(rows.Count + " items").Bold();
                            f.Cell().Text("").Bold();
                            f.Cell().Text("").Bold();
                            f.Cell().Text("").Bold();
                            f.Cell().Text("").Bold();
                            f.Cell().Text(rows.Sum(r => r.QtySold).ToString("N0")).Bold().AlignRight();
                            f.Cell().Text(rows.Sum(r => r.Revenue).ToString("N2")).Bold().AlignRight();
                            f.Cell().Text(rows.Sum(r => r.CurrentStock).ToString("N0")).Bold().AlignRight();
                            foreach (var st in stores)
                            {
                                var q = rows.Sum(r => r.BranchQtys.FirstOrDefault(b => b.StoreId == st.Id)?.Qty ?? 0);
                                f.Cell().Text(q.ToString("N0")).Bold().AlignRight();
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
        }
    }
}
