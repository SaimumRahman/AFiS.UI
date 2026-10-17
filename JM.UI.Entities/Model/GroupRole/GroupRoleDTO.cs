using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.GroupRole
{
    public class GroupRoleDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Highest invoice discount (%) a user of this group may give in Sales POS.
        /// NULL / blank means no limit.
        /// </summary>
        public decimal? MaxDiscount { get; set; }
    }
}
