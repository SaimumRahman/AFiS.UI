using JM.UI.Entities.Model.Users;
using JM.UI.Entities.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;

namespace JM.UI.Client.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
        Task SetTokenAsync(string token);
        Task<string> GetRefreshTokenAsync();
        Task SetRefreshTokenAsync(string refreshToken);
        Task<bool> IsTokenValidAsync();
        Task ClearTokenAsync();
        Task InitializeTokenAsync();
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RefreshAccessTokenAsync();
    }

    public class TokenService : ITokenService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly ITokenProvider _tokenProvider;
        private readonly HttpClient _httpClient;

        public TokenService(
            ProtectedLocalStorage localStorage,
            ITokenProvider tokenProvider,
            HttpClient httpClient)
        {
            _localStorage = localStorage;
            _tokenProvider = tokenProvider;
            _httpClient = httpClient;
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

        public async Task<string> GetRefreshTokenAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<string>("RefreshToken");
                if (result.Success)
                    return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting refresh token: {ex.Message}");
            }
            return string.Empty;
        }

        public async Task SetRefreshTokenAsync(string refreshToken)
        {
            try
            {
                await _localStorage.SetAsync("RefreshToken", refreshToken);
                Console.WriteLine("✅ Refresh token saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving refresh token: {ex.Message}");
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

        public async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                var refreshToken = await GetRefreshTokenAsync();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    Console.WriteLine("❌ No refresh token available");
                    return false;
                }

                Console.WriteLine("🔄 Attempting to refresh access token...");

                var request = new { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Refresh failed: {response.StatusCode}");
                    return false;
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
                if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                {
                    Console.WriteLine("❌ Invalid refresh response");
                    return false;
                }

                await SetTokenAsync(authResponse.Token);
                if (!string.IsNullOrEmpty(authResponse.RefreshToken))
                    await SetRefreshTokenAsync(authResponse.RefreshToken);

                Console.WriteLine("✅ Access token refreshed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Refresh error: {ex.Message}");
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
                await _localStorage.DeleteAsync("RefreshToken");

                Console.WriteLine("✅ Token cleared from local storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token: {ex.Message}");
            }
        }
    }
}