using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.CoreUsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.UserGroup
{
    public class GroupUsersDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CoreUserDTO> AssignedUsers { get; set; } = new();
        public List<CoreUserDTO> AvailableUsers { get; set; } = new();
    }
}
