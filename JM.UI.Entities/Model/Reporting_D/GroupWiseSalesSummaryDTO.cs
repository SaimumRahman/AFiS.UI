using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class GroupWiseSalesSummaryDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public decimal HlsAmount { get; set; }
        public decimal GecAmount { get; set; }
        public decimal BtbAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
