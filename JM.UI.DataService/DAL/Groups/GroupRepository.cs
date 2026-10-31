using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Groups
{
    public class GroupRepository : BaseRepository, IGroupRepository
    {
        public GroupRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<GroupRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<GroupModelDTO>> GetGroups()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Groups/GetAllGroups");
                response.EnsureSuccessStatusCode();

                var groups = await response.Content.ReadFromJsonAsync<List<GroupModelDTO>>();
                return groups ?? new List<GroupModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all groups");
                throw;
            }
        }

        public async Task<GroupModelDTO?> GetGroupById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Groups/GetGroupById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<GroupModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching group by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateGroup(GroupModelDTO group)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("Groups/SaveUpdateGroup", group);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving group");
                throw;
            }
        }

        public async Task DeleteGroup(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Groups/DeleteGroup/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group: {Id}", id);
                throw;
            }
        }
    }
}
