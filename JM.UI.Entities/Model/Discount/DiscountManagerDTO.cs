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

        /// <summary>Primary store (first selected store) — kept for backward compatibility.</summary>
        public int? StoreId { get; set; }
        /// <summary>Comma-joined store names the campaign is valid for (one detail row per store).</summary>
        public string? StoreName { get; set; }
        /// <summary>Ids of every store the campaign is valid for (derived from its detail rows).</summary>
        public List<int> StoreIds { get; set; } = new();

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<DiscountManagerDetailsDTO> DiscountDetails { get; set; } = new();
    }
}
