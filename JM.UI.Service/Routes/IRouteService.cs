using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Routes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Routes
{
    public interface IRouteService
    {
        RouteModelDTO CreateNewRoute();
        Task<ResponseResult> DeleteRoute(int id);
        Task<RouteModelDTO?> GetRouteById(int id);
        Task<IEnumerable<RouteModelDTO>> GetRoutes();
        string GetStatusBadgeStyle(bool isActive);
        Task<ResponseResult> SaveUpdateRoute(RouteModelDTO route);
        Task<ResponseResult> ToggleRouteStatus(int id);
        string Truncate(string? value, int maxChars);
        Task<(bool IsValid, string ErrorMessage)> ValidateRoute(RouteModelDTO route);
    }
}