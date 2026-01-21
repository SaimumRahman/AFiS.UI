using JM.UI.Entities.Model.GroupRoutePermission;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;

namespace JM.UI.Client.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ProtectedSessionStorage _session;

        public PermissionService(ProtectedSessionStorage session)
        {
            _session = session;
        }

        public async Task<bool> CanAccessRoute(string route)
        {
            var result = await _session.GetAsync<string>("Permissions");
            if (!result.Success) return false;

            var permissions = JsonConvert.DeserializeObject<List<GroupRoutePermissionModelDTO>>(result.Value);
            return permissions.Any(p => p.Route == route);
        }

        public async Task<bool> CanPerformAction(string route, string action)
        {
            var result = await _session.GetAsync<string>("Permissions");
            if (!result.Success) return false;

            var permissions = JsonConvert.DeserializeObject<List<GroupRoutePermissionModelDTO>>(result.Value);
            return permissions.Any(p =>
                p.Route == route &&
                p.Action.Equals(action, StringComparison.OrdinalIgnoreCase));
        }
    }

}
