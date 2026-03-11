using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.ItemCatalogue;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.ItemCatalogue
{
    public class ItemCatalogueService : IItemCatalogueService
    {
        private readonly IRepositoryUnitOfWork _uow;

        public ItemCatalogueService(IRepositoryUnitOfWork uow) => _uow = uow;

        public Task<IEnumerable<ItemCatalogueDTO>> GetItemCatalogues()
            => _uow.ItemCatalogueRepository.GetItemCatalogues();

        public Task<ItemCatalogueDTO?> GetItemCatalogueById(int id)
            => _uow.ItemCatalogueRepository.GetItemCatalogueById(id);

        public async Task<ResponseResult> SaveItemCatalogue(ItemCatalogueDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CatalogueName))
                return new ResponseResult { IsSuccessStatus = false, Message = "Catalogue name is required." };

            if (dto.CatalogueName.Length > 200)
                return new ResponseResult { IsSuccessStatus = false, Message = "Catalogue name cannot exceed 200 characters." };

            return await _uow.ItemCatalogueRepository.SaveItemCatalogue(dto);
        }

        public async Task<ResponseResult> DeleteItemCatalogue(int id)
        {
            try
            {
                await _uow.ItemCatalogueRepository.DeleteItemCatalogue(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
