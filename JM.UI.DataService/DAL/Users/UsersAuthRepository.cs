using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using JM.Infrastructure.Base;
using JM.Infrastructure.Common;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Users;
using JM.UI.Entities.Services;
using JM.UI.Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Users
{
    public class UsersAuthRepository : IUserAuthRepository
    {
        private readonly ILogger<UsersAuthRepository> _logger;
        private readonly HttpClient _httpClient;
        private readonly ITokenProvider _token;


        public UsersAuthRepository(
            ILogger<UsersAuthRepository> logger,
            IHttpClientFactory httpClientFactory,
            ITokenProvider token)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthApi");
            _token = token;
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

                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        throw new Exception("This email address is already registered.");
                    }

                    throw new Exception($"Registration failed. {responseContent}");
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

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Fetching all users");

                var token = _token.GetToken();

                _httpClient.DefaultRequestHeaders.Remove("Authorization");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/auth/User-GetAll");

                response.EnsureSuccessStatusCode();

                var users = await response.Content.ReadFromJsonAsync<List<User>>();

                return users ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching users");
                throw;
            }
        }
        public async Task<bool> UpdateActiveInactiveUser(string userName, bool isActive)
        {
            try
            {
                _logger.LogInformation(
                    "Updating active status for UserName: {UserName}, IsActive: {IsActive}",
                    userName, isActive);

                var token = _token.GetToken();

                _httpClient.DefaultRequestHeaders.Remove("Authorization");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(
                    $"api/auth/Active-Inactive-User?userName={Uri.EscapeDataString(userName)}&isActive={isActive}");

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to update status for UserName: {UserName}. Status: {StatusCode}, Response: {Response}",
                        userName,
                        response.StatusCode,
                        responseContent);

                    return false;
                }

                _logger.LogInformation(
                    "Successfully updated status for UserName: {UserName}",
                    userName);

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "HTTP request exception while updating status for UserName: {UserName}",
                    userName);

                throw new Exception($"Failed to update user status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while updating status for UserName: {UserName}",
                    userName);

                throw;
            }
        }
    }
}