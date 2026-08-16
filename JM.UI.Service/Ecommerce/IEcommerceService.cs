using JM.UI.Entities.Model.Ecommerce;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Ecommerce
{
    public interface IEcommerceService
    {
        Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId);
        Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter);
    }
}