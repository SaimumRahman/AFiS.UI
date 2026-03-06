using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.ItemOrigin;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.ItemOrigin
{
    public class ItemOriginService : IItemOriginService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ItemOriginService(IRepositoryUnitOfWork repositoryUnitOfWork)
        {
            _repositoryUnitOfWork = repositoryUnitOfWork;
        }

        public async Task<IEnumerable<ItemOriginDTO>> GetItemOrigins()
        {
            return await _repositoryUnitOfWork.ItemOriginRepository.GetItemOrigins();
        }

        public async Task<ResponseResult> SaveItemOrigin(ItemOriginDTO origin)
        {
            if (string.IsNullOrWhiteSpace(origin.OriginName))
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = "Origin name is required."
                };
            }

            return await _repositoryUnitOfWork.ItemOriginRepository.SaveItemOrigin(origin);
        }
    }
}
