namespace JM.UI.Entities.Model.Reporting_D
{
    public class GroupWiseSalesDetailsDTO
    {
        public string GroupName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SubProductName { get; set; } = string.Empty;
        public decimal HlsAmount { get; set; }
        public decimal GecAmount { get; set; }
        public decimal BtbAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
