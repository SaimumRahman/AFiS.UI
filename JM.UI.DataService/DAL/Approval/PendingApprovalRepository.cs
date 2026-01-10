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
    public class PendingApprovalRepository : BaseRepository, IPendingApprovalRepository
    {
        public PendingApprovalRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PendingApprovalRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetAll(int currentUser)
        {
            try
            {
                _logger.LogInformation("Fetching all pending approvals");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($@"PendingApprovals/getall?currentUser={currentUser}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all pending approvals");
                throw new Exception("Failed to fetch pending approvals", ex);
            }
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByWorkflowId(int workflowId)
        {
            try
            {
                _logger.LogInformation("Fetching pending approvals by workflowId: {WorkflowId}", workflowId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PendingApprovals/getbyworkflow/{workflowId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending approvals by workflowId: {WorkflowId}", workflowId);
                throw new Exception("Failed to fetch pending approvals by workflow", ex);
            }
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByEntityId(int entityId)
        {
            try
            {
                _logger.LogInformation("Fetching pending approvals by entityId: {EntityId}", entityId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PendingApprovals/getbyentity/{entityId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending approvals by entityId: {EntityId}", entityId);
                throw new Exception("Failed to fetch pending approvals by entity", ex);
            }
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetByApproverUserId(int approverUserId)
        {
            try
            {
                _logger.LogInformation("Fetching pending approvals by approverUserId: {ApproverUserId}", approverUserId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PendingApprovals/getbyapprover/{approverUserId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending approvals by approverUserId: {ApproverUserId}", approverUserId);
                throw new Exception("Failed to fetch pending approvals by approver", ex);
            }
        }

        public async Task<PendingApprovalDTO?> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching pending approval by Id: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PendingApprovals/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Pending approval not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending approval by Id: {Id}", id);
                throw new Exception("Failed to fetch pending approval", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdate(PendingApprovalDTO command)
        {
            try
            {
                _logger.LogInformation("Saving pending approval");
                var PendingApprovalDTO = new
                {
                    PendingApprovalDTO = command
                };
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync(
                    "PendingApprovals/insert-update", PendingApprovalDTO);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pending approval");
                throw new Exception("Failed to save pending approval", ex);
            }
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting pending approval: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"PendingApprovals/delete/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pending approval: {Id}", id);
                throw new Exception("Failed to delete pending approval", ex);
            }
        }

        public async Task<ResponseResult> MarkReminderSent(int id)
        {
            try
            {
                _logger.LogInformation("Marking reminder sent for pending approval: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync(
                    $"PendingApprovals/markremindersent/{id}", null);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking reminder sent: {Id}", id);
                throw new Exception("Failed to mark reminder sent", ex);
            }
        }

        public async Task<ResponseResult> Deactivate(int id)
        {
            try
            {
                _logger.LogInformation("Deactivating pending approval: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync(
                    $"PendingApprovals/deactivate/{id}", null);

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ResponseResult>()
                       ?? new ResponseResult { IsSuccessStatus = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating pending approval: {Id}", id);
                throw new Exception("Failed to deactivate pending approval", ex);
            }
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetAllPendingPickup(int currentUser)
        {

            try
            {
                _logger.LogInformation("Fetching all pending approvals");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($@"PendingApprovals/getallPickup?currentUser={currentUser}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all pending approvals");
                throw new Exception("Failed to fetch pending approvals", ex);
            }
        }

        public async Task<IEnumerable<PendingApprovalDTO>> GetAllPendingDelivery(int currentUser)
        {

            try
            {
                _logger.LogInformation("Fetching all pending approvals");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PendingApprovals/getallDelivery?currentUser={currentUser}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PendingApprovalDTO>>()
                       ?? new List<PendingApprovalDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all pending approvals");
                throw new Exception("Failed to fetch pending approvals", ex);
            }
        }
    }
}
