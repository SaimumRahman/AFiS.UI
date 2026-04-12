using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Transfer;

public class TransferDetailDTO
{
    // ── Primary keys ──
    public int TransferDetailID { get; set; }
    public int TransferID { get; set; }
    public int ItemID { get; set; }

    // ── UI display helpers (not persisted, resolved in component) ──
    public string? Barcode { get; set; }
    public string? ItemName { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public int? SizeId { get; set; }
    public string? SizeName { get; set; }
    public int? GroupId { get; set; }
    public int? SubGroupId { get; set; }
    public int? DesignId { get; set; }
    public string? UnitName { get; set; }
    public bool IsNewItem { get; set; }

    // ── Core detail fields ──
    public decimal IssueQty { get; set; }
    public int UnitID { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? SerialNo { get; set; }

    // ── Audit ──
    public DateTime CreatedAt { get; set; }
    public string? CreatedRemarks { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdateRemark { get; set; }
    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeleteRemarks { get; set; }
}
public class LookupItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

