using JM.UI.Entities.Model.Barcodes;

namespace JM.UI.Service.Barcode
{
    public interface IBarcodePrintConfigService
    {
        Task<IEnumerable<BarcodePrintConfigDTO>> GetAllBarcodePrintConfigs();
        Task<IEnumerable<BarcodeItemDTO>> GetBarcodeItemsByPurchaseId(int purchaseId);
        Task<BarcodePrintConfigDTO?> GetTopBarcodePrintConfig();
    }
}