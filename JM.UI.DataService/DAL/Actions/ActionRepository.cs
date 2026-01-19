using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Actions;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Actions
{
    public class ActionRepository : BaseRepository, IActionRepository
    {
        public ActionRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ActionRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ActionDTO>> GetAllActions()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all actions");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Actions/getall");
                response.EnsureSuccessStatusCode();

                var actions = await response.Content.ReadFromJsonAsync<List<ActionDTO>>();

                return actions ?? new List<ActionDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get actions");
                throw new Exception("Failed to fetch actions: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get actions");
                throw new Exception("Unexpected error fetching actions: " + ex.Message, ex);
            }
        }

        public async Task<ActionDTO?> GetActionById(int actionId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch action by ID: {ActionId}", actionId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Actions/{actionId}");
                response.EnsureSuccessStatusCode();

                var action = await response.Content.ReadFromJsonAsync<ActionDTO>();

                return action;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get action by ID");
                throw new Exception("Failed to fetch action: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get action by ID");
                throw new Exception("Unexpected error fetching action: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> CreateAction(ActionDTO action)
        {
            try
            {
                _logger.LogInformation("Starting to create action");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ActionKey = action.ActionKey,
                    Description = action.Description
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Actions/create", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during create action");
                throw new Exception("Failed to create action: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during create action");
                throw new Exception("Unexpected error creating action: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> UpdateAction(ActionDTO action)
        {
            try
            {
                _logger.LogInformation("Starting to update action");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ActionId = action.ActionId,
                    ActionKey = action.ActionKey,
                    Description = action.Description
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PutAsync("Actions/update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during update action");
                throw new Exception("Failed to update action: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during update action");
                throw new Exception("Unexpected error updating action: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> DeleteAction(int actionId)
        {
            try
            {
                _logger.LogInformation("Starting to delete action");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Actions/{actionId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during delete action");
                throw new Exception("Failed to delete action: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete action");
                throw new Exception("Unexpected error deleting action: " + ex.Message, ex);
            }
        }
    }
}
