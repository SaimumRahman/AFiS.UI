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
        public async Task<string> Register(UserAuthDetailsDAO detailsDAO)
        {
            try
            {
                _logger.LogInformation("Starting user registration for Email: {Email}", detailsDAO.Email);
             
                var registerRequest = new
                {
                    Email = detailsDAO.Email,
                    Username = detailsDAO.UserName,
                    PhoneNumber = detailsDAO.Mobile,
                    Password = detailsDAO.Password,
                    ConfirmPassword = detailsDAO.ConfirmPassword,
                    EmpId = 1
                };
                // Make POST request with JSON serialization
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

                // Read response content
                var responseContent = await response.Content.ReadAsStringAsync();

                // Ensure success status code
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("User registration successful for Email: {Email}", detailsDAO.Email);

                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during user registration for Email: {Email}",
                    detailsDAO.Email);
                throw new Exception($"Failed to register user: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration for Email: {Email}",
                    detailsDAO.Email);
                throw new Exception($"Unexpected error during registration: {ex.Message}", ex);
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