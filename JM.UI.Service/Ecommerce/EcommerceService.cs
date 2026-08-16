using JM.UI.DataService.DAL.Ecommerce;
using JM.UI.Entities.Model.Ecommerce;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Ecommerce
{
    public class EcommerceService : IEcommerceService
    {
        private readonly IEcommerceRepository _repository;

        public EcommerceService(IEcommerceRepository repository)
        {
            _repository = repository;
        }

        public async Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId)
        {
            return await _repository.GetEcommerceStore(storeId);
        }

        public async Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter)
        {
            return await _repository.GetEcommerceItems(filter);
        }
    }
}