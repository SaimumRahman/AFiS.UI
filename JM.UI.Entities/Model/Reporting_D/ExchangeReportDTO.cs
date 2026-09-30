using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class ExchangeReportDTO
    {
        // ── Exchange grouping ───────────────────────────────────────────
        public int SaleMasterId { get; set; }
        public string NewInvoiceNo { get; set; } = string.Empty;
        public DateTime ExchangeDate { get; set; }
        public string OldInvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // ── Stacking position inside the exchange ────────────────────────
        public int Position { get; set; }

        // ── Old (returned) item ──────────────────────────────────────────
        public string OldItemCode { get; set; } = string.Empty;
        public string OldItemName { get; set; } = string.Empty;
        public decimal OldQty { get; set; }
        public decimal OldRate { get; set; }
        public decimal OldValue { get; set; }
        public string OldUnit { get; set; } = string.Empty;

        // ── New (replacement) item ───────────────────────────────────────
        public string NewItemCode { get; set; } = string.Empty;
        public string NewItemName { get; set; } = string.Empty;
        public decimal NewQty { get; set; }
        public decimal NewRate { get; set; }
        public decimal NewValue { get; set; }
        public string NewUnit { get; set; } = string.Empty;

        // ── Exchange level amounts (repeated on every stacked row) ───────
        public decimal OldTotalValue { get; set; }
        public decimal NewTotalValue { get; set; }
        public decimal ExchangeAdjustment { get; set; }
        public decimal PaidAmount { get; set; }
        public string HandledBy { get; set; } = string.Empty;
    }
}
