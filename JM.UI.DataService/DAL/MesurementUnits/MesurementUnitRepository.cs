using JM.Infrastructure.Models;
using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.MesurementUnits
{
    public class MesurementUnitRepository : BaseRepository, IMesurementUnitRepository
    {
        public MesurementUnitRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<MesurementUnitRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<MesurementUnitModelDTO>> GetMesurementUnits()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("MesurementUnits/GetAllMesurementUnits");
                response.EnsureSuccessStatusCode();

                var units = await response.Content.ReadFromJsonAsync<List<MesurementUnitModelDTO>>();
                return units ?? new List<MesurementUnitModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all measurement units");
                throw;
            }
        }

        public async Task<MesurementUnitModelDTO?> GetMesurementUnitById(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"MesurementUnits/GetMesurementUnitById/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<MesurementUnitModelDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching measurement unit by ID: {Id}", id);
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateMesurementUnit(MesurementUnitModelDTO unit)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PostAsJsonAsync("MesurementUnits/SaveUpdateMesurementUnit", unit);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving measurement unit");
                throw;
            }
        }

        public async Task DeleteMesurementUnit(int id)
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"MesurementUnits/DeleteMesurementUnit/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting measurement unit: {Id}", id);
                throw;
            }
        }
    }
}
