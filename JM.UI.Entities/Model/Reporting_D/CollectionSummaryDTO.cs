using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class CollectionPaymentDTO
    {
        public int PayTranId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string ChannelGroup { get; set; } = "Cash";
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TrxId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class CollectionRowDTO
    {
        public int RefId { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public bool IsBooking { get; set; }
        public DateTime CreatedDate { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreCode { get; set; } = string.Empty;
        public string SourceType { get; set; } = "POS Sale";
        public string Cashier { get; set; } = string.Empty;
        public decimal NetPaid { get; set; }
        public List<CollectionPaymentDTO> Payments { get; set; } = new();
    }

    public class CollectionSummaryReportDTO
    {
        public List<CollectionRowDTO> Rows { get; set; } = new();
        public List<string> Channels { get; set; } = new();
    }
}