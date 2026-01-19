using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Actions
{
    public class ActionDTO
    {
        public int ActionId { get; set; }
        public string ActionKey { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
