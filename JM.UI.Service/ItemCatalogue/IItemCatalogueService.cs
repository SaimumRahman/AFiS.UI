using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemCatalogue;

namespace JM.UI.Service.ItemCatalogue
{
    public interface IItemCatalogueService
    {
        Task<ResponseResult> DeleteItemCatalogue(int id);
        Task<ItemCatalogueDTO?> GetItemCatalogueById(int id);
        Task<IEnumerable<ItemCatalogueDTO>> GetItemCatalogues();
        Task<ResponseResult> SaveItemCatalogue(ItemCatalogueDTO dto);
    }
}