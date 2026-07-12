using JM.UI.Entities.Model.Barcodes;
using JM.UI.Entities.Model.Items;

namespace JM.UI.Service.Barcode;

public interface IBarcodePrintConfigService
{
    Task<IEnumerable<BarcodePrintConfigDTO>> GetAllBarcodePrintConfigs();
    Task<IEnumerable<BarcodeItemDTO>> GetBarcodeItemsByPurchaseId(int purchaseId);
    Task<BarcodePrintConfigDTO?> GetTopBarcodePrintConfig();
    // ── Barcode Template ─────────────────────────────────────────
    Task<IEnumerable<BarcodeTemplateDTO>> GetAllBarcodeTemplates();
    Task<BarcodeTemplateDTO?> GetBarcodeTemplateById(int id);
    Task<IEnumerable<BarcodeItemDTO>> GetAllItemsForBarcodePrint();
}