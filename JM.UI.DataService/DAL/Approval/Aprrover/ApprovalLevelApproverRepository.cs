using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Approval.Aprrover
{
    public class ApprovalLevelApproverRepository : BaseRepository, IApprovalLevelApproverRepository
    {
        public ApprovalLevelApproverRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ApprovalLevelApproverRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApprovers()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all approval level approvers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ApprovalLevelApprovers/getall");
                response.EnsureSuccessStatusCode();

                var approvers = await response.Content.ReadFromJsonAsync<List<ApprovalLevelApproverModelDTO>>();

                return approvers ?? new List<ApprovalLevelApproverModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval level approvers");
                throw new Exception("Failed to fetch approval level approvers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval level approvers");
                throw new Exception("Unexpected error fetching approval level approvers: " + ex.Message, ex);
            }
        }
        public async Task<ApprovalLevelApproverModelDTO> IsExistApproval()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all approval level approvers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ApprovalLevelApprovers/IsExists");
                response.EnsureSuccessStatusCode();

                var approvers = await response.Content.ReadFromJsonAsync<ApprovalLevelApproverModelDTO>();

                return approvers ?? new ApprovalLevelApproverModelDTO();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval level approvers");
                throw new Exception("Failed to fetch approval level approvers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval level approvers");
                throw new Exception("Unexpected error fetching approval level approvers: " + ex.Message, ex);
            }
        }

        public async Task<IEnumerable<ApprovalLevelApproverModelDTO>> GetApprovalLevelApproversByLevelId(int levelId)
        {
            try
            {
                _logger.LogInformation("Starting to fetch approvers for level: {LevelId}", levelId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"ApprovalLevelApprovers/getbylevel/{levelId}");
                response.EnsureSuccessStatusCode();

                var approvers = await response.Content.ReadFromJsonAsync<List<ApprovalLevelApproverModelDTO>>();

                return approvers ?? new List<ApprovalLevelApproverModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approvers by level ID: {LevelId}", levelId);
                throw new Exception($"Failed to fetch approvers: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approvers by level ID: {LevelId}", levelId);
                throw new Exception($"Unexpected error fetching approvers: {ex.Message}", ex);
            }
        }

        public async Task<ApprovalLevelApproverModelDTO?> GetApprovalLevelApproverById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch approval level approver: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"ApprovalLevelApprovers/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Approval level approver not found: {Id}", id);
                    return null;
                }

                var approver = await response.Content.ReadFromJsonAsync<ApprovalLevelApproverModelDTO>();
                return approver;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval level approver by ID: {Id}", id);
                throw new Exception($"Failed to fetch approval level approver: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval level approver by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching approval level approver: {ex.Message}", ex);
            }
        }

        public async Task DeleteApprovalLevelApprover(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete approval level approver: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"ApprovalLevelApprovers/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval level approver deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete approval level approver: {Id}", id);
                throw new Exception($"Failed to delete approval level approver: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete approval level approver: {Id}", id);
                throw new Exception($"Unexpected error deleting approval level approver: {ex.Message}", ex);
            }
        }

        public async Task ToggleApprovalLevelApproverStatus(int id)
        {
            try
            {
                _logger.LogInformation("Starting to toggle approval level approver status: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync($"ApprovalLevelApprovers/toggle-status/{id}", null);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval level approver status toggled successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during toggle approval level approver status: {Id}", id);
                throw new Exception($"Failed to toggle approval level approver status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during toggle approval level approver status: {Id}", id);
                throw new Exception($"Unexpected error toggling approval level approver status: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateApprovalLevelApprover(ApprovalLevelApproverModelDTO approver)
        {
            try
            {
                _logger.LogInformation("Starting to save approval level approver");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ApprovalLevelApproverDTO = approver
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("ApprovalLevelApprovers/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save approval level approver");
                throw new Exception("Failed to save approval level approver: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save approval level approver");
                throw new Exception("Unexpected error saving approval level approver: " + ex.Message, ex);
            }
        }
    }
}
