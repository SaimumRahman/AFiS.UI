using JM.UI.Entities.Model.GroupRoutePermission;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;

namespace JM.UI.Client.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ProtectedSessionStorage _session;
        private readonly ProtectedLocalStorage _localStorage;

        public PermissionService(ProtectedSessionStorage session, ProtectedLocalStorage protectedLocalStorage)
        {
            _session = session;
            _localStorage = protectedLocalStorage;
        }

        public async Task<bool> CanAccessRoute(string route)
        {
            try
            {
                // Read from ProtectedLocalStorage
                var storageResult = await _localStorage.GetAsync<string>("Permissions");

                // Check if we got something back and it's not empty/null
                if (string.IsNullOrWhiteSpace(storageResult.Value))
                {
                    return false;
                }

                // Deserialize the JSON string back to the list
                var permissions = JsonConvert.DeserializeObject<List<GroupRoutePermissionModelDTO>>(storageResult.Value);

                // Safety check in case deserialization failed
                if (permissions == null)
                {
                    return false;
                }

                // Check if any permission allows this route
                return permissions.Any(p => p.Route == route);
            }
            catch (Exception ex)
            {
                // Optional: log the error in development
                // Console.WriteLine($"Error reading permissions: {ex.Message}");
                return false;
            }
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
