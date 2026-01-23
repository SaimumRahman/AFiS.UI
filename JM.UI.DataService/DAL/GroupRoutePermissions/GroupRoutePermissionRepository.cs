using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRoutePermission;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.GroupRoutePermissions
{
    public class GroupRoutePermissionRepository : BaseRepository, IGroupRoutePermissionRepository
    {
        public GroupRoutePermissionRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<GroupRoutePermissionRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<GroupRoutePermissionModelDTO>> GetGroupRoutePermissions()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all group route permissions");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("GroupRoutePermissions/getall");
                response.EnsureSuccessStatusCode();

                var permissions = await response.Content.ReadFromJsonAsync<List<GroupRoutePermissionModelDTO>>();

                return permissions ?? new List<GroupRoutePermissionModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group route permissions");
                throw new Exception("Failed to fetch group route permissions: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group route permissions");
                throw new Exception("Unexpected error fetching group route permissions: " + ex.Message, ex);
            }
        }

        public async Task<GroupRoutePermissionModelDTO?> GetGroupRoutePermissionById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group route permission: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"GroupRoutePermissions/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Group route permission not found: {Id}", id);
                    return null;
                }

                var permission = await response.Content.ReadFromJsonAsync<GroupRoutePermissionModelDTO>();
                return permission;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group route permission by ID: {Id}", id);
                throw new Exception($"Failed to fetch group route permission: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group route permission by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching group route permission: {ex.Message}", ex);
            }
        }
        public async Task<GroupRoutePermissionModelDTO?> GetRoutePermittedForUser(int userId, string routePath)
        {
            // Optional: add basic input validation (helps debugging & prevents bad requests)
            if (userId <= 0)
            {
                _logger.LogWarning("Invalid userId provided: {UserId}", userId);
                return null;
            }

            if (string.IsNullOrWhiteSpace(routePath))
            {
                _logger.LogWarning("Route path cannot be empty for user {UserId}", userId);
                return null;
            }

            try
            {
                _logger.LogInformation(
                    "Fetching route permission for UserId = {UserId}, RoutePath = {RoutePath}",
                    userId, routePath);

                var httpClient = GetAuthenticatedClient("MainApi");

                // Consider using Uri.EscapeDataString for safety if routePath can contain special chars
                var encodedRoute = Uri.EscapeDataString(routePath);
                var url = $"GroupRoutePermissions/GetRoutePermittedForUser?userId={userId}&routePath={encodedRoute}";

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Route permission not found or request failed → UserId: {UserId}, Route: {RoutePath}, Status: {StatusCode}",
                        userId, routePath, response.StatusCode);

                    return null;
                }

                var permission = await response.Content.ReadFromJsonAsync<GroupRoutePermissionModelDTO>();

                _logger.LogInformation(
                    "Successfully retrieved route permission for UserId = {UserId}, RoutePath = {RoutePath}",
                    userId, routePath);

                return permission;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "HTTP request failed while fetching route permission → UserId: {UserId}, Route: {RoutePath}",
                    userId, routePath);

                throw new Exception($"Failed to fetch route permission (HTTP error): {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while fetching route permission → UserId: {UserId}, Route: {RoutePath}",
                    userId, routePath);

                throw new Exception($"Unexpected error fetching route permission: {ex.Message}", ex);
            }
        }
        public async Task<List<GroupRoutePermissionModelDTO?>> GetGroupRoutePermissionByGroupId(int groupId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group route permission: {groupId}", groupId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"GroupRoutePermissions/GetByGroupId/{groupId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Group route permission not found: {groupId}", groupId);
                    return null;
                }

                var permission = await response.Content.ReadFromJsonAsync<List<GroupRoutePermissionModelDTO>>();
                return permission;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group route permission by groupId: {groupId}", groupId);
                throw new Exception($"Failed to fetch group route permission: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group route permission by ID: {groupId}", groupId);
                throw new Exception($"Unexpected error fetching group route permission: {ex.Message}", ex);
            }
        }
        public async Task<List<GroupRoutePermissionModelDTO?>> GetRouteListByGroupId(int groupId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group route permission: {groupId}", groupId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"GroupRoutePermissions/GetRouteListByGroupId/{groupId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Group route permission not found: {groupId}", groupId);
                    return null;
                }

                var permission = await response.Content.ReadFromJsonAsync<List<GroupRoutePermissionModelDTO>>();
                return permission;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group route permission by groupId: {groupId}", groupId);
                throw new Exception($"Failed to fetch group route permission: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group route permission by ID: {groupId}", groupId);
                throw new Exception($"Unexpected error fetching group route permission: {ex.Message}", ex);
            }
        }
        public async Task<List<GroupRoutePermissionModelDTO?>> GetRouteListByUserId(int userId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group route permission: {userId}", userId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"GroupRoutePermissions/GetRouteListByUserId/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Group route permission not found: {userId}", userId);
                    return null;
                }

                var permission = await response.Content.ReadFromJsonAsync<List<GroupRoutePermissionModelDTO>>();
                return permission;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group route permission by userId: {userId}", userId);
                throw new Exception($"Failed to fetch group route permission: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group route permission by ID: {userId}", userId);
                throw new Exception($"Unexpected error fetching group route permission: {ex.Message}", ex);
            }
        }

        public async Task DeleteGroupRoutePermission(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete group route permission: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"GroupRoutePermissions/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Group route permission deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete group route permission: {Id}", id);
                throw new Exception($"Failed to delete group route permission: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete group route permission: {Id}", id);
                throw new Exception($"Unexpected error deleting group route permission: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateGroupRoutePermission(GroupRoutePermissionModelDTO permission)
        {
            try
            {
                _logger.LogInformation("Starting to save group route permission");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    GroupRoutePermissionDTO = permission
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("GroupRoutePermissions/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save group route permission");
                throw new Exception("Failed to save group route permission: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save group route permission");
                throw new Exception("Unexpected error saving group route permission: " + ex.Message, ex);
            }
        }
    }
}
