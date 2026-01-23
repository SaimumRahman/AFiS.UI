using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.GroupActionPermission;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.GroupActionPermission;

public class GroupActionPermissionRepository : BaseRepository, IGroupActionPermissionRepository
{
    public GroupActionPermissionRepository(IHttpClientFactory factory, ITokenProvider token, ILogger<GroupActionPermissionRepository> logger)
        : base(factory, token, logger) { }

    public async Task<IEnumerable<GroupActionPermissionDTO>> GetGroupActionPermissions(int groupId)
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync($"GroupActionPermission/getall/{groupId}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<GroupActionPermissionDTO>>() ?? new();
    }
    public async Task<ResponseResult> InsertUpdateGroupActionPermissions(int groupId, List<GroupActionPermissionDTO> permissions)
    {
        try
        {
            _logger.LogInformation("Starting to save group action permissions for GroupId: {GroupId}", groupId);

            var httpClient = GetAuthenticatedClient("MainApi");
            var requestBody = new
            {
                GroupId = groupId,
                Permissions = permissions
            };
            var content = JsonContent.Create(requestBody);
            var response = await httpClient.PostAsync("GroupActionPermission/insert-update", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

            return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during save group action permissions for GroupId: {GroupId}", groupId);
            throw new Exception("Failed to save group action permissions: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during save group action permissions for GroupId: {GroupId}", groupId);
            throw new Exception("Unexpected error saving group action permissions: " + ex.Message, ex);
        }
    }
}
