using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Stock;

public class StockLedgerDTO
{
    public string StoreName { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }

    // Item attributes, shown as sortable grid columns.
    public string GroupName { get; set; }
    public string SubGroupName { get; set; }
    public string DesignName { get; set; }
    public string BrandName { get; set; }

    public decimal OpeningStock { get; set; }
    public decimal ReceiveQty { get; set; }
    public decimal IssueQty { get; set; }
    public decimal ClosingStock { get; set; }

    // Item sale price (shown in the pivoted branch table).
    public decimal SalePrice { get; set; }

    // Active campaign for this item/store at query time. All null when there is
    // no running campaign.
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public decimal? CampaignDiscountValue { get; set; }
    public int? CampaignDiscountTypeId { get; set; }
    public DateTime? CampaignStartDate { get; set; }
    public DateTime? CampaignEndDate { get; set; }
    public int? CampaignStoreId { get; set; }

    // Store identity of this row (branch names are not unique identifiers).
    public int StoreId { get; set; }

    public DateTime LastTransactionDate { get; set; }
}