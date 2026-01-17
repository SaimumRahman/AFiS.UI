using JM.UI.Entities.Model.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using System.Security.Claims;

namespace JM.UI.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage _localStorage; // Changed to LocalStorage
        private readonly ITokenService _tokenService;
        private ClaimsPrincipal _cachedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(
            ProtectedLocalStorage localStorage, // Changed to LocalStorage
            ITokenService tokenService)
        {
            _localStorage = localStorage;
            _tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                Console.WriteLine("🔍 Checking authentication state...");

                // Check if token exists in local storage (shared across tabs)
                var tokenResult = await _localStorage.GetAsync<string>("Credentials");

                if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
                {
                    Console.WriteLine("❌ No token found - User not authenticated");
                    _cachedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                    return new AuthenticationState(_cachedPrincipal);
                }

                var token = tokenResult.Value;
                Console.WriteLine($"✅ Token found: {token.Substring(0, Math.Min(20, token.Length))}...");

                // Validate token is not expired
                if (!await _tokenService.ValidateTokenAsync(token))
                {
                    Console.WriteLine("❌ Token invalid or expired - Clearing session");
                    await ClearSessionAsync();
                    _cachedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                    return new AuthenticationState(_cachedPrincipal);
                }

                // Get user info from local storage
                var userInfoResult = await _localStorage.GetAsync<string>("UserInfo");
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

                    Console.WriteLine($"✅ User authenticated: {userInfo.Username} (UserId: {userInfo.UserId})");
                }
                else
                {
                    Console.WriteLine("⚠️ Token found but UserInfo missing - User still authenticated");
                }

                var identity = new ClaimsIdentity(claims, "JwtAuth");
                _cachedPrincipal = new ClaimsPrincipal(identity);

                return new AuthenticationState(_cachedPrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Auth state error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _cachedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(_cachedPrincipal);
            }
        }

        public void NotifyUserAuthentication()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            _cachedPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(_cachedPrincipal));
            NotifyAuthenticationStateChanged(authState);
        }

        private async Task ClearSessionAsync()
        {
            await _localStorage.DeleteAsync("Credentials");
            await _localStorage.DeleteAsync("UserId");
            await _localStorage.DeleteAsync("UserInfo");
            await _localStorage.DeleteAsync("CompanyId");
        }
    }
}