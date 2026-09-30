using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class CustomerWiseSalesDetailDTO
    {
        // ── Customer grouping ───────────────────────────────────────────
        public int CustomerId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerStatus { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        // ── Invoice grouping ────────────────────────────────────────────
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public bool IsBooking { get; set; }

        // ── Item line ───────────────────────────────────────────────────
        public int SalesDetailsId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal GrossAmount { get; set; }

        // ── Invoice level amounts (repeated once per invoice) ───────────
        // InvDisc and CampaignDisc are placeholders: SalesMaster stores only the
        // combined TotalDiscount, so these stay 0 until the discount split is
        // persisted separately. TotalDiscount is the authoritative figure.
        public decimal InvDisc { get; set; }
        public decimal CampaignDisc { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Due { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
