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
        public string? ExistingBarcode { get; set; }
        public int? ItemId { get; set; }
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? DesignId { get; set; }
        public bool IsNewItemMode { get; set; } = false;
    }
}
