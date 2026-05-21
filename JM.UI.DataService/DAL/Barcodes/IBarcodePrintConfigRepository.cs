using JM.UI.Entities.Model.Barcodes;

namespace JM.UI.DataService.DAL.Barcode
{
    public interface IBarcodePrintConfigRepository
    {
        Task<IEnumerable<BarcodePrintConfigDTO>> GetAllBarcodePrintConfigs();
        Task<IEnumerable<BarcodeItemDTO>> GetBarcodeItemsByPurchaseId(int purchaseId);
        Task<BarcodePrintConfigDTO?> GetTopBarcodePrintConfig();
    }
}