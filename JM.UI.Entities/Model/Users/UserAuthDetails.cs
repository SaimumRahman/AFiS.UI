
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Model.Users
{
    public class User
    {
        public int UserId { get; set; }
       // public Microsoft.AspNet.Identity.EntityFramework.IdentityUser  UserName { get; set; }
       // public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
     //   public Microsoft.AspNet.Identity.EntityFramework.IdentityUser Email { get; set; }
        public int Role { get; set; }
        public int Designation { get; set; }
        public string LandingPage { get; set; }
        public int isActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public bool IsFirstLogin { get; set; }

       
    }
}
