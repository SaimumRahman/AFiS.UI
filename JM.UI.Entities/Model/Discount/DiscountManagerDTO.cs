using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Discount
{
    public class DiscountManagerDTO
    {
        public int Id { get; set; }
        public string DiscountName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<DiscountManagerDetailsDTO> DiscountDetails { get; set; } = new();
    }
}
