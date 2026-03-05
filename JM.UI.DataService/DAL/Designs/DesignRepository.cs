using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Designs
{
    public class DesignRepository : BaseRepository, IDesignRepository
    {
        public DesignRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<DesignRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<DesignModelDTO>> GetDesigns()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Designs/GetAllDesigns");
                response.EnsureSuccessStatusCode();

                var designs = await response.Content.ReadFromJsonAsync<List<DesignModelDTO>>();
                return designs ?? new List<DesignModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all designs");
                throw;
            }
        }

        public async Task<DesignModelDTO?> GetDesignById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Designs/GetDesignById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DesignModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching design by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Designs/GetDesignsBySubGroupId/{subGroupId}");

                if (!response.IsSuccessStatusCode)
                    return new List<DesignModelDTO>();

                var designs = await response.Content.ReadFromJsonAsync<List<DesignModelDTO>>();
                return designs ?? new List<DesignModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching designs by SubGroupId: {SubGroupId}", subGroupId);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateDesign(DesignModelDTO design)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("Designs/SaveUpdateDesign", design);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving design");
                throw;
            }
        }

        public async Task DeleteDesign(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Designs/DeleteDesign/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting design: {Id}", id);
                throw;
            }
        }
    }
}
