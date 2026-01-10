using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Stores
{
    public interface IStoreRepository
    {
        Task<IEnumerable<StoreDTO>> GetStores();
        Task<StoreDTO?> GetStoreById(int id);
        Task DeleteStore(int id);
        Task<ResponseResult> SaveUpdateStore(StoreDTO store);
    }

}
