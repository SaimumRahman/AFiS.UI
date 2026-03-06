using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.ItemBrand;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.ItemBrand
{
    public class ItemBrandService : IItemBrandService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ItemBrandService(IRepositoryUnitOfWork repositoryUnitOfWork)
        {
            _repositoryUnitOfWork = repositoryUnitOfWork;
        }

        public async Task<IEnumerable<ItemBrandDTO>> GetItemBrands()
        {
            return await _repositoryUnitOfWork.ItemBrandRepository.GetItemBrands();
        }

        public async Task<ResponseResult> SaveItemBrand(ItemBrandDTO brand)
        {
            if (string.IsNullOrWhiteSpace(brand.BrandName))
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Brand name is required."
                };
            }

            return await _repositoryUnitOfWork.ItemBrandRepository.SaveItemBrand(brand);
        }
    }
}
