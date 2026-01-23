using System.Threading.Tasks;

namespace JM.UI.Client.Services
{
    public interface IPermissionService
    {
        Task<bool> CanAccessRoute(string route);
        Task<bool> CanPerformAction(string route, string action);
    }
}