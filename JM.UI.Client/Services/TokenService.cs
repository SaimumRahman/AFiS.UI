using JM.UI.Entities.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;

namespace JM.UI.Client.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
        Task SetTokenAsync(string token);
        Task<bool> IsTokenValidAsync();
        Task ClearTokenAsync();
        Task InitializeTokenAsync();
        Task<bool> ValidateTokenAsync(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly ProtectedLocalStorage _localStorage; // Changed to LocalStorage
        private readonly ITokenProvider _tokenProvider;

        public TokenService(
            ProtectedLocalStorage localStorage, // Changed to LocalStorage
            ITokenProvider tokenProvider)
        {
            _localStorage = localStorage;
            _tokenProvider = tokenProvider;
        }

        public async Task InitializeTokenAsync()
        {
            try
            {
                var currentToken = _tokenProvider.GetToken();

                if (string.IsNullOrEmpty(currentToken))
                {
                    var tokenResult = await _localStorage.GetAsync<string>("Credentials");
                    if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                    {
                        _tokenProvider.SetToken(tokenResult.Value);
                        Console.WriteLine($"✅ Token initialized from local storage");
                    }
                    else
                    {
                        Console.WriteLine("❌ No token found in local storage");
                    }
                }
                else
                {
                    Console.WriteLine("✅ Token already in memory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing token: {ex.Message}");
            }
        }

        public async Task<string> GetTokenAsync()
        {
            var token = _tokenProvider.GetToken();

            if (!string.IsNullOrEmpty(token))
                return token;

            try
            {
                var tokenResult = await _localStorage.GetAsync<string>("Credentials");
                if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                {
                    _tokenProvider.SetToken(tokenResult.Value);
                    return tokenResult.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token: {ex.Message}");
            }

            return string.Empty;
        }

        public async Task SetTokenAsync(string token)
        {
            try
            {
                Console.WriteLine($"💾 Saving token to local storage...");

                _tokenProvider.SetToken(token);
                await _localStorage.SetAsync("Credentials", token);

                Console.WriteLine($"✅ Token saved successfully to local storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving token: {ex.Message}");
            }
        }

        public async Task<bool> IsTokenValidAsync()
        {
            try
            {
                var token = await GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token is empty");
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var isValid = jwtToken.ValidTo > DateTime.UtcNow;
                Console.WriteLine($"Token valid: {isValid}, Expires: {jwtToken.ValidTo}");

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Check if token is expired
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    Console.WriteLine("❌ Token expired");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token validation failed: {ex.Message}");
                return false;
            }
        }

        public async Task ClearTokenAsync()
        {
            try
            {
                _tokenProvider.ClearToken();
                await _localStorage.DeleteAsync("Credentials");
                await _localStorage.DeleteAsync("CompanyId");
                await _localStorage.DeleteAsync("UserInfo");
                await _localStorage.DeleteAsync("UserId");

                Console.WriteLine("✅ Token cleared from local storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token: {ex.Message}");
            }
        }
    }
}