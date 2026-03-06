using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemOrigin;

namespace JM.UI.DataService.DAL.ItemOrigin
{
    public interface IItemOriginRepository
    {
        Task<IEnumerable<ItemOriginDTO>> GetItemOrigins();
        Task<ResponseResult> SaveItemOrigin(ItemOriginDTO origin);
    }
}