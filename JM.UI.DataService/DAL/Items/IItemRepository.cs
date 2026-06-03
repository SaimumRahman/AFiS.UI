using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemDTO>> GetItems();
        Task<ItemDTO?> GetItemById(int id);
        Task<IEnumerable<ItemDTO>> LoadItemsBySubGroup(int subGroupId);
        Task<ResponseResult> SaveUpdateItem(ItemDTO item);
        Task DeleteItem(int id);
        Task<IEnumerable<ItemDTO>> GetItemsByStoreId(int storeId);
    }
}
