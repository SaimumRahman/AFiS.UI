using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.GroupActionPermission
{
    public class GroupActionPermissionDTO
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int GroupId { get; set; }
        public int ActionId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string? RoutePath { get; set; }
        public bool IsActive { get; set; }
        public bool HasViewPermission { get; set; }
        public bool HasCreatePermission { get; set; }
        public bool HasEditPermission { get; set; }
        public bool HasDeletePermission { get; set; }
        public string? GroupName { get; set; } // For display purposes
        public string? ActionKey { get; set; } // For display purposes (CREATE, EDIT, DELETE, VIEW)
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
