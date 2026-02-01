using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items
{
    public interface IItemService
    {
        Task<IEnumerable<ItemDTO>> GetItems();
        Task<ItemDTO?> GetItemById(int id);
        Task<ResponseResult> SaveUpdateItem(ItemDTO item);
        Task<ResponseResult> DeleteItem(int id);
    }
}
