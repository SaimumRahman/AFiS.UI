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

    /// <summary>Store id, used to label branch-specific campaigns.</summary>
    public int StoreId { get; set; }

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
/// An active discount campaign attached to an item, resolved from
/// DiscountManager / DiscountManagerDetails. DiscountTypeId 1 = percentage,
/// 2 = flat amount (৳) — the same convention the discount screens use.
/// </summary>
public class StockLedgerCampaign
{
    public int CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public int DiscountTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>NULL or 0 means the campaign runs in every branch.</summary>
    public int? StoreId { get; set; }

    public bool IsPercentage => DiscountTypeId != 2;

    /// <summary>"15%" or "৳ 500.00".</summary>
    public string DiscountLabel => IsPercentage
        ? $"{DiscountValue:0.##}%"
        : $"৳ {DiscountValue:N2}";

    /// <summary>Whole days until the campaign ends (0 = ends today).</summary>
    public int DaysLeft
    {
        get
        {
            var days = (EndDate.Date - DateTime.Now.Date).Days;
            return days < 0 ? 0 : days;
        }
    }

    public string DaysLeftText => DaysLeft == 0
        ? "Ends today"
        : DaysLeft == 1 ? "1 Day Left" : $"{DaysLeft} Days Left";

    /// <summary>Single-line caption shown under the item name.</summary>
    public string DisplayText => $"{Name}: {DiscountLabel} Off ({DaysLeftText})";

    public string ValidityText =>
        $"{StartDate:dd-MMM-yyyy} to {EndDate:dd-MMM-yyyy}";

    /// <summary>True when the campaign is limited to one branch.</summary>
    public bool IsBranchSpecific => StoreId.HasValue && StoreId.Value > 0;
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

    /// <summary>Every active campaign found for this item (deduplicated).</summary>
    public List<StockLedgerCampaign> Campaigns { get; set; } = new();

    /// <summary>Campaign used for the grid caption: global first, else soonest end.</summary>
    public StockLedgerCampaign? PrimaryCampaign =>
        Campaigns.FirstOrDefault(c => !c.IsBranchSpecific)
        ?? Campaigns.OrderBy(c => c.EndDate).FirstOrDefault();

    public bool HasCampaign => Campaigns.Count > 0;

    /// <summary>Caption under the item name, empty when there is no campaign.</summary>
    public string CampaignText => PrimaryCampaign?.DisplayText ?? string.Empty;

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
