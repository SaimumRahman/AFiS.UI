using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JM.UI.Entities.Model.DailyExpense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Service.Reports;

/// <summary>
/// Builds a PDF voucher for the daily expenses that were just posted to the
/// general ledger from the Post Ledger page.
/// </summary>
public class DailyExpensePostReportService
{
    // Colour constants (hex) — avoids any Colors.* namespace conflict.
    private const string Black = "#000000";
    private const string GreyLighten3 = "#F5F5F5";   // header / subtotal bg
    private const string GreyLighten2 = "#EEEEEE";   // total bg
    private const string GreyMedium = "#9E9E9E";     // border
    private const string GreyDarken1 = "#616161";    // small note text

    public byte[] GenerateDailyExpensePostReport(
        IEnumerable<DailyExpenseDTO> expenses,
        DateTime businessDay,
        DateTime businessWindowStart,
        DateTime businessWindowEnd,
        string storeName = "ASIA FASHION",
        string? storeAddress = null,
        string? postedByName = null,
        string title = "DAILY EXPENSE POSTING VOUCHER")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var expenseList = expenses?.ToList() ?? new List<DailyExpenseDTO>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Element(header => ComposeHeader(
                    header, storeName, storeAddress, businessDay,
                    businessWindowStart, businessWindowEnd, postedByName, title));

                page.Content().Element(content => ComposeContent(content, expenseList));

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
        string? storeAddress,
        DateTime businessDay,
        DateTime businessWindowStart,
        DateTime businessWindowEnd,
        string? postedByName,
        string title)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text(storeName).Bold().FontSize(13);

            if (!string.IsNullOrWhiteSpace(storeAddress))
                col.Item().AlignCenter().Text(storeAddress).FontSize(9);

            col.Item().AlignCenter().Text(title).Bold().FontSize(10);
            col.Item().AlignCenter()
                .Text($"Business Day: {businessDay:dd-MMM-yyyy}")
                .FontSize(9);
            col.Item().AlignCenter()
                .Text($"Window: {businessWindowStart:dd-MMM-yyyy hh:mm tt} to {businessWindowEnd.AddTicks(-1):dd-MMM-yyyy hh:mm tt}")
                .FontSize(8);

            col.Item().PaddingTop(2).Table(info =>
            {
                info.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                info.Cell().Padding(2)
                    .Text($"Store: {storeName}").FontSize(8);
                info.Cell().Padding(2)
                    .Text($"Posted By: {(string.IsNullOrWhiteSpace(postedByName) ? "-" : postedByName)}").FontSize(8);
                info.Cell().Padding(2).AlignRight()
                    .Text($"Printed: {DateTime.Now:dd-MMM-yyyy hh:mm tt}").FontSize(8);
            });

            col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Black);
        });
    }

    // ── CONTENT ───────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, List<DailyExpenseDTO> expenses)
    {
        container.PaddingTop(4).Column(col =>
        {
            col.Item().Table(table =>
            {
                DefineColumns(table);
                AddTableHeader(table);

                int index = 0;
                decimal total = 0;

                foreach (var expense in expenses)
                {
                    index++;
                    total += expense.Amount;

                    var ledger = string.IsNullOrWhiteSpace(expense.LedgerAccountCode)
                        ? (expense.LedgerAccountName ?? "-")
                        : $"{expense.LedgerAccountCode} - {expense.LedgerAccountName}";
                    var paidFrom = string.IsNullOrWhiteSpace(expense.AccountName) ? "Cash" : expense.AccountName;
                    var voucher = expense.VoucherNo.HasValue ? $"#{expense.VoucherNo}" : "-";

                    AddRow(table, index.ToString(),
                        expense.ExpenseDate.ToString("dd-MMM-yy HH:mm"),
                        expense.StoreName ?? "-",
                        ledger,
                        paidFrom,
                        expense.Description ?? "-",
                        voucher,
                        expense.Amount.ToString("N2"));
                }

                // Total row
                table.Cell().ColumnSpan(6).Background(GreyLighten2).Padding(3)
                    .AlignRight().Text("Total").Bold().FontSize(8);
                table.Cell().Background(GreyLighten2).Padding(3).Text("");
                table.Cell().Background(GreyLighten2).Padding(3)
                    .AlignRight().Text($"{total:N2}").Bold().FontSize(8);
            });

            col.Item().PaddingTop(6).Text(
                "Generated automatically after posting expenses to the general ledger.")
                .FontSize(7).FontColor(GreyDarken1);
        });
    }

    // ── TABLE HELPERS ─────────────────────────────────────────────────────

    private static void DefineColumns(TableDescriptor table)
    {
        table.ColumnsDefinition(c =>
        {
            c.ConstantColumn(22);   // #
            c.ConstantColumn(72);   // Entered At
            c.RelativeColumn(2);    // Store
            c.RelativeColumn(3);    // Ledger Account
            c.RelativeColumn(2);    // Paid From
            c.RelativeColumn(3);    // Description
            c.ConstantColumn(48);   // Voucher
            c.ConstantColumn(68);   // Amount
        });
    }

    private static void AddTableHeader(TableDescriptor table)
    {
        HeaderCell(table, "#", TextAlign.Left);
        HeaderCell(table, "Entered At", TextAlign.Left);
        HeaderCell(table, "Store", TextAlign.Left);
        HeaderCell(table, "Expense Ledger", TextAlign.Left);
        HeaderCell(table, "Paid From", TextAlign.Left);
        HeaderCell(table, "Description", TextAlign.Left);
        HeaderCell(table, "Voucher", TextAlign.Center);
        HeaderCell(table, "Amount", TextAlign.Right);
    }

    private static void HeaderCell(TableDescriptor table, string text, TextAlign align)
    {
        var cell = table.Cell().Background(GreyLighten3).Border(0.5f).BorderColor(GreyMedium).Padding(3);
        cell = align switch
        {
            TextAlign.Right => cell.AlignRight(),
            TextAlign.Center => cell.AlignCenter(),
            _ => cell.AlignLeft()
        };
        cell.Text(text).Bold().FontSize(8);
    }

    private static void AddRow(
        TableDescriptor table,
        string index,
        string enteredAt,
        string store,
        string ledger,
        string paidFrom,
        string description,
        string voucher,
        string amount)
    {
        DataCell(table, index, TextAlign.Left);
        DataCell(table, enteredAt, TextAlign.Left);
        DataCell(table, store, TextAlign.Left);
        DataCell(table, ledger, TextAlign.Left);
        DataCell(table, paidFrom, TextAlign.Left);
        DataCell(table, description, TextAlign.Left);
        DataCell(table, voucher, TextAlign.Center);
        DataCell(table, amount, TextAlign.Right);
    }

    private static void DataCell(TableDescriptor table, string text, TextAlign align)
    {
        var cell = table.Cell().BorderBottom(0.3f).BorderColor(GreyMedium).Padding(3);
        cell = align switch
        {
            TextAlign.Right => cell.AlignRight(),
            TextAlign.Center => cell.AlignCenter(),
            _ => cell.AlignLeft()
        };
        cell.Text(text).FontSize(7.5f);
    }

    private enum TextAlign
    {
        Left,
        Center,
        Right
    }
}
