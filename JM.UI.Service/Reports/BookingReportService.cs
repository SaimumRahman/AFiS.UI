using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JM.UI.Entities.Model.Reporting_D;

namespace JM.UI.Service.Reports;

public class BookingReportService
{
    // ── Colour constants (hex) — avoids any Colors.* namespace conflict ──
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";   // light header bg
    private const string GreyMedium = "#9E9E9E";     // border
    private const string GreyDarken1 = "#616161";    // small note text

    public byte[] GenerateBookingReport(
        IEnumerable<BookingReportDTO> bookings,
        string storeName = "ASIA FASHION",
        string storeAddress = "HOSSAIN PLAZA (1ST FLOOR), BANDARTILA, CHATTOGRAM.",
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
                    ComposeContent(content, bookings));

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
            col.Item().AlignCenter().Text("BOOKING REPORT").Bold().FontSize(10);

            var range = (dateFrom.HasValue && dateTo.HasValue)
                ? $"DATE RANGE: {dateFrom:dd-MMM-yy} to {dateTo:dd-MMM-yy}"
                : "DATE RANGE: ALL TIME";

            col.Item().AlignCenter().Text(range).FontSize(9);
            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, IEnumerable<BookingReportDTO> bookings)
    {
        container.PaddingTop(4).Column(col =>
        {
            col.Item().Table(table =>
            {
                DefineItemTableColumns(table);
                AddTableHeader(table);

                foreach (var row in bookings)
                {
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.BookingId).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.BookingDate.ToString("dd-MMM-yy")).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.Mobile).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.Name).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.ItemCode).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.ItemName).FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.Rate:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.Quantity:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.GrossAmount:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.DiscountAmount:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.PaidAmount:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .AlignRight().Text($"{row.DueBalance:N2}").FontSize(7.5f);
                    table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(2)
                        .Text(row.CreatedBy).FontSize(7.5f);
                }
            });

            // ── Footer note ───────────────────────────────────────────────
            col.Item().PaddingTop(6).Text(
                "NOTE: ONLY CURRENTLY BOOKED INVOICES ARE SHOWN HERE. ONCE THE BOOKED ITEMS ARE DELIVERED, IT WILL BE REMOVED FROM THIS LIST.")
                .FontSize(7).FontColor(GreyDarken1);
        });
    }

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static void DefineItemTableColumns(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(60);    // Booking ID
            c.ConstantColumn(60);    // Booking Date
            c.ConstantColumn(65);    // Mobile
            c.RelativeColumn(1f);    // Name
            c.ConstantColumn(50);    // Item Code
            c.RelativeColumn(1.1f);  // Item Name
            c.ConstantColumn(45);    // Rate
            c.ConstantColumn(38);    // Quantity
            c.ConstantColumn(52);    // Gross Amount
            c.ConstantColumn(52);    // Discount Amount
            c.ConstantColumn(52);    // Paid Amount
            c.ConstantColumn(52);    // Due Balance
            c.ConstantColumn(70);    // Created By
        });
    }

    private void AddTableHeader(TableDescriptor table)
    {
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Booking ID").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Booking Date").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Mobile").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Name").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Item Code").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Item Name").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Rate").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Quantity").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Gross Amount").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Discount Amount").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Paid Amount").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .AlignRight().Text("Due Balance").Bold().FontSize(7.5f);
        table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(2)
            .Text("Created By").Bold().FontSize(7.5f);
    }
}