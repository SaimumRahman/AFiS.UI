using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemOrigin;

namespace JM.UI.Service.ItemOrigin
{
    public interface IItemOriginService
    {
        Task<IEnumerable<ItemOriginDTO>> GetItemOrigins();
        Task<ResponseResult> SaveItemOrigin(ItemOriginDTO origin);
    }
}