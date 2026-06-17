using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodePrintItemDTO
    {
        public int BarcodeId { get; set; }
        public string BarcodeValue { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal? Price { get; set; }
        public string? UoM { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int PrintQty { get; set; } = 1;
        public decimal LabelWidthMm { get; set; }
        public decimal LabelHeightMm { get; set; }
        public int TemplateId { get; set; }
        public int Id { get; set; }
        public string SalesPrice { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string ReturnRefNo { get; set; } = string.Empty;
        public string DisplayLabel => $"{BarcodeValue}  —  {ProductName} ({GroupName})";
    }
}
