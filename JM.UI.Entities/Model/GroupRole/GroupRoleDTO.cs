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
    }
}
