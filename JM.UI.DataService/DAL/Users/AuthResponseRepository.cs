using JM.Infrastructure.Base;
using JM.Infrastructure.Common;
using JM.UI.DataService.DAL.Users;
using JM.UI.Entities.Model.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Users
{

    public class AuthResponseRepository : IAuthResponseRepository
    {
        private readonly ILogger<AuthResponseRepository> _logger;
        private readonly HttpClient _httpClient;

        public AuthResponseRepository(
            ILogger<AuthResponseRepository> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("AuthApi");
        }
        //public async Task<int> DeleteExistingUserResponse(int userId)
        //{
        //    try
        //    {
        //        var response = await _httpClient.DeleteAsync($"/api/user-responses/{userId}");
        //        response.EnsureSuccessStatusCode();

        //        var result = await response.Content.ReadFromJsonAsync<int>();
        //        return result;
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, "Failed to delete user response for UserId: {UserId}", userId);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error occurred while deleting user response for UserId: {UserId}", userId);
        //        throw;
        //    }
        //}
        public async Task<int> DeleteExistingUserResponse(int UserId)
        {
            try
            {
                // return await base.ExecuteIdentityAsync($@"delete from AuthenticatedUserResponse where UserId=@UserId", new { UserId = UserId });
                return 1;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<int> SaveUserResponse(AuthenticatedUserResponse authenticatedUser)
        {
            try
            {
                //return await base.ExecuteIdentityAsync($@"insert AuthenticatedUserResponse (Token,UserId,Username,IsFirstLogin,IssueDate,ExpireDate) VALUES (@Token,@UserId,@Username,@IsFirstLogin,@IssueDate,@ExpireDate)", authenticatedUser);
                return 1;
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}