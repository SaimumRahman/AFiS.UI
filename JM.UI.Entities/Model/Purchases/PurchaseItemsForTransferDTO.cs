using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Purchases;

/// <summary>
/// A purchase found by its SystemInvoiceNo together with its lines, each resolved
/// against a specific store so Item Transfer can load them into the preview grid.
/// </summary>
public class PurchaseItemsForTransferDTO
{
    public int PurchaseId { get; set; }
    public string? SystemInvoiceNo { get; set; }

    /// <summary>Store the purchase was booked into.</summary>
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public List<PurchaseStockItemDTO> Items { get; set; } = new();
}
