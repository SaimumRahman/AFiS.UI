using JM.UI.Entities.Model.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using System.Security.Claims;

namespace JM.UI.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ITokenService _tokenService;

        public CustomAuthenticationStateProvider(
            ProtectedSessionStorage sessionStorage,
            ITokenService tokenService)
        {
            _sessionStorage = sessionStorage;
            _tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Check if token exists in session
                var tokenResult = await _sessionStorage.GetAsync<string>("Credentials");

                if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
                {
                    Console.WriteLine("❌ No token found - User not authenticated");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var token = tokenResult.Value;

                // Validate token is not expired
                if (!await _tokenService.ValidateTokenAsync(token))
                {
                    Console.WriteLine("❌ Token invalid or expired - Clearing session");
                    await ClearSessionAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Get user info from session
                var userInfoResult = await _sessionStorage.GetAsync<string>("UserInfo");
                var claims = new List<Claim>();

                if (userInfoResult.Success && !string.IsNullOrEmpty(userInfoResult.Value))
                {
                    var userInfo = JsonConvert.DeserializeObject<AuthenticatedUserResponse>(userInfoResult.Value);

                    claims.Add(new Claim(ClaimTypes.Name, userInfo.Username ?? ""));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()));
                    claims.Add(new Claim("UserId", userInfo.UserId.ToString()));

                    if (userInfo.CompanyId > 0)
                    {
                        claims.Add(new Claim("CompanyId", userInfo.CompanyId.ToString()));
                    }

                    Console.WriteLine($"✅ User authenticated: {userInfo.Username}");
                }

                var identity = new ClaimsIdentity(claims, "JwtAuth");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Auth state error: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserAuthentication()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            NotifyAuthenticationStateChanged(authState);
        }

        private async Task ClearSessionAsync()
        {
            await _sessionStorage.DeleteAsync("Credentials");
            await _sessionStorage.DeleteAsync("UserId");
            await _sessionStorage.DeleteAsync("UserInfo");
            await _sessionStorage.DeleteAsync("CompanyId");
        }
    }
}
