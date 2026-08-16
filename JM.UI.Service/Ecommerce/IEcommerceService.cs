using JM.UI.Entities.Model.Ecommerce;
using System.Threading;
using System.Threading.Tasks;

namespace JM.UI.Service.Ecommerce
{
    public interface IEcommerceService
    {
        Task<EcommerceStoreDTO?> GetEcommerceStore(int? storeId);
        Task<IEnumerable<EcommerceItemDTO>> GetEcommerceItems(EcommerceFilterRequestDTO filter);
        Task<EcommercePostResponseDTO> PostItemToProductApi(EcommerceItemDTO item, string currentUser, string userRole, CancellationToken ct = default);
    }

    public class EcommercePostResponseDTO
    {
        public bool IsSuccess { get; set; }
        public int? InsertedId { get; set; }
        public string? Message { get; set; }
    }
}