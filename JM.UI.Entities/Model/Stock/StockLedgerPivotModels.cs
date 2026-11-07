using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Entities.Model.Stock;

/// <summary>
/// One store block in the pivoted stock ledger: the store name plus the four
/// measures (Opening, Receive, Issue, Balance) shown under it.
/// </summary>
public class StockLedgerStoreGroup
{
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Display key used to look the group up in a row's cell map.</summary>
    public string Key => StoreName.Trim();

    /// <summary>Head Office is highlighted differently from the branches.</summary>
    public bool IsHeadOffice =>
        StoreName.Contains("HEAD OFFICE", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Opening / Receive / Issue / Balance for one store of one item.</summary>
public class StockLedgerStoreCell
{
    public decimal OpeningStock { get; set; }
    public decimal ReceiveQty { get; set; }
    public decimal IssueQty { get; set; }
    public decimal ClosingStock { get; set; }
}

/// <summary>
/// One item row of the pivoted ledger: barcode, item name and sale price, the
/// per-store measures, and the row's grand totals across every store.
/// </summary>
public class StockLedgerPivotRow
{
    /// <summary>Grouping key — the barcode when present, otherwise the item name.</summary>
    public string ItemKey { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }

    public string GroupName { get; set; } = string.Empty;
    public string SubGroupName { get; set; } = string.Empty;
    public string DesignName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;

    /// <summary>Store name → measures for that store.</summary>
    public Dictionary<string, StockLedgerStoreCell> Cells { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public decimal TotalOpening { get; set; }
    public decimal TotalReceive { get; set; }
    public decimal TotalIssue { get; set; }
    public decimal TotalClosing { get; set; }

    /// <summary>Group / Product / Sub Product / Brand, joined for the sub-caption.</summary>
    public string MetaText =>
        string.Join(" / ", new[] { GroupName, SubGroupName, DesignName, BrandName }
            .Where(v => !string.IsNullOrWhiteSpace(v)));

    public bool HasMeta => !string.IsNullOrWhiteSpace(MetaText);

    public StockLedgerStoreCell GetCell(string? storeName)
    {
        var key = (storeName ?? string.Empty).Trim();
        return Cells.TryGetValue(key, out var cell) ? cell : EmptyCell;

        // Shared zero cell, so a store that never stocked the item renders as 0.00.
    }

    private static readonly StockLedgerStoreCell EmptyCell = new();
}
