using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Routes;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace JM.UI.DataService.DAL.Routes
{
    public class RouteRepository : BaseRepository, IRouteRepository
    {
        public RouteRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<RouteRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<RouteModelDTO>> GetRoutes()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all routes");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Routes/getall");
                response.EnsureSuccessStatusCode();

                var routes = await response.Content.ReadFromJsonAsync<List<RouteModelDTO>>();

                return routes ?? new List<RouteModelDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get routes");
                throw new Exception("Failed to fetch routes: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get routes");
                throw new Exception("Unexpected error fetching routes: " + ex.Message, ex);
            }
        }

        public async Task<RouteModelDTO?> GetRouteById(int id)
        {
            try
            {
                _logger.LogInformation("Starting to fetch route: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync($"Routes/get/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Route not found: {Id}", id);
                    return null;
                }

                var route = await response.Content.ReadFromJsonAsync<RouteModelDTO>();
                return route;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get route by ID: {Id}", id);
                throw new Exception($"Failed to fetch route: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get route by ID: {Id}", id);
                throw new Exception($"Unexpected error fetching route: {ex.Message}", ex);
            }
        }

        public async Task DeleteRoute(int id)
        {
            try
            {
                _logger.LogInformation("Starting to delete route: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.DeleteAsync($"Routes/delete/{id}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Route deleted successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during delete route: {Id}", id);
                throw new Exception($"Failed to delete route: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during delete route: {Id}", id);
                throw new Exception($"Unexpected error deleting route: {ex.Message}", ex);
            }
        }

        public async Task ToggleRouteStatus(int id)
        {
            try
            {
                _logger.LogInformation("Starting to toggle route status: {Id}", id);

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.PutAsync($"Routes/toggle-status/{id}", null);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Route status toggled successfully: {Id}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request exception during toggle route status: {Id}", id);
                throw new Exception($"Failed to toggle route status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during toggle route status: {Id}", id);
                throw new Exception($"Unexpected error toggling route status: {ex.Message}", ex);
            }
        }

        public async Task<ResponseResult> SaveUpdateRoute(RouteModelDTO route)
        {
            try
            {
                _logger.LogInformation("Starting to save route");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    RouteDTO = route
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("Routes/insert-update", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();

                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during save route");
                throw new Exception("Failed to save route: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save route");
                throw new Exception("Unexpected error saving route: " + ex.Message, ex);
            }
        }
    }
}
