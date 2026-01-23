using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.CoreUsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.UserGroup
{
    public class GroupUsersDTO
    {
        public int UserGroupId { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }

        // Joined data from core_users
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }

        // Joined data from Groups table
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
