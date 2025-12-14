using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Model.Users
{
    [Table("AuthenticatedUserResponse")]
    public class AuthenticatedUserResponseDB
    {
        [Key]
        public int AURID { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public bool IsFirstLogin { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
