using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Suppliers
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        public SupplierRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<SupplierRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<SupplierModelDTO>> GetSuppliers()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Suppliers/GetAllSuppliers");
                response.EnsureSuccessStatusCode();

                var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierModelDTO>>();
                return suppliers ?? new List<SupplierModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all suppliers");
                throw;
            }
        }

        public async Task<SupplierModelDTO?> GetSupplierById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Suppliers/GetSupplierById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<SupplierModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSupplier(SupplierModelDTO supplier)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new { SupplierDTO = supplier };
                var response = await httpClient.PostAsJsonAsync("Suppliers/InsertUpdateSupplier", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving supplier");
                throw;
            }
        }

        public async Task DeleteSupplier(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Suppliers/DeleteSupplier/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier: {Id}", id);
                throw;
            }
        }
    }
}
