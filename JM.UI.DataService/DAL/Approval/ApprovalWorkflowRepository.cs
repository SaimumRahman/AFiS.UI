using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Approval
{
    public class ApprovalWorkflowRepository : BaseRepository, IApprovalWorkflowRepository
    {
        public ApprovalWorkflowRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<ApprovalWorkflowRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<ApprovalWorkflowModelDTO>> GetApprovalWorkflows()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all approval workflows");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("ApprovalWorkflows/getall");
                response.EnsureSuccessStatusCode();

                var approvalWorkflows = await response.Content.ReadFromJsonAsync<List<ApprovalWorkflowModelDTO>>();

                return approvalWorkflows ?? new List<ApprovalWorkflowModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval workflows");
                throw new Exception("Failed to fetch approval workflows: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval workflows");
                throw new Exception("Unexpected error fetching approval workflows: " + ex.Message, ex);
            }
        }

        public async Task<ApprovalWorkflowModelDTO?> GetApprovalWorkflowById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch approval workflow: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"ApprovalWorkflows/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Approval workflow not found: {Id}", id);
                    return null;
                }

                var approvalWorkflow = await response.Content.ReadFromJsonAsync<ApprovalWorkflowModelDTO>();
                return approvalWorkflow;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get approval workflow by ID: {Id}", id);
                throw new Exception($"Failed to fetch approval workflow: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get approval workflow by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching approval workflow: {ex.Message}", ex);
            }
        }

        public async Task DeleteApprovalWorkflow(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete approval workflow: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"ApprovalWorkflows/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval workflow deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete approval workflow: {Id}", id);
                throw new Exception($"Failed to delete approval workflow: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete approval workflow: {Id}", id);
                throw new Exception($"Unexpected error deleting approval workflow: {ex.Message}", ex);
            }
        }

        public async Task ToggleApprovalWorkflowStatus(int id)
        {
            try
            {
                _logger.LogInformation("Starting to toggle approval workflow status: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync($"ApprovalWorkflows/toggle-status/{id}", null);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Approval workflow status toggled successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during toggle approval workflow status: {Id}", id);
                throw new Exception($"Failed to toggle approval workflow status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during toggle approval workflow status: {Id}", id);
                throw new Exception($"Unexpected error toggling approval workflow status: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateApprovalWorkflow(ApprovalWorkflowModelDTO approvalWorkflow)
        {
            try
            {
                _logger.LogInformation("Starting to save approval workflow");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    ApprovalWorkflowDTO = approvalWorkflow
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("ApprovalWorkflows/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save approval workflow");
                throw new Exception("Failed to save approval workflow: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save approval workflow");
                throw new Exception("Unexpected error saving approval workflow: " + ex.Message, ex);
            }
        }
    }
}
