using JM.UI.Entities.Model.Ecommerce;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Ecommerce
{
    public interface IEcommerceRepository
    {
        Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId);
        Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter);
    }
}