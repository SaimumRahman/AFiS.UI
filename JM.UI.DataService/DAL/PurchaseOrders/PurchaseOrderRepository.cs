using JM.Infrastructure.Models;
using JM.UI.Entities.Model.PurchaseOrders;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.PurchaseOrders
{
    public class PurchaseOrderRepository : BaseRepository, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<PurchaseOrderRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<PurchaseOrderModelDTO>> GetPurchaseOrders()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("PurchaseOrders/GetAllPurchaseOrders");
                response.EnsureSuccessStatusCode();

                var purchaseOrders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderModelDTO>>();
                return purchaseOrders ?? new List<PurchaseOrderModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all purchase orders");
                throw;
            }
        }

        public async Task<PurchaseOrderModelDTO?> GetPurchaseOrderById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"PurchaseOrders/GetPurchaseOrderById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<PurchaseOrderModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase order by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdatePurchaseOrder(PurchaseOrderModelDTO purchaseOrder)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { PurchaseOrderDTO = purchaseOrder };
                var response = await httpClient.PostAsJsonAsync("PurchaseOrders/InsertUpdatePurchaseOrder", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase order");
                throw;
            }
        }

        public async Task DeletePurchaseOrder(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"PurchaseOrders/DeletePurchaseOrder/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order: {Id}", id);
                throw;
            }
        }
    }
}
