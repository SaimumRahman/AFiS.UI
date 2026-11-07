using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    /// <summary>
    /// Flat row set for the customer-wise sales report: one row per sold item line,
    /// with the invoice level amounts repeated on every line of that invoice.
    /// The UI groups these rows by customer (summary) and by invoice (drill-down).
    /// </summary>
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

        // Branch the invoice was billed from.
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        // Return / exchange markers taken from the invoice.
        public bool IsReturn { get; set; }
        public bool IsExchange { get; set; }
        public string ReturnInvoiceNo { get; set; } = string.Empty;

        // ── Item line ───────────────────────────────────────────────────
        public int SalesDetailsId { get; set; }
        public int ItemId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal GrossAmount { get; set; }

        // Item classification, used by the Group / Product / Sub Product filters.
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = string.Empty;
        public int DesignId { get; set; }
        public string DesignName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        // ── Line amounts ────────────────────────────────────────────────
        public decimal LineDiscount { get; set; }
        public decimal Vat { get; set; }

        /// <summary>Gross minus the line discount plus VAT — the line's net value.</summary>
        public decimal LineNet => GrossAmount - LineDiscount + Vat;

        // ── Invoice level amounts (repeated once per invoice) ───────────
        // InvDisc and CampaignDisc are placeholders: SalesMaster stores only the
        // combined TotalDiscount, so these stay 0 until the discount split is
        // persisted separately. TotalDiscount is the authoritative figure.
        public decimal InvDisc { get; set; }
        public decimal CampaignDisc { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Due { get; set; }

        /// <summary>Invoice bill after discount: what the customer owed for the invoice.</summary>
        public decimal InvoiceNet { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
