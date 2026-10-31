using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Model.Users
{
    public class AuthenticatedUserResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int UserId { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public bool IsFirstLogin { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public List<Role> Roles { get; set; }
        public int CompanyId { get; set; }
    }
}
