using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Routes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Routes
{
    public class RouteService : IRouteService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public RouteService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<RouteModelDTO>> GetRoutes()
        {
            return await _repositoryUnitOfWork.RouteRepository.GetRoutes();
        }

        public async Task<RouteModelDTO?> GetRouteById(int id)
        {
            return await _repositoryUnitOfWork.RouteRepository.GetRouteById(id);
        }

        public async Task<ResponseResult> SaveUpdateRoute(RouteModelDTO route)
        {
            var validation = await ValidateRoute(route);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            return await _repositoryUnitOfWork.RouteRepository.SaveUpdateRoute(route);
        }

        public async Task<ResponseResult> DeleteRoute(int id)
        {
            try
            {
                await _repositoryUnitOfWork.RouteRepository.DeleteRoute(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Route deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete route: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> ToggleRouteStatus(int id)
        {
            try
            {
                await _repositoryUnitOfWork.RouteRepository.ToggleRouteStatus(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Route status updated successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to update status: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateRoute(RouteModelDTO route)
        {
            if (string.IsNullOrWhiteSpace(route.RouteName))
                return Task.FromResult((false, "Route name is required."));

            if (route.RouteName.Length > 100)
                return Task.FromResult((false, "Route name cannot exceed 100 characters."));

            if (string.IsNullOrWhiteSpace(route.RoutePath))
                return Task.FromResult((false, "Route path is required."));

            if (route.RoutePath.Length > 200)
                return Task.FromResult((false, "Route path cannot exceed 200 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public RouteModelDTO CreateNewRoute()
        {
            return new RouteModelDTO
            {
                IsActive = true
            };
        }

        public string GetStatusBadgeStyle(bool isActive)
        {
            return isActive
                ? "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;"
                : "background-color: #f44336; color: white; padding: 4px 8px; border-radius: 4px;";
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
