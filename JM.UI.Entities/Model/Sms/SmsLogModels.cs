using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Sms
{
    public class SmsLogDTO
    {
        public long Id { get; set; }
        public DateTime SentDate { get; set; }
        public string MobileNo { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public int? StoreId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string SmsType { get; set; } = string.Empty;
        public string MessageBody { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorDetails { get; set; } = string.Empty;
        public int? HttpStatusCode { get; set; }
        public int? SentBy { get; set; }
        public string SentByName { get; set; } = string.Empty;
    }

    public class SmsLogPageResult
    {
        public List<SmsLogDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
