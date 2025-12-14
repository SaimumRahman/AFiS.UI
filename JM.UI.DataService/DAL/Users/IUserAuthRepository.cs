using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Users
{
    public interface IUserAuthRepository
    {
        public  Task<string> Register(UserAuthDetailsDAO detailsDAO);
        public Task<AuthenticatedUserResponse> Login(LoginRequest loginRequest);
        Task<LoggedInfo> GetCompanyIdByUserId(int Userid);
    }
}
