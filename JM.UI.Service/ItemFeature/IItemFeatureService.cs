using JM.Infrastructure.Models;
using JM.UI.Entities.Model.ItemFeatures;

namespace JM.UI.Service.ItemFeature
{
    public interface IItemFeatureService
    {
        Task<IEnumerable<ItemFeatureDTO>> GetItemFeatures();
        Task<ResponseResult> SaveItemFeature(ItemFeatureDTO feature);
    }
}