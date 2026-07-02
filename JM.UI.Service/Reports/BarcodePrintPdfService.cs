using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JM.UI.Entities.Model.Barcodes;
using QRCoder;

namespace JM.UI.Service.Reports;

public class BarcodePrintPdfService
{

    public byte[] GeneratePdf(
        List<BarcodePrintItemDTO> items,
        List<string> fields,
        decimal labelWidthMm,
        decimal labelHeightMm)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        if (items.Count == 0)
            return Array.Empty<byte>();

        float lw = (float)labelWidthMm;
        float lh = (float)labelHeightMm;

        var allLabels = new List<(BarcodePrintItemDTO Item, int Copy, int Of)>();
        foreach (var item in items)
            for (int c = 1; c <= item.PrintQty; c++)
                allLabels.Add((item, c, item.PrintQty));

        var document = Document.Create(d =>
        {
            foreach (var (item, copy, of) in allLabels)
            {
                d.Page(page =>
                {
                    page.Size(lw, lh, Unit.Millimetre);
                    page.MarginLeft(3, Unit.Millimetre);
                    page.MarginRight(3, Unit.Millimetre);
                    page.MarginTop(2, Unit.Millimetre);
                    page.MarginBottom(2, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

                    page.Content().Element(c =>
                        ComposeLabelInCell(c, item, fields, copy, of, lw, lh));
                });
            }
        });

        return document.GeneratePdf();
    }

    private static void ComposeLabelInCell(
        IContainer cell,
        BarcodePrintItemDTO item,
        List<string> fields,
        int copy,
        int of,
        float lw,
        float lh)
    {
        float qrSizeMm = Math.Min(lh * 0.42f, lw * 0.38f);
        float qrSizeSvg = Math.Max(60, (int)(qrSizeMm * 3));
        var qrData = item.ReturnRefNo ?? item.BarcodeValue ?? "";

        cell.Padding(1f)
            .Column(col =>
            {
                if (fields.Contains("ProductName", StringComparer.OrdinalIgnoreCase))
                    col.Item().PaddingTop(0.3f).Text(item.ProductName ?? "")
                        .Bold().FontSize(7f);

                if (fields.Contains("Brand", StringComparer.OrdinalIgnoreCase))
                    col.Item().Text(string.IsNullOrWhiteSpace(item.Brand) ? "\u2014" : item.Brand)
                        .FontSize(6f);

                col.Item().PaddingTop(0.5f).Row(r =>
                {
                    r.AutoItem().Element(q =>
                        q.Width(qrSizeMm, Unit.Millimetre)
                         .Height(qrSizeMm, Unit.Millimetre)
                         .Svg(_ => GenerateQrSvg(qrData, (int)qrSizeSvg)));

                    r.RelativeItem().PaddingLeft(1f).Column(d =>
                    {
                        d.Item().Text(item.BarcodeValue ?? "")
                            .FontSize(7f).Bold();

                        if (fields.Contains("Price", StringComparer.OrdinalIgnoreCase))
                            d.Item().Text(item.Price.HasValue ? $"\u09F3 {item.Price:N2}" : "\u09F3 \u2014")
                                .FontSize(6f).Bold();

                        if (fields.Contains("UOM", StringComparer.OrdinalIgnoreCase) ||
                            fields.Contains("UoM", StringComparer.OrdinalIgnoreCase))
                            d.Item().Text(string.IsNullOrWhiteSpace(item.UoM) ? "\u2014" : item.UoM)
                                .FontSize(5f);
                    });
                });

                if (!string.IsNullOrWhiteSpace(item.ReturnRefNo))
                    col.Item().PaddingTop(0.3f).Text(item.ReturnRefNo)
                        .FontSize(6f);

                col.Item().PaddingTop(0.3f).Row(b =>
                {
                    b.AutoItem().Text(item.GroupName ?? "")
                        .FontSize(6f);
                    b.RelativeItem().AlignRight().Text($"{copy}/{of}")
                        .FontSize(6f);
                });
            });
    }

    private static string GenerateQrSvg(string data, int size)
    {
        if (string.IsNullOrWhiteSpace(data))
            data = "\u200B";

        var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        var svg = new SvgQRCode(qrData);
        return svg.GetGraphic(size);
    }
}
