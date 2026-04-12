using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Transfer;

public class TransferPreviewRow
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;

    public int? ColorId { get; set; }
    public string ColorName { get; set; } = string.Empty;

    public int? SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;

    public int? GroupId { get; set; }
    public int? SubGroupId { get; set; }
    public int? DesignId { get; set; }

    public int? UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;

    // ── Stock (read-only, loaded from API) ──
    public decimal StockQuantity { get; set; }

    // ── Editable fields ──
    public decimal IssueQty { get; set; }
    public decimal UnitPrice { get; set; }
    public string? SerialNo { get; set; }
    public string? CreatedRemarks { get; set; }

    // ── Calculated ──
    public decimal TotalAmount { get; set; }

    // ── Flags ──
    public bool IsNewItem { get; set; }
    public string? ProductType { get; set; }
    public bool CountStockByColor { get; set; }
    public bool CountStockBySize { get; set; }
}
