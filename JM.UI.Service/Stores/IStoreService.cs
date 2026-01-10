using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Stores
{
    public interface IStoreService
    {
        StoreDTO CreateNewStore();
        Task<ResponseResult> DeleteStore(int id);
        Task<StoreDTO?> GetStoreById(int id);
        Task<IEnumerable<StoreDTO>> GetStores();
        Task<ResponseResult> SaveUpdateStore(StoreDTO store);
        string Truncate(string? value, int maxChars);
        Task<(bool IsValid, string ErrorMessage)> ValidateStore(StoreDTO store);
    }
}