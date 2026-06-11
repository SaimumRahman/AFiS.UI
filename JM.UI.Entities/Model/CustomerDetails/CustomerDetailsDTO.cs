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
    }
}
