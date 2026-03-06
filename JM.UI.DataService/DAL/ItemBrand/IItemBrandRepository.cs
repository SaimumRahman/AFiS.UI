using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemBrand;

namespace JM.UI.DataService.DAL.ItemBrand
{
    public interface IItemBrandRepository
    {
        Task<IEnumerable<ItemBrandDTO>> GetItemBrands();
        Task<ResponseResult> SaveItemBrand(ItemBrandDTO brand);
    }
}