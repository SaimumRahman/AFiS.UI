using System;

namespace JM.UI.Entities.Model.Purchases;

/// <summary>
/// A single purchase line resolved for transfer entry: the item's master data plus
/// its stock and sale rate in the requested store.
/// </summary>
public class PurchaseStockItemDTO
{
    public int ItemId { get; set; }

    /// <summary>Purchase line barcode when set, otherwise the item's own barcode.</summary>
    public string? Barcode { get; set; }

    public string? ItemName { get; set; }

    public int? ColorId { get; set; }
    public string? ColorName { get; set; }

    public int? SizeId { get; set; }
    public string? SizeName { get; set; }

    public int? GroupId { get; set; }
    public int? SubGroupId { get; set; }
    public int? DesignId { get; set; }

    public int? UnitId { get; set; }
    public string? UnitName { get; set; }

    /// <summary>Quantity that was purchased on this line.</summary>
    public decimal PurchaseQuantity { get; set; }

    /// <summary>Available stock of the item in the requested store (0 when none).</summary>
    public decimal CurrentStock { get; set; }

    /// <summary>Store sale rate when set, otherwise the item's sale price.</summary>
    public decimal SalePrice { get; set; }

    public bool IsSaleable { get; set; }
    public bool IsNewItem { get; set; }
    public string? ProductType { get; set; }
}
