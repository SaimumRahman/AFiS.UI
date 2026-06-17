using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodeItemDTO
    {
        public int Id { get; set; }
        public string BarcodeValue { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal? SalesPrice { get; set; } = 0;
        public string UnitName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string ReturnRefNo { get; set; } = string.Empty;
        public string DisplayLabel => $"{BarcodeValue}  —  {ProductName} ({GroupName})";
    }

}
