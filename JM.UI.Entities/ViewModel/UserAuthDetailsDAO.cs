
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.ViewModel
{

    public class UserAuthDetailsDAO
    {
        
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string LandingPage { get; set; }
        public int Role { get; set; }
        public int Designation { get; set; }
        public int isActive { get; set; }
        public bool IsFirstLogin { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }


    }
}
