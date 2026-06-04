using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
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
        public async Task<IEnumerable<TransferMasterDTO>> GetUndispatchedTransfers(int storeId)
        {
            try
            {
                _logger.LogInformation("Fetching all undispatched transfers for store: {StoreId}", storeId);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/undispatched-transfers-by-store/{storeId}");
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
        public async Task<IEnumerable<TransferDetailDTO>> GetDetailsByTransferId(long transferId)
        {
            try
            {
                _logger.LogInformation("Fetching transfer details for transfer: {TransferId}", transferId);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/get-details-by-transfer-id/{transferId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Transfer not found: {TransferId}", transferId);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<IEnumerable<TransferDetailDTO>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get transfer details by ID: {TransferId}", transferId);
                throw new Exception($"Failed to fetch transfer details: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get transfer details by ID: {TransferId}", transferId);
                throw new Exception($"Unexpected error fetching transfer details: {ex.Message}", ex);
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

                // ── Read body regardless of status ──────────────────────────
                var rawBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Transfer save failed. Status: {Status} | Body: {Body}",
                        (int)response.StatusCode, rawBody);

                    return new ResponseResult
                    {
                        IsSuccessStatus = false,
                        StatusCode = (int)response.StatusCode,
                        Message = $"Server error ({(int)response.StatusCode}): {rawBody}"
                    };
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ResponseResult>(rawBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server." };
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

        public async Task<ItemDTO?> SearchByBarcodeExact(string barcode, int storeId)
        {
            try
            {
                _logger.LogInformation("Fetching item by exact barcode: {Barcode}", barcode);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/search-barcode-exact/{Uri.EscapeDataString(barcode)}/{storeId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ItemDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during barcode search");
                throw new Exception("Failed to fetch item by barcode: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during barcode search");
                throw new Exception("Unexpected error fetching item by barcode: " + ex.Message, ex);
            }
        }
        public async Task<IEnumerable<ItemDTO?>> SearchByBarcodeUptoColor(string barcode, int storeId)
        {
            try
            {
                _logger.LogInformation("Fetching item by barcode up to color: {Barcode}", barcode);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/search-barcode-upto-color/{Uri.EscapeDataString(barcode)}/{storeId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<ItemDTO?>>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during barcode search");
                throw new Exception("Failed to fetch item by barcode: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during barcode search");
                throw new Exception("Unexpected error fetching item by barcode: " + ex.Message, ex);
            }
        }

        public async Task<ResponseResult> UpdateDispatchStatus(List<int> transferIds, int updatedBy)
        {
            try
            {
                _logger.LogInformation("Updating dispatch status for transfers: {TransferIds}", string.Join(", ", transferIds));

                var httpClient = GetAuthenticatedClient("MainApi");

                var command = new { TransferIds = transferIds, UpdatedBy = updatedBy };
                var response = await httpClient.PatchAsJsonAsync("Transfer/update-dispatch-status", command);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = true, Message = "Dispatch status updated successfully." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during dispatch status update for transfers: {TransferIds}", string.Join(", ", transferIds));
                throw new Exception($"Failed to update dispatch status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during dispatch status update for transfers: {TransferIds}", string.Join(", ", transferIds));
                throw new Exception($"Unexpected error updating dispatch status: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<TransferMasterDTO>> GetDispatchedTransfers(int storeId)
        {
            try
            {
                _logger.LogInformation("Fetching all dispatched transfers for store: {StoreId}", storeId);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/dispatched-transfers-by-store/{storeId}");
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
        public async Task<IEnumerable<TransferMasterDTO>> GetAllByStoreIdAsync(int storeId)
        {
            try
            {
                _logger.LogInformation("Fetching all transfers for store: {StoreId}", storeId);
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Transfer/by-store/{storeId}");
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

        public async Task UpdateReceivedStatus(List<int> receivedDetailIds, List<int> fullyReceivedMasterIds, DateTime now, int userId)
        {
            try
            {
                _logger.LogInformation("Updating received status. Details: {DetailCount}, Masters: {MasterCount}",
                    receivedDetailIds.Count, fullyReceivedMasterIds.Count);

                var httpClient = GetAuthenticatedClient("MainApi");

                var requestBody = new
                {
                    DetailIds = receivedDetailIds,
                    MasterIds = fullyReceivedMasterIds,
                    ReceivedDate = now,
                    ReceivedBy = userId
                };

                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PatchAsync("Transfer/update-received-status", content);

                var rawBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Update received status failed. Status: {Status} | Body: {Body}",
                        (int)response.StatusCode, rawBody);

                    throw new Exception($"Server error ({(int)response.StatusCode}): {rawBody}");
                }

                _logger.LogInformation("Received status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during UpdateMasterReceivedStatus");
                throw new Exception("Unexpected error updating received status: " + ex.Message, ex);
            }
        }
    }
}
