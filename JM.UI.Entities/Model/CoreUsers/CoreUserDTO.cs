using JM;
using JM.UI;
using JM.UI.Entities;
using JM.UI.Entities.Model;
using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.CoreUsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.CoreUsers
{
    public class CoreUserDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public int? RoleID { get; set; }
    }
}
