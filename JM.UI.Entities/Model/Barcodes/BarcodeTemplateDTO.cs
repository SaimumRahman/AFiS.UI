using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodeTemplateDTO
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;  // e.g. "33*55"
        public string Descriptions { get; set; } = string.Empty;  // e.g. "ProductName, Brand, UOM, Price"
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Parsed helpers (no DB column)
        public decimal HeightMm => decimal.TryParse(TemplateName.Split('*').ElementAtOrDefault(0), out var h) ? h : 30;
        public decimal WidthMm => decimal.TryParse(TemplateName.Split('*').ElementAtOrDefault(1), out var w) ? w : 50;
        public List<string> Fields => Descriptions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

}
