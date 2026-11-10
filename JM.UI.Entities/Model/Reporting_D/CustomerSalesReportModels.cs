using System;
using System.Collections.Generic;
using System.Linq;

namespace JM.UI.Entities.Model.Reporting_D
{
    /// <summary>
    /// One row of the customer summary table. Built in the UI from the flat
    /// <see cref="CustomerWiseSalesDetailDTO"/> rows returned by the API.
    /// </summary>
    public class CustomerSalesSummaryRow
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string MembershipType { get; set; } = string.Empty;
        public int InvoiceCount { get; set; }

        /// <summary>Sum of invoice net amounts (TotalBill - TotalDiscount).</summary>
        public decimal TotalPurchase { get; set; }

        public DateTime? LastPurchaseDate { get; set; }
        public string PrimaryBranch { get; set; } = string.Empty;

        public bool IsWalkIn => CustomerId <= 0;

        public string DisplayName => IsWalkIn
            ? "Walk-in Customer"
            : (string.IsNullOrWhiteSpace(CustomerName) ? "(Unnamed customer)" : CustomerName);

        public string DisplayCode => IsWalkIn
            ? "-"
            : (string.IsNullOrWhiteSpace(CustomerCode) ? $"C{CustomerId}" : CustomerCode);

        public string DisplayPhone => string.IsNullOrWhiteSpace(CustomerPhone) ? "-" : CustomerPhone;

        public string DisplayTier => string.IsNullOrWhiteSpace(MembershipType) ? "GENERAL" : MembershipType;
    }

    /// <summary>
    /// A single invoice inside the customer drill-down. Each invoice becomes one
    /// receipt and one block of item lines.
    /// </summary>
    public class CustomerSalesInvoiceGroup
    {
        public int SaleMasterId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public bool IsExchange { get; set; }
        public bool IsReturn { get; set; }
        public string ReturnInvoiceNo { get; set; } = string.Empty;
        public decimal TotalDiscount { get; set; }
        public decimal InvoiceNet { get; set; }
        public decimal NetPaid { get; set; }
        public decimal Due { get; set; }
        public List<CustomerWiseSalesDetailDTO> Lines { get; set; } = new();

        public string StatusText
        {
            get
            {
                if (IsReturn && !string.IsNullOrWhiteSpace(ReturnInvoiceNo))
                    return $"Return ({ReturnInvoiceNo})";
                if (IsReturn)
                    return "Return";
                if (IsExchange)
                    return string.IsNullOrWhiteSpace(ReturnInvoiceNo)
                        ? "Exchanged"
                        : $"Exchanged ({ReturnInvoiceNo})";
                return "Regular Purchase";
            }
        }

        public bool IsSpecial => IsReturn || IsExchange;
    }
}
