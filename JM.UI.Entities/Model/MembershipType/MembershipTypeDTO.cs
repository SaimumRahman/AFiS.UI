using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.MembershipType
{
    public class MembershipTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? DurationInMonths { get; set; }
        public int? DiscountRate { get; set; }
    }
}
