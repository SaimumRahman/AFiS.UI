using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Stock;

public class StockLedgerDTO
{
    public string StoreName { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public decimal OpeningStock { get; set; }
    public decimal ReceiveQty { get; set; }
    public decimal IssueQty { get; set; }
    public decimal ClosingStock { get; set; }
    public DateTime LastTransactionDate { get; set; }
}