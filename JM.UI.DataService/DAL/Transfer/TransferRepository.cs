using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Transfer;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Transfer
{
    public class TransferRepository : BaseRepository, ITransferRepository
    {
        public TransferRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<TransferRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<TransferMasterDTO>> GetTransfers()
        {
            try
            {
                _logger.LogInformation("Fetching all transfers");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Transfer/getall");
                response.EnsureSuccessStatusCode();

                var transfers = await response.Content.ReadFromJsonAsync<List<TransferMasterDTO>>();
                return transfers ?? new List<TransferMasterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get transfers");
                throw new Exception("Failed to fetch transfers: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get transfers");
                throw new Exception("Unexpected error fetching transfers: " + ex.Message, ex);
            }
        }

        public async Task<TransferMasterDTO?> GetTransferById(long id)
        {
            try
            {
                _logger.LogInformation("Fetching transfer: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Transfer not found: {Id}", id);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<TransferMasterDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get transfer by ID: {Id}", id);
                throw new Exception($"Failed to fetch transfer: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get transfer by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching transfer: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateTransfer(TransferMasterDTO transfer)
        {
            try
            {
                _logger.LogInformation("Saving transfer: {TransferId}", transfer.TransferId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { Transfer = transfer };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Transfer/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save transfer");
                throw new Exception("Failed to save transfer: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save transfer");
                throw new Exception("Unexpected error saving transfer: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> DeleteTransfer(long id, int deletedBy)
        {
            try
            {
                _logger.LogInformation("Deleting transfer: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Transfer/delete/{id}/{deletedBy}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Transfer deleted successfully." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during delete transfer: {Id}", id);
                throw new Exception($"Failed to delete transfer: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete transfer: {Id}", id);
                throw new Exception($"Unexpected error deleting transfer: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> DeleteTransferDetail(long detailId, int deletedBy)
        {
            try
            {
                _logger.LogInformation("Deleting transfer detail: {DetailId}", detailId);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Transfer/delete-detail/{detailId}/{deletedBy}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Detail removed successfully." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during delete transfer detail: {DetailId}", detailId);
                throw new Exception($"Failed to delete transfer detail: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete transfer detail: {DetailId}", detailId);
                throw new Exception($"Unexpected error deleting transfer detail: {ex.Message}", ex);
            }
        }
    }
}
