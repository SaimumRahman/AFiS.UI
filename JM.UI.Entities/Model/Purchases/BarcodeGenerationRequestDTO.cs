using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class BarcodeGenerationRequestDTO
    {
        public string? ShadeNo { get; set; }
        public string? ColorName { get; set; }
        public string? SizeName { get; set; }
        public string? BarcodePrefix { get; set; }
        public int? ItemId { get; set; }
        public int? GroupId { get; set; }
    }
}
