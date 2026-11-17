using System;

namespace JM.UI.Entities.Model.Discount
{
    /// <summary>
    /// A product that already sits in another active campaign (same store) whose date
    /// range shares at least one day with the campaign being edited.
    /// </summary>
    public class CampaignConflictDTO
    {
        public int DiscountManagerId { get; set; }
        public string DiscountName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }

        public decimal DiscountValue { get; set; }
        public int DiscountTypeId { get; set; }
        public string? DiscountTypeName { get; set; }

        /// <summary>e.g. "15%" or "৳200".</summary>
        public string DiscountText =>
            DiscountTypeId == 2 ? "৳" + DiscountValue.ToString("0.##") : DiscountValue.ToString("0.##") + "%";
    }
}
