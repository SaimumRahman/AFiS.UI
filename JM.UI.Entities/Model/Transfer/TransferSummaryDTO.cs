using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Transfer
{
    public class TransferSummaryDTO
    {
        public string FromStoreCode { get; set; } = string.Empty;
        public string FromStoreName { get; set; } = string.Empty;
        public string? FromStoreAddress { get; set; }
        public string? FromStoreContact { get; set; }
        public string ToStoreCode { get; set; } = string.Empty;
        public string ToStoreName { get; set; } = string.Empty;
        public string? ToStoreAddress { get; set; }
        public string? ToStoreContact { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public string? Comments { get; set; }
        public string? Barcode { get; set; }
        public string? ItemName { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal IssueQty { get; set; }
        public decimal Amount { get; set; }
        public string? UserName { get; set; }
    }
}
