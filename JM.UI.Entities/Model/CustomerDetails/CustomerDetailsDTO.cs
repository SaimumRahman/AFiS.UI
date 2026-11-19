using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.CustomerDetails
{
    public class CustomerDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }        // make nullable
        public string Phone { get; set; }
        public string? Address { get; set; }      // make nullable
        public int? MemberTypeId { get; set; }    // make nullable
        public DateTime LastModifiedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string MembershipType { get; set; }
        public int? DiscountRate { get; set; }
        public bool IsForceAdd { get; set; } = false;
        public string? WhatsApp { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? Anniversary { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? Designation { get; set; }
        public string? EmployeeId { get; set; }
        public string? AddressHome { get; set; }
        public string? AddressOffice { get; set; }
        public string? AddressOther { get; set; }
        public string? DefaultAddressType { get; set; }
        public string Status { get; set; } = "Active";
        public string? Remarks { get; set; }
        public int? UpdateBy { get; set; }
    }
}
