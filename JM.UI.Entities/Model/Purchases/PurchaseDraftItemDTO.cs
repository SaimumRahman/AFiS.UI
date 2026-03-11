using System.Collections.Generic;

namespace JM.UI.Entities.Model.Purchases
{
    public class PurchaseDraftItemDTO
    {
        public int Id { get; set; }
        public int DraftId { get; set; }

        // ── Item identity ────────────────────────────────────────────
        public int? ItemId { get; set; }
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
        public string? Catalogue { get; set; }
        public string? MaterialType { get; set; }

        // ── Brand / Origin / Features ────────────────────────────────
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandColor { get; set; }
        public int? OriginId { get; set; }
        public string? OriginName { get; set; }
        public string? Origin { get; set; }
        public string? Features { get; set; }
        public List<int> FeatureIds { get; set; } = new();
        public string FeaturesDisplay { get; set; } = string.Empty;

        // ── Barcode / UoM ────────────────────────────────────────────
        public string? Barcode { get; set; }
        public int? MesurementUnitId { get; set; }
        public string MesurementUnitName { get; set; }

        // ── Pricing ──────────────────────────────────────────────────
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? ProductPricePercentage { get; set; }
        public decimal? OtherCost { get; set; }
        public decimal? CarryingCost { get; set; }
        // FIX: TransportCost was missing — required to fully round-trip draft save/load
        public decimal? TransportCost { get; set; }
        public decimal? OperationalCost { get; set; }
        public decimal? VatPercentage { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? SalePrice { get; set; }

        // ── Flags ────────────────────────────────────────────────────
        public bool IsSaleable { get; set; }
        // FIX: IsConsume was missing — required for Consume-type items
        public bool IsConsume { get; set; }
        public bool IsNewItem { get; set; }
        public bool IsActive { get; set; }
        public bool CountStockByColor { get; set; }
        public bool CountStockBySize { get; set; }

        // ── Type ─────────────────────────────────────────────────────
        public string? ProductType { get; set; }
        public int? CatalogueId { get; set; }
        public string? CatalogueName { get; set; }
    }
}