using System;

namespace JM.UI.Entities.Model.Barcodes
{
    public class BarcodeModelDTO
    {
        public int Id { get; set; }
        public int? LastBarcode { get; set; }
        public string? BarcodeNote { get; set; }
        public bool? ShowEncodedPurchasePrice { get; set; }
        public string? PurchasePriceEncoder { get; set; }
        public string? Prefix { get; set; }
        public bool? HideSalePrice { get; set; }
    }
}
