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
