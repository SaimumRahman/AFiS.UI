using System;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class PayTypeWiseSalesDetailsDTO
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public decimal ExchangeAmount { get; set; }
        public decimal CashPaid { get; set; }
        public decimal CityPos { get; set; }
        public decimal DbbLPos { get; set; }
        public decimal Bkash { get; set; }
        public decimal Nagad { get; set; }
        public decimal Dues { get; set; }
        public string RcvdBy { get; set; } = string.Empty;
    }
}
