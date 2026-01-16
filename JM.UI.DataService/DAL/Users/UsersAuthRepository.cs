using JM.Infrastructure.Base;
using JM.Infrastructure.Common;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Users;
using JM.UI.Entities.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Users
{
    public class UsersAuthRepository: IUserAuthRepository
    {
        private readonly ILogger<UsersAuthRepository> _logger;
        private readonly HttpClient _httpClient;

        public UsersAuthRepository(
            ILogger<UsersAuthRepository> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthApi");
        }
        public async Task<string> Register(RegisterRequest registerRequests)
        {
            try
            {
                _logger.LogInformation("Starting user registration for Email: {Email}", registerRequests.Email);

                var registerRequest = new
                {
                    Email = registerRequests.Email,
                    Username = registerRequests.Username,
                    PhoneNumber = registerRequests.PhoneNumber,
                    Password = registerRequests.Password,
                    ConfirmPassword = registerRequests.ConfirmPassword
                    // Removed EmpId since it's commented out in the controller
                };

                // Make POST request with JSON serialization
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

                // Read response content first
                var responseContent = await response.Content.ReadAsStringAsync();

                // Check if request was successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Registration failed for Email: {Email}. Status: {StatusCode}, Response: {Response}",
                        registerRequests.Email,
                        response.StatusCode,
                        responseContent);

                    throw new Exception($"Registration failed with status {response.StatusCode}: {responseContent}");
                }

                _logger.LogInformation("User registration successful for Email: {Email}", registerRequests.Email);
                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during user registration for Email: {Email}",
                    registerRequests.Email);
                throw new Exception($"Failed to register user: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration for Email: {Email}",
                    registerRequests.Email);
                throw;
            }
        }


        public async Task<AuthenticatedUserResponse> Login(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("Logging In: {LoginId}", loginRequest.LoginId);

                // Make POST request to the correct login endpoint
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

                // Ensure success status code
                response.EnsureSuccessStatusCode();

                // Deserialize response to the correct type
                var authenticatedUser = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();

                if (authenticatedUser == null)
                {
                    _logger.LogError("Failed to deserialize login response for {LoginId}", loginRequest.LoginId);
                    throw new InvalidOperationException("Failed to process login response");
                }

                _logger.LogInformation("User Login successful for {LoginId}", loginRequest.LoginId);

                return authenticatedUser;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during login for {LoginId}", loginRequest.LoginId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {LoginId}", loginRequest.LoginId);
                throw;
            }
        }

        public async Task<LoggedInfo> GetCompanyIdByUserId(int Userid)
        {
          
                try
                {
                    string sql = "";
                    sql = $@"Select UserId,CompanyId from UserAuthDetails where UserId=@UserId";

                   // return await base.QueryFirstOrDefaultAsync<LoggedInfo>(sql, new { Userid = Userid });
                return null;
            }
                catch (Exception e)
                {

                    throw;
                }
          
        }
    }
}