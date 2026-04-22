using System;

namespace JM.UI.Entities.Model.StockOpening
{
    public class StockOpeningItemDTO
    {
        public int Id { get; set; }
        public int StockOpeningId { get; set; }

        // ── Item Identity ────────────────────────────────────────────
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int? SubGroupId { get; set; }
        public string? SubGroupName { get; set; }
        public int? DesignId { get; set; }
        public string? DesignName { get; set; }

        // ── Attributes ───────────────────────────────────────────────
        public string? ShadeNo { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public string? MaterialType { get; set; }

        // ── Brand / Origin / Features ────────────────────────────────
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? OriginId { get; set; }
        public string? OriginName { get; set; }
        public List<int> FeatureIds { get; set; } = new();
        public string FeaturesDisplay { get; set; } = string.Empty;

        // ── Barcode / UoM / Catalogue ────────────────────────────────
        public string? Barcode { get; set; }
        public int? MesurementUnitId { get; set; }
        public string? MesurementUnitName { get; set; }
        public int? CatalogueId { get; set; }
        public string? CatalogueName { get; set; }

        // ── Pricing: S.Rate only ─────────────────────────────────────
        public decimal? SalePrice { get; set; }

        // ── Quantity / Total ─────────────────────────────────────────
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PurchasePrice { get; set; }

        // ── Flags ────────────────────────────────────────────────────
        public bool IsSaleable { get; set; }
        public bool IsConsume { get; set; }
        public bool IsRawMaterial { get; set; }
        public bool IsNewItem { get; set; }
        public bool IsActive { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }

        // ── Type ─────────────────────────────────────────────────────
        public string? ProductType { get; set; }
        public int ProductTypeInt { get; set; } = 1;

        // ── Image ────────────────────────────────────────────────────
        public string? ImageBase64 { get; set; }
    }
}
