
using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace JM.UI.Service.Users
{
    public interface IUserAuthService
    {
        public Task<string> Register(RegisterRequest registerRequest);
        public Task<AuthenticatedUserResponse> Login(LoginRequest loginRequest);
        Task<LoggedInfo> GetCompanyIdByUserId(int Userid);
        Task<List<User>> GetAllUsers();
        Task<bool> UpdateActiveInactiveUser(string userName, bool isActive);
        Task<string> ResetUserPassword(string userName, string newPassword);
        Task<string> ChangePassword(string userName, string oldPassword, string newPassword);
    }
}