using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.Reporting_D
{
    public class SalesBookingLineDTO
    {
        public int KeyId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SalesPersonName { get; set; } = string.Empty;
        public string UomName { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public bool IsReturn { get; set; }
        public string ReturnRefNo { get; set; } = string.Empty;
    }

    public class SalesBookingPaymentDTO
    {
        public int KeyId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "Cash";
    }

    public class SalesBookingInvoiceDTO
    {
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public int CreatedBy { get; set; }
        public string ServedByName { get; set; } = string.Empty;

        public decimal TotalBill { get; set; }
        public decimal CampaignDiscount { get; set; }
        public decimal InvoiceDiscount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ExchangeAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalDue { get; set; }

        public bool IsReturnExchange { get; set; }
        public string ReturnInvoiceNo { get; set; } = string.Empty;
        public string TransactionType { get; set; } = "Sale";

        public List<SalesBookingLineDTO> Lines { get; set; } = new();
        public List<SalesBookingPaymentDTO> Payments { get; set; } = new();
    }

    public class SalesBookingBookingDTO
    {
        public int BookingMasterId { get; set; }
        public string BookingNo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public int CreatedBy { get; set; }
        public string ServedByName { get; set; } = string.Empty;

        public decimal TotalBill { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal DueBalance { get; set; }

        public List<SalesBookingLineDTO> Lines { get; set; } = new();
        public List<SalesBookingPaymentDTO> Payments { get; set; } = new();
    }

    public class SalesBookingUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class SalesBookingReportDTO
    {
        public List<SalesBookingInvoiceDTO> Invoices { get; set; } = new();
        public List<SalesBookingBookingDTO> Bookings { get; set; } = new();
    }
}
