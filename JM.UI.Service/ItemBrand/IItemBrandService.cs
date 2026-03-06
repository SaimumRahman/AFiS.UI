using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemBrand;

namespace JM.UI.Service.ItemBrand
{
    public interface IItemBrandService
    {
        Task<IEnumerable<ItemBrandDTO>> GetItemBrands();
        Task<ResponseResult> SaveItemBrand(ItemBrandDTO brand);
    }
}