using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Routes
{
    public class RouteModelDTO
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string RoutePath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
