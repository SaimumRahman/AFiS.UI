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
    public DateTime LastTransactionDate { get; set; }
}