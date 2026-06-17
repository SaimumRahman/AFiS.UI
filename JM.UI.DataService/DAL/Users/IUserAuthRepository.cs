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
        public  Task<string> Register(RegisterRequest registerRequest);
        public Task<AuthenticatedUserResponse> Login(LoginRequest loginRequest);
        Task<LoggedInfo> GetCompanyIdByUserId(int Userid);
        Task<List<User>> GetAllUsers();
        Task<bool> UpdateActiveInactiveUser(string userName, bool isActive);
    }
}
