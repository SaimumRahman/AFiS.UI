using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemCatalogue;

namespace JM.UI.DataService.DAL.ItemCalalogue
{
    public interface IItemCatalogueRepository
    {
        Task DeleteItemCatalogue(int id);
        Task<ItemCatalogueDTO?> GetItemCatalogueById(int id);
        Task<IEnumerable<ItemCatalogueDTO>> GetItemCatalogues();
        Task<ResponseResult> SaveItemCatalogue(ItemCatalogueDTO dto);
    }
}