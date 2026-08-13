using JM.Infrastructure.Models;
using JM.UI.Entities.Model.UserGroup;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.UserGroup
{

    public class UserGroupRepository : BaseRepository, IUserGroupRepository
    {
        public UserGroupRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<UserGroupRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<UserGroupDTO>> GetAllUserGroups()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all user groups");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("UserGroups/getall");
                response.EnsureSuccessStatusCode();

                var userGroups = await response.Content.ReadFromJsonAsync<List<UserGroupDTO>>();

                return userGroups ?? new List<UserGroupDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get user groups");
                throw new Exception("Failed to fetch user groups: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get user groups");
                throw new Exception("Unexpected error fetching user groups: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<UserGroupDTO>> GetUserGroupsByGroupId(int groupId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch user groups by group ID: {GroupId}", groupId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"UserGroups/bygroup/{groupId}");
                response.EnsureSuccessStatusCode();

                var userGroups = await response.Content.ReadFromJsonAsync<List<UserGroupDTO>>();

                return userGroups ?? new List<UserGroupDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get user groups by group ID");
                throw new Exception("Failed to fetch user groups: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get user groups by group ID");
                throw new Exception("Unexpected error fetching user groups: " + ex.Message, ex);
            }
        }

        public async Task<GroupUsersDTO> GetGroupUsersDetail(int groupId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group users detail: {GroupId}", groupId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"UserGroups/group-users/{groupId}");
                response.EnsureSuccessStatusCode();

                var groupUsers = await response.Content.ReadFromJsonAsync<GroupUsersDTO>();

                return groupUsers ?? new GroupUsersDTO();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group users detail");
                throw new Exception("Failed to fetch group users detail: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group users detail");
                throw new Exception("Unexpected error fetching group users detail: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> AssignUsersToGroup(int groupId, List<int> userIds)
        {
            try
            {
                _logger.LogInformation("Starting to assign users to group");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    GroupId = groupId,
                    UserIds = userIds
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("UserGroups/assign", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during assign users to group");
                throw new Exception("Failed to assign users to group: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during assign users to group");
                throw new Exception("Unexpected error assigning users to group: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> RemoveUserFromGroup(int userId, int groupId)
        {
            try
            {
                _logger.LogInformation("Starting to remove user from group");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    UserId = userId,
                    GroupId = groupId
                };
                var content = JsonContent.Create(requestBody);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(httpClient.BaseAddress + "UserGroups/remove"),
                    Content = content
                };

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during remove user from group");
                throw new Exception("Failed to remove user from group: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during remove user from group");
                throw new Exception("Unexpected error removing user from group: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> UpdateGroupUsers(int groupId, List<int> userIds)
        {
            try
            {
                _logger.LogInformation("Starting to update group users");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    GroupId = groupId,
                    UserIds = userIds
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PutAsync("UserGroups/update-group-users", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during update group users");
                throw new Exception("Failed to update group users: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during update group users");
                throw new Exception("Unexpected error updating group users: " + ex.Message, ex);
            }
        }

        public async Task<int> GetAdminGroupCountByUserId(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching admin group count for user: {UserId}", userId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"UserGroups/admin-count/{userId}");
                response.EnsureSuccessStatusCode();

                var count = await response.Content.ReadFromJsonAsync<int>();
                return count;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get admin group count");
                throw new Exception("Failed to fetch admin group count: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get admin group count");
                throw new Exception("Unexpected error fetching admin group count: " + ex.Message, ex);
            }
        }
    }
}
