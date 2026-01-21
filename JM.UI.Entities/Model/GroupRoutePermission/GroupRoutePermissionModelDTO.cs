using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.GroupRoutePermission
{
    public class GroupRoutePermissionModelDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int RouteId { get; set; }
        public int IsPermitted { get; set; }
        // Optional display properties (if you want to show group/route names in the UI)
        public string? GroupName { get; set; }
        public string? RouteName { get; set; }
        public string Route { get; set; }
        public string Action { get; set; }
    }
}
