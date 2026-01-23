

using JM.UI.DataService.DAL.Users;
using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using JM.UI.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace JM.UI.Service.Users
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUserAuthRepository _UserauthRepo;

        public UserAuthService(IUserAuthRepository userAuthRepository)
        {
            _UserauthRepo = userAuthRepository;
        }

        public async Task<AuthenticatedUserResponse> Login(LoginRequest loginRequest)
        {
          return  await _UserauthRepo.Login(loginRequest);
        }

        public async Task<string> Register(RegisterRequest registerRequest)
        {
          return await _UserauthRepo.Register(registerRequest);    
        }
        public async Task<LoggedInfo> GetCompanyIdByUserId(int Userid)
        {
            return await _UserauthRepo.GetCompanyIdByUserId(Userid);
        }
        public async Task<List<User>> GetAllUsers()
        {
            return await _UserauthRepo.GetAllUsers();
        }
    }
}