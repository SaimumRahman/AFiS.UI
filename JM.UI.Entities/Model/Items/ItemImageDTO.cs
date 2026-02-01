using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.Items
{
    public class ItemImageDTO
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ImagePath { get; set; }
        public bool IsDefault { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
