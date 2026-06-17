using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodePrintRequestDTO
    {
        public List<BarcodePrintItemDTO> Items { get; set; } = new();
        public int TemplateId { get; set; }
        public int? PrintedBy { get; set; }
        public DateTime PrintedAt { get; set; } = DateTime.Now;
    }
}
