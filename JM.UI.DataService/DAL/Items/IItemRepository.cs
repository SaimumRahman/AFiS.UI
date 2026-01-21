using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemModelDTO>> GetItems();
        Task<ItemModelDTO?> GetItemById(int id);
        Task<ResponseResult> SaveUpdateItem(ItemModelDTO item);
        Task DeleteItem(int id);
    }
}
