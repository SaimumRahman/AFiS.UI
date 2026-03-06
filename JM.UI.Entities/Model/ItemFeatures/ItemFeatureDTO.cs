using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.ItemFeatures
{
    public class ItemFeatureDTO
    {
        public int FeatureId { get; set; }

        public string FeatureName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
