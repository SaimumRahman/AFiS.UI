using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace JM.UI.DataService.DAL.SubGroups
{
    public class SubGroupRepository : BaseRepository, ISubGroupRepository
    {
        public SubGroupRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<SubGroupRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<SubGroupModelDTO>> GetSubGroups()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("SubGroups/GetAllSubGroups");
                response.EnsureSuccessStatusCode();

                var subGroups = await response.Content.ReadFromJsonAsync<List<SubGroupModelDTO>>();
                return subGroups ?? new List<SubGroupModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all sub-groups");
                throw;
            }
        }

        public async Task<SubGroupModelDTO?> GetSubGroupById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SubGroups/GetSubGroupById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<SubGroupModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sub-group by ID: {Id}", id);
                throw;
            }
        }
        public async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SubGroups/GetSubGroupByGroupId/{groupId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var subGroups = await response.Content.ReadFromJsonAsync<List<SubGroupModelDTO>>();
                return subGroups ?? new List<SubGroupModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sub-group by ID: {Id}", groupId);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSubGroup(SubGroupModelDTO subGroup)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("SubGroups/SaveUpdateSubGroup", subGroup);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sub-group");
                throw;
            }
        }

        public async Task DeleteSubGroup(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"SubGroups/DeleteSubGroup/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sub-group: {Id}", id);
                throw;
            }
        }

        public async Task<bool> IsCodeExistsAsync(string code, int id = 0)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"SubGroups/IsCodeExists?code={Uri.EscapeDataString(code)}&id={id}");
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if sub-group code exists: {Code}", code);
                return false;
            }
        }
    }
}
