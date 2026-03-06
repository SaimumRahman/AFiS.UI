using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemFeatures;

namespace JM.UI.DataService.DAL.ItemFeatures
{
    public interface IItemFeatureRepository
    {
        Task<IEnumerable<ItemFeatureDTO>> GetItemFeatures();
        Task<ResponseResult> SaveItemFeature(ItemFeatureDTO feature);
    }
}