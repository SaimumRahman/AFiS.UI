using JM.Infrastructure.Models;
using JM.UI.Entities.Model.GroupRole;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.GroupRole
{
    public class GroupRoleRepository : BaseRepository, IGroupRoleRepository
    {
        public GroupRoleRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<GroupRoleRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<GroupRoleDTO>> GetGroupRoles()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all group roles");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("GroupRoles/getall");
                response.EnsureSuccessStatusCode();

                var groupRoles = await response.Content.ReadFromJsonAsync<List<GroupRoleDTO>>();

                return groupRoles ?? new List<GroupRoleDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group roles");
                throw new Exception("Failed to fetch group roles: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group roles");
                throw new Exception("Unexpected error fetching group roles: " + ex.Message, ex);
            }
        }

        public async Task<GroupRoleDTO?> GetGroupRoleById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch group role: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"GroupRoles/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Group role not found: {Id}", id);
                    return null;
                }

                var groupRole = await response.Content.ReadFromJsonAsync<GroupRoleDTO>();
                return groupRole;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get group role by ID: {Id}", id);
                throw new Exception($"Failed to fetch group role: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get group role by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching group role: {ex.Message}", ex);
            }
        }

        public async Task DeleteGroupRole(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete group role: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"GroupRoles/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Group role deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete group role: {Id}", id);
                throw new Exception($"Failed to delete group role: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete group role: {Id}", id);
                throw new Exception($"Unexpected error deleting group role: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateGroupRole(GroupRoleDTO groupRole)
        {
            try
            {
                _logger.LogInformation("Starting to save group role");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    GroupRoleDTO = groupRole
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("GroupRoles/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save group role");
                throw new Exception("Failed to save group role: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save group role");
                throw new Exception("Unexpected error saving group role: " + ex.Message, ex);
            }
        }
    }
}
