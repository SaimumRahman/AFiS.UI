using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases;

public class PurchaseSummaryDTO
{
    public int Id { get; set; }
    public string? BillInvoiceNumber { get; set; }
    public string? SystemInvoiceNo { get; set; }
    public string? BillInvoiceName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? SupplierName { get; set; }
    public string? StoreName { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? DueAmount { get; set; }
    public bool IsActive { get; set; }
}
