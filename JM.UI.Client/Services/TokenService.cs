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
    }

    public class TokenService : ITokenService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ITokenProvider _tokenProvider;

        public TokenService(
            ProtectedSessionStorage sessionStorage,
            ITokenProvider tokenProvider)
        {
            _sessionStorage = sessionStorage;
            _tokenProvider = tokenProvider;
        }

        public async Task InitializeTokenAsync()
        {
            try
            {
                var currentToken = _tokenProvider.GetToken();

                if (string.IsNullOrEmpty(currentToken))
                {
                    var tokenResult = await _sessionStorage.GetAsync<string>("Credentials");
                    if (tokenResult.Success && !string.IsNullOrEmpty(tokenResult.Value))
                    {
                        _tokenProvider.SetToken(tokenResult.Value);
                        Console.WriteLine($"✅ Token initialized from storage");
                    }
                    else
                    {
                        Console.WriteLine("❌ No token found in storage");
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
                var tokenResult = await _sessionStorage.GetAsync<string>("Credentials");
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
                Console.WriteLine($"💾 Saving token...");

                _tokenProvider.SetToken(token);
                await _sessionStorage.SetAsync("Credentials", token);

                Console.WriteLine($"✅ Token saved successfully");
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

        public async Task ClearTokenAsync()
        {
            try
            {
                _tokenProvider.ClearToken();
                await _sessionStorage.DeleteAsync("Credentials");
                await _sessionStorage.DeleteAsync("CompanyId");
                await _sessionStorage.DeleteAsync("UserInfo");

                Console.WriteLine("✅ Token cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token: {ex.Message}");
            }
        }
    }
}
