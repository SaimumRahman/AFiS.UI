using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.UserGroup
{
    public class UserGroupAssignmentDTO
    {
        public int GroupId { get; set; }
        public List<int> UserIds { get; set; } = new();
    }
}
