using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.Entities.Model.Users
{
    public class LoginRequest
    {
            public string LoginId { get; set; }

            public string Password { get; set; }
        
    }
}
