using JM.UI.Entities.Model.CounterClosing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Service.Reports
{
    public class CounterClosingReportPdfService
    {
        public byte[] Generate(CounterClosingPreviewDTO preview, string printedBy = "-")
            => BuildDocument(preview, printedBy).GeneratePdf();

        public Document BuildDocument(CounterClosingPreviewDTO p, string printedBy = "-")
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var branch = p != null ? (p.StoreName ?? "-") : "-";
            var date = p != null ? p.BusinessDate : DateTime.Today;
            var printedAt = DateTime.Now;
            var opening = p != null ? p.OpeningCash : 0m;
            var cashSales = p != null ? p.CashIncome : 0m;
            var expenses = p != null ? p.CashOut : 0m;
            if (expenses == 0m && p != null && p.Expenses != null) expenses = p.Expenses.Sum(e => e.Amount);
            var closing = opening + cashSales - expenses;
            var counted = p != null ? (p.ActualCash ?? 0m) : 0m;
            var variance = counted - closing;
            var netSales = Sum(p?.SummaryRows, r => r.NetTotal);
            var grossTotal = Sum(p?.SummaryRows, r => r.PosSale + r.Booking);
            var totalDisc = Sum(p?.SummaryRows, r => r.Discount);
            var discPct = grossTotal > 0 ? (totalDisc / grossTotal * 100m) : 0m;
            var mfsTotal = Sum(p?.SummaryRows, r => r.Mfs);
            var bankTotal = Sum(p?.SummaryRows, r => r.BankPos);
            var cashTotal = Sum(p?.SummaryRows, r => r.Cash);
            var invCount = p != null && p.Transactions != null ? p.Transactions.Count(t => !t.IsBooking) : 0;
            var bkgCount = p != null && p.Transactions != null ? p.Transactions.Count(t => t.IsBooking) : 0;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(10, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(200).Text("DAY CLOSING REPORT").Bold().FontSize(16);
                            r.RelativeItem().AlignRight().Text("Revenue and expense ledgers posted today").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                        col.Item().PaddingTop(2).LineHorizontal(2).LineColor(QuestPDF.Helpers.Colors.Black);
                        col.Item().PaddingTop(4).Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Medium).Padding(4).Row(r =>
                        {
                            r.RelativeItem().Text(t => { t.Span("Branch: ").NormalWeight(); t.Span(branch).Bold(); });
                            r.RelativeItem().Text(t => { t.Span("Date: ").NormalWeight(); t.Span(date.ToString("dd-MMM-yyyy")).Bold(); });
                            r.RelativeItem().Text(t => { t.Span("Print: ").NormalWeight(); t.Span(printedAt.ToString("dd-MMM-yyyy, hh:mm tt")).Bold(); });
                            r.RelativeItem().AlignRight().Text(t => { t.Span("Page: ").NormalWeight(); t.Span("1 of 2").Bold(); });
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingTop(6).Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Medium).Row(r =>
                        {
                            KpiBox(r, "Net Sales", netSales.ToString("N0"), false);
                            KpiBox(r, "Discount", $"{discPct:0.0}% / {totalDisc:N0}", false);
                            r.RelativeItem(2).Padding(4).Column(c =>
                            {
                                c.Item().Text("Payment Mix").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                c.Item().Text($"Cash - {cashTotal:N0}").Bold().FontSize(9);
                                c.Item().Text($"MFS - {mfsTotal:N0}").Bold().FontSize(9);
                                c.Item().Text($"Bank POS - {bankTotal:N0}").Bold().FontSize(9);
                            });
                            KpiBox(r, "Invoices - Bookings", $"{invCount} - {bkgCount}", false);
                            KpiBox(r, "Total Expenses", expenses.ToString("N0"), false);
                            KpiBox(r, "Closing Balance", closing.ToString("N0"), true);
                        });

                        col.Item().PaddingTop(8).Text("REVENUE").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(70); cd.RelativeColumn(1.2f); cd.ConstantColumn(55); cd.ConstantColumn(50);
                                cd.ConstantColumn(55); cd.ConstantColumn(50); cd.ConstantColumn(55); cd.ConstantColumn(50);
                            });
                            t.Header(h =>
                            {
                                foreach (var txt in new[] { "Reference", "Served By", "Total Amt", "Disc Amt", "Net Amt", "MFS", "Bank POS", "Cash" })
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text(txt).Bold().FontSize(8);
                            });
                            foreach (var row in (p.Transactions ?? new List<CounterTransactionDTO>()))
                            {
                                t.Cell().Text(row.InvoiceNo).FontSize(8);
                                t.Cell().Text(row.CustomerName ?? "-").FontSize(8);
                                t.Cell().AlignRight().Text(row.TotalBill.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text("-").FontSize(8);
                                t.Cell().AlignRight().Text(row.TotalBill.ToString("N0")).Bold().FontSize(8);
                                t.Cell().AlignRight().Text("-").FontSize(8);
                                t.Cell().AlignRight().Text("-").FontSize(8);
                                t.Cell().AlignRight().Text(row.PaidAmount.ToString("N0")).FontSize(8);
                            }
                            t.Footer(f =>
                            {
                                f.Cell().Text("TOTAL").Bold(); f.Cell().Text("");
                                f.Cell().AlignRight().Text(grossTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(totalDisc.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(netSales.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(mfsTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(bankTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(cashTotal.ToString("N0")).Bold();
                            });
                        });

                        col.Item().PaddingTop(8).Text("EXPENSE / PAYMENT").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(60); cd.RelativeColumn(1.2f); cd.RelativeColumn(1.6f); cd.ConstantColumn(60);
                            });
                            t.Header(h =>
                            {
                                foreach (var txt in new[] { "Voucher No", "Ledger Name", "Description", "Paid Amt" })
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text(txt).Bold().FontSize(8);
                            });
                            foreach (var e in (p.Expenses ?? new List<CounterExpenseRowDTO>()))
                            {
                                t.Cell().Text(e.VoucherNo).FontSize(8);
                                t.Cell().Text(e.LedgerName).FontSize(8);
                                t.Cell().Text(e.Description).FontSize(8);
                                t.Cell().AlignRight().Text(e.Amount.ToString("N0")).FontSize(8);
                            }
                            t.Footer(f =>
                            {
                                f.Cell().Text("TOTAL PAID").Bold(); f.Cell().Text(""); f.Cell().Text("");
                                f.Cell().AlignRight().Text(expenses.ToString("N0")).Bold();
                            });
                        });
                    });

                    page.Footer().Row(r =>
                    {
                        foreach (var lbl in new[] { "Cashier", "Assistant Operation", "Operation", "Accounts", "MD's Approval" })
                        {
                            r.RelativeItem().Column(c2 =>
                            {
                                c2.Item().PaddingTop(18).LineHorizontal(1);
                                c2.Item().AlignCenter().Text(lbl).FontSize(8);
                            });
                        }
                    });
                });

                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(10, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(260).Text("DAY CLOSING REPORT - SUMMARY").Bold().FontSize(16);
                            r.RelativeItem().AlignRight().Text("Cashier, salesman and cash position for the day").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                        col.Item().PaddingTop(2).LineHorizontal(2).LineColor(QuestPDF.Helpers.Colors.Black);
                        col.Item().PaddingTop(4).Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Medium).Padding(4).Row(r =>
                        {
                            r.RelativeItem().Text(t => { t.Span("Branch: ").NormalWeight(); t.Span(branch).Bold(); });
                            r.RelativeItem().Text(t => { t.Span("Date: ").NormalWeight(); t.Span(date.ToString("dd-MMM-yyyy")).Bold(); });
                            r.RelativeItem().Text(t => { t.Span("Print: ").NormalWeight(); t.Span(printedAt.ToString("dd-MMM-yyyy, hh:mm tt")).Bold(); });
                            r.RelativeItem().AlignRight().Text(t => { t.Span("Page: ").NormalWeight(); t.Span("2 of 2").Bold(); });
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text("1 - CASHIER DETAILS").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(50); cd.RelativeColumn(1.2f); cd.ConstantColumn(55); cd.ConstantColumn(55);
                                cd.ConstantColumn(55); cd.ConstantColumn(50); cd.ConstantColumn(55); cd.ConstantColumn(50);
                                cd.ConstantColumn(55); cd.ConstantColumn(50);
                            });
                            t.Header(h =>
                            {
                                foreach (var txt in new[] { "Counter", "Cashier", "POS Sale", "Booking", "Gross", "Disc", "Net", "MFS", "Bank POS", "Cash" })
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text(txt).Bold().FontSize(7);
                            });
                            foreach (var row in (p.SummaryRows ?? new List<CounterClosingSummaryRowDTO>()))
                            {
                                t.Cell().Text(row.ServedBy).FontSize(8);
                                t.Cell().Text(row.CashierName).FontSize(8);
                                t.Cell().AlignRight().Text(row.PosSale.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text(row.Booking.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text((row.PosSale + row.Booking).ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text(row.Discount.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text(row.NetTotal.ToString("N0")).Bold().FontSize(8);
                                t.Cell().AlignRight().Text(row.Mfs.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text(row.BankPos.ToString("N0")).FontSize(8);
                                t.Cell().AlignRight().Text(row.Cash.ToString("N0")).FontSize(8);
                            }
                            t.Footer(f =>
                            {
                                var s = p.SummaryRows ?? new List<CounterClosingSummaryRowDTO>();
                                f.Cell().Text("TOTAL").Bold(); f.Cell().Text("");
                                f.Cell().AlignRight().Text(s.Sum(r => r.PosSale).ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(s.Sum(r => r.Booking).ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(grossTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(totalDisc.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(netSales.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(mfsTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(bankTotal.ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(cashTotal.ToString("N0")).Bold();
                            });
                        });

                        col.Item().PaddingTop(8).Text("3 - CASH RECONCILIATION").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.ConstantColumn(70); });
                            var rr = new Action<string, string, bool, bool>((label, value, bold, grey) =>
                            {
                                var cell = grey ? t.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3) : t.Cell();
                                cell.Text(label).FontSize(8);
                                var vcell = grey ? t.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3).AlignRight() : t.Cell().AlignRight();
                                vcell.Text(value).FontSize(8);
                            });
                            rr("Opening Balance", opening.ToString("N0"), false, false);
                            rr("Today's All Income (Net)", netSales.ToString("N0"), false, false);
                            rr("    MFS", mfsTotal.ToString("N0"), false, false);
                            rr("    Bank POS", bankTotal.ToString("N0"), false, false);
                            rr("    Cash", cashSales.ToString("N0"), false, false);
                            rr("Cash + Opening Balance", (opening + cashSales).ToString("N0"), true, true);
                            rr("Less: Expenses", $"({expenses:N0})", false, false);
                            rr("CLOSING BALANCE", closing.ToString("N0"), true, true);
                        });

                        col.Item().PaddingTop(8).Text("2 - SALESMAN DETAILS").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(30); cd.RelativeColumn(2); cd.ConstantColumn(60); cd.ConstantColumn(60); cd.ConstantColumn(60);
                            });
                            t.Header(h =>
                            {
                                foreach (var txt in new[] { "#", "Salesman", "Total", "Disc Amt", "Net Sale" })
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten3).Text(txt).Bold().FontSize(8);
                            });
                            int i = 1;
                            foreach (var s in (p.SalesmanSummary ?? new List<CounterSalesmanSummaryDTO>()))
                            {
                                t.Cell().Text(i.ToString()).FontSize(8);
                                t.Cell().Text(s.Salesman).FontSize(8);
                                if (s.NoSale)
                                {
                                    t.Cell().Text("-").FontSize(8); t.Cell().Text("-").FontSize(8);
                                    t.Cell().Text("No sale").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                }
                                else
                                {
                                    t.Cell().AlignRight().Text(s.Total.ToString("N0")).FontSize(8);
                                    t.Cell().AlignRight().Text(s.Discount.ToString("N0")).FontSize(8);
                                    t.Cell().AlignRight().Text(s.Net.ToString("N0")).Bold().FontSize(8);
                                }
                                i++;
                            }
                            t.Footer(f =>
                            {
                                var s = p.SalesmanSummary ?? new List<CounterSalesmanSummaryDTO>();
                                f.Cell().Text("").Bold(); f.Cell().Text("TOTAL").Bold();
                                f.Cell().AlignRight().Text(s.Sum(x => x.Total).ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(s.Sum(x => x.Discount).ToString("N0")).Bold();
                                f.Cell().AlignRight().Text(netSales.ToString("N0")).Bold();
                            });
                        });

                        col.Item().PaddingTop(8).Text("4 - CASH COUNT & VARIANCE").Bold().FontSize(10);
                        col.Item().PaddingTop(2).Text($"Counted cash: {counted:N0}").FontSize(9);
                        col.Item().Text($"Expected cash (Closing Balance): {closing:N0}").FontSize(9);
                        var varText = variance < 0 ? $"Short {Math.Abs(variance):N0}" : variance > 0 ? $"Excess {variance:N0}" : "Balanced";
                        col.Item().Text($"VARIANCE: {varText}").Bold().FontSize(9);
                    });

                    page.Footer().Row(r =>
                    {
                        foreach (var lbl in new[] { "Cashier", "Assistant Operation", "Operation", "Accounts", "MD's Approval" })
                        {
                            r.RelativeItem().Column(c2 =>
                            {
                                c2.Item().PaddingTop(18).LineHorizontal(1);
                                c2.Item().AlignCenter().Text(lbl).FontSize(8);
                            });
                        }
                    });
                });
            });
        }

        private static decimal Sum<T>(List<T> items, Func<T, decimal> selector)
            => items != null ? items.Sum(selector) : 0m;

        private static void KpiBox(RowDescriptor r, string label, string value, bool highlight)
        {
            var cell = highlight ? r.RelativeItem().Padding(4).Background(QuestPDF.Helpers.Colors.Grey.Lighten3) : r.RelativeItem().Padding(4);
            cell.Column(c =>
            {
                c.Item().Text(label).FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                c.Item().Text(value).Bold().FontSize(11);
            });
        }
    }
}