using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Routes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Routes
{
    public interface IRouteRepository
    {
        Task DeleteRoute(int id);
        Task<RouteModelDTO?> GetRouteById(int id);
        Task<IEnumerable<RouteModelDTO>> GetRoutes();
        Task<ResponseResult> SaveUpdateRoute(RouteModelDTO route);
        Task ToggleRouteStatus(int id);
    }
}