using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Purchases
{
    public class PurchaseRepository : BaseRepository, IPurchaseRepository
    {
        public PurchaseRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        // =============================================
        // Get All Purchases
        // =============================================
        public async Task<IEnumerable<PurchaseSummaryDTO>> GetPurchases()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all purchases");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Purchase/getall");
                response.EnsureSuccessStatusCode();

                var purchases = await response.Content.ReadFromJsonAsync<List<PurchaseSummaryDTO>>();
                return purchases ?? new List<PurchaseSummaryDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchases");
                throw new Exception("Failed to fetch purchases: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchases");
                throw new Exception("Unexpected error fetching purchases: " + ex.Message, ex);
            }
        }

        // =============================================
        // Get Purchase By Id
        // =============================================
        public async Task<PurchaseDTO?> GetPurchaseById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch purchase: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Purchase not found: {Id}", id);
                    return null;
                }

                var purchase = await response.Content.ReadFromJsonAsync<PurchaseDTO>();
                return purchase;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchase by ID: {Id}", id);
                throw new Exception($"Failed to fetch purchase: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchase by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching purchase: {ex.Message}", ex);
            }
        }
        public async Task<SystemInVoiceDTO?> GetSystemInvoiceNew()
        {
            try
            {
                _logger.LogInformation("Requesting new System Invoice Number");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Purchase/system-invoice-new");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to retrieve new System Invoice Number");
                    return null;
                }

                var invoice = await response.Content.ReadFromJsonAsync<SystemInVoiceDTO>();
                return invoice;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while fetching new System Invoice Number");
                throw new Exception($"Failed to fetch System Invoice Number: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching new System Invoice Number");
                throw new Exception($"Unexpected error fetching System Invoice Number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItems(int purchaseId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/get-item-purchase/{purchaseId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch purchase items for PurchaseId: {PurchaseId}. Status: {StatusCode}",
                        purchaseId, response.StatusCode);
                    return new List<PurchaseItemDTO>();
                }

                var purchase = await response.Content.ReadFromJsonAsync<IEnumerable<PurchaseItemDTO>>();
                return purchase ?? new List<PurchaseItemDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPurchaseItems service for PurchaseId: {PurchaseId}", purchaseId);
                throw;
            }
        }
        // =============================================
        // Save/Update Purchase
        // =============================================
        public async Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items)
        {
            try
            {
                _logger.LogInformation("Starting to save purchase");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    Purchase = purchase,
                    Items = items
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("api/Purchase/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save purchase");
                throw new Exception("Failed to save purchase: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save purchase");
                throw new Exception("Unexpected error saving purchase: " + ex.Message, ex);
            }
        }

        // =============================================
        // Delete Purchase
        // =============================================
        public async Task DeletePurchase(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete purchase: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/Purchase/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Purchase deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete purchase: {Id}", id);
                throw new Exception($"Failed to delete purchase: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete purchase: {Id}", id);
                throw new Exception($"Unexpected error deleting purchase: {ex.Message}", ex);
            }
        }

        // =============================================
        // Generate Barcode
        // =============================================
        public async Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Generating barcode");

                var httpClient = GetAuthenticatedClient("MainApi");
                var content = JsonContent.Create(request);
                var response = await httpClient.PostAsync("api/Purchase/generate-barcode", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BarcodeResponse>();
                return result?.Barcode ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating barcode");
                throw new Exception("Failed to generate barcode: " + ex.Message, ex);
            }
        }

        // =============================================
        // Search By Barcode
        // =============================================
        public async Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode)
        {
            try
            {
                _logger.LogInformation("Searching by barcode: {Barcode}", barcode);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/search-barcode/{barcode}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BarcodeSearchResponseDTO>();
                return result ?? new BarcodeSearchResponseDTO { Found = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching by barcode: {Barcode}", barcode);
                throw new Exception("Failed to search by barcode: " + ex.Message, ex);
            }
        }
        public async Task<IEnumerable<PurchaseDraftDTO>> GetPurchaseDrafts()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all purchase drafts");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/Purchase/getall-draft");
                response.EnsureSuccessStatusCode();

                var drafts = await response.Content.ReadFromJsonAsync<List<PurchaseDraftDTO>>();

                return drafts ?? new List<PurchaseDraftDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchase drafts");
                throw new Exception("Failed to fetch purchase drafts: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchase drafts");
                throw new Exception("Unexpected error fetching purchase drafts: " + ex.Message, ex);
            }
        }

        public async Task<PurchaseDraftDTO?> GetPurchaseDraftById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch purchase draft: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"api/Purchase/get-draft/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Purchase draft not found: {Id}", id);
                    return null;
                }

                var draft = await response.Content.ReadFromJsonAsync<PurchaseDraftDTO>();
                return draft;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get purchase draft by ID: {Id}", id);
                throw new Exception($"Failed to fetch purchase draft: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get purchase draft by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching purchase draft: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SavePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items)
        {
            try
            {
                _logger.LogInformation("Starting to save purchase draft");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    Draft = draft,
                    Items = items
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("api/Purchase/save-draft", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save purchase draft");
                throw new Exception("Failed to save purchase draft: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save purchase draft");
                throw new Exception("Unexpected error saving purchase draft: " + ex.Message, ex);
            }
        }

        public async Task DeletePurchaseDraft(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete purchase draft: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"api/Purchase/delete-draft/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Purchase draft deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete purchase draft: {Id}", id);
                throw new Exception($"Failed to delete purchase draft: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete purchase draft: {Id}", id);
                throw new Exception($"Unexpected error deleting purchase draft: {ex.Message}", ex);
            }
        }
    }

   

    internal class BarcodeResponse
    {
        public string Barcode { get; set; } = string.Empty;
    }
}
