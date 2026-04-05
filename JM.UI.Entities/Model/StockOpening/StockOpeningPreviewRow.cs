using JM;
using JM.UI;
using JM.UI.Entities;
using JM.UI.Entities.Model;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Entities.Model.StockOpening;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Entities.Model.StockOpening
{
    public class StockOpeningPreviewRow
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        // ── Attributes ───────────────────────────────────────────────
        public int? ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public int? SizeId { get; set; }
        public string SizeName { get; set; } = string.Empty;
        public string? ShadeNo { get; set; }

        // ── Hierarchy ────────────────────────────────────────────────
        public int? GroupId { get; set; }
        public int? SubGroupId { get; set; }
        public int? DesignId { get; set; }

        // ── Brand / Origin / Features ────────────────────────────────
        public int? BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int? OriginId { get; set; }
        public string OriginName { get; set; } = string.Empty;
        public List<int> FeatureIds { get; set; } = new();
        public string FeaturesDisplay { get; set; } = string.Empty;

        // ── UoM / Catalogue ──────────────────────────────────────────
        public int? MesurementUnitId { get; set; }
        public string MesurementUnitName { get; set; } = string.Empty;
        public int? CatalogueId { get; set; }
        public string CatalogueName { get; set; } = string.Empty;

        // ── Pricing: S.Rate only ─────────────────────────────────────
        public decimal SalePrice { get; set; }

        // ── Quantity ─────────────────────────────────────────────────
        public decimal Quantity { get; set; }
        public decimal StockQuantity { get; set; }

        // ── Calculated ───────────────────────────────────────────────
        public decimal TotalAmount { get; set; }

        // ── Flags ────────────────────────────────────────────────────
        public bool IsNewItem { get; set; }
        public bool IsSaleable { get; set; }
        public bool IsConsume { get; set; }
        public bool IsRawMaterial { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }

        // ── Image ────────────────────────────────────────────────────
        public string? ImageBase64 { get; set; }
    }
}
