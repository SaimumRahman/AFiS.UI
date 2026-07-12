using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Barcodes;
using JM.UI.Entities.Model.Items;

namespace JM.UI.Service.Barcode
{
    public class BarcodePrintConfigService : IBarcodePrintConfigService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public BarcodePrintConfigService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<BarcodePrintConfigDTO>> GetAllBarcodePrintConfigs()
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetAllBarcodePrintConfigs();
        }

        public async Task<BarcodePrintConfigDTO?> GetTopBarcodePrintConfig()
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetTopBarcodePrintConfig();
        }

        public async Task<IEnumerable<BarcodeItemDTO>> GetBarcodeItemsByPurchaseId(int purchaseId)
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetBarcodeItemsByPurchaseId(purchaseId);
        }
        public async Task<IEnumerable<BarcodeTemplateDTO>> GetAllBarcodeTemplates()
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetAllBarcodeTemplates();
        }

        public async Task<BarcodeTemplateDTO?> GetBarcodeTemplateById(int id)
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetBarcodeTemplateById(id);
        }
        public async Task<IEnumerable<BarcodeItemDTO>> GetAllItemsForBarcodePrint()
        {
            return await _repositoryUnitOfWork.BarcodePrintConfigRepository.GetAllItemsForBarcodePrint();
        }
    }
}