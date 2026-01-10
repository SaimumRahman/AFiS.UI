using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Services;
using JM.UI.Entities.ViewModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Approval
{
    public class ApprovalLevelRepository : BaseRepository, IApprovalLevelRepository
    {
        public ApprovalLevelRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ApprovalLevelRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ApprovalLevelModelDTO>> GetApprovalLevels()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all approval levels");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ApprovalLevels/getall");
                response.EnsureSuccessStatusCode();

                var approvalLevels = await response.Content.ReadFromJsonAsync<List<ApprovalLevelModelDTO>>();

                return approvalLevels ?? new List<ApprovalLevelModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval levels");
                throw new Exception("Failed to fetch approval levels: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval levels");
                throw new Exception("Unexpected error fetching approval levels: " + ex.Message, ex);
            }
        }

        public async Task<ApprovalLevelModelDTO?> GetApprovalLevelById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch approval level: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"ApprovalLevels/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Approval level not found: {Id}", id);
                    return null;
                }

                var approvalLevel = await response.Content.ReadFromJsonAsync<ApprovalLevelModelDTO>();
                return approvalLevel;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval level by ID: {Id}", id);
                throw new Exception($"Failed to fetch approval level: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval level by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching approval level: {ex.Message}", ex);
            }
        }

        public async Task DeleteApprovalLevel(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete approval level: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"ApprovalLevels/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval level deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete approval level: {Id}", id);
                throw new Exception($"Failed to delete approval level: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete approval level: {Id}", id);
                throw new Exception($"Unexpected error deleting approval level: {ex.Message}", ex);
            }
        }

        public async Task ToggleApprovalLevelStatus(int id)
        {
            try
            {
                _logger.LogInformation("Starting to toggle approval level status: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync($"ApprovalLevels/toggle-status/{id}", null);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval level status toggled successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during toggle approval level status: {Id}", id);
                throw new Exception($"Failed to toggle approval level status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during toggle approval level status: {Id}", id);
                throw new Exception($"Unexpected error toggling approval level status: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateApprovalLevel(ApprovalLevelModelDTO approvalLevel)
        {
            try
            {
                _logger.LogInformation("Starting to save approval level");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ApprovalLevelDTO = approvalLevel
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("ApprovalLevels/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save approval level");
                throw new Exception("Failed to save approval level: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save approval level");
                throw new Exception("Unexpected error saving approval level: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<UserAuthDetailsDAO>> GetUser()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all approval levels");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("User/getallUser");
                response.EnsureSuccessStatusCode();

                var approvalLevels = await response.Content.ReadFromJsonAsync<List<UserAuthDetailsDAO>>();

                return approvalLevels ?? new List<UserAuthDetailsDAO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval levels");
                throw new Exception("Failed to fetch approval levels: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval levels");
                throw new Exception("Unexpected error fetching approval levels: " + ex.Message, ex);
            }
        }
    }
}
