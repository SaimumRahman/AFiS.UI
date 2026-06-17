using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodePrintConfigDTO
    {
        public int Id { get; set; }

        public decimal LabelWidthMm { get; set; }

        public decimal LabelHeightMm { get; set; }

        public int FabricRepeatCount { get; set; }
    }
}
