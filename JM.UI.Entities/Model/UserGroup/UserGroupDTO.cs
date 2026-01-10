using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.UserGroup
{
    public class UserGroupDTO
    {
        public int UserGroupId { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? GroupName { get; set; }
    }
}
