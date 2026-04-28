using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseInvoiceDTO
    {
        public int Id { get; set; }
        public string PurchaseNo { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public int ItemCount { get; set; }
    }
}
